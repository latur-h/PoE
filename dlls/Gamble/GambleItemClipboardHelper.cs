using PoE.dlls.Automation;
using PoE.dlls.Logger;

namespace PoE.dlls.Gamble
{
    internal static class GambleItemClipboardHelper
    {
        private const int MaxReadAttempts = 4;

        public sealed class HashState
        {
            public int Hash { get; private set; }

            private int _unchangedReads;

            public bool Register(string content, int maxUnchangedReads, CancellationTokenSource cts)
            {
                int nextHash = content.GetHashCode();
                if (Hash != nextHash)
                {
                    Hash = nextHash;
                    _unchangedReads = 0;
                    return true;
                }

                if (++_unchangedReads >= maxUnchangedReads)
                {
                    GamblerLog.MaxAttemptsReached();
                    cts.Cancel();
                    return false;
                }

                return true;
            }

            public void Reset()
            {
                Hash = 0;
                _unchangedReads = 0;
            }
        }

        public static async Task<string?> CopyAndReadAsync(
            Main main,
            InputSimulatorHost inputHost,
            TimeSpan delay,
            CancellationToken cancellationToken,
            int? baselineHash = null,
            bool requireHashChange = false)
        {
            TimeSpan settle = SettleDelay(delay);

            for (int attempt = 0; attempt < MaxReadAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (attempt > 0 || requireHashChange)
                    await Task.Delay(settle, cancellationToken);

                await SendCopyAsync(inputHost, delay, cancellationToken);
                await Task.Delay(settle, cancellationToken);

                string? content = ReadClipboardText(main);
                if (string.IsNullOrWhiteSpace(content))
                    continue;

                int hash = content.GetHashCode();
                if (requireHashChange && baselineHash.HasValue && hash == baselineHash.Value)
                    continue;

                return content;
            }

            return null;
        }

        public static async Task<string?> ConfirmMatchAsync(
            Main main,
            InputSimulatorHost inputHost,
            TimeSpan delay,
            CancellationToken cancellationToken)
        {
            TimeSpan settle = SettleDelay(delay);
            await Task.Delay(settle, cancellationToken);
            return await CopyAndReadAsync(main, inputHost, delay, cancellationToken);
        }

        private static TimeSpan SettleDelay(TimeSpan delay)
        {
            long ticks = Math.Max(delay.Ticks / 2, TimeSpan.FromMilliseconds(15).Ticks);
            return TimeSpan.FromTicks(ticks);
        }

        private static async Task SendCopyAsync(
            InputSimulatorHost inputHost,
            TimeSpan delay,
            CancellationToken cancellationToken)
        {
            inputHost.Simulator.Send("Ctrl Down");
            await Task.Delay(delay, cancellationToken);
            inputHost.Simulator.Send("Alt Down");
            await Task.Delay(delay, cancellationToken);
            inputHost.Simulator.Send("C Down");
            await Task.Delay(delay, cancellationToken);
            inputHost.Simulator.Send("C Up");
            await Task.Delay(delay, cancellationToken);
            inputHost.Simulator.Send("Alt Up");
            await Task.Delay(delay, cancellationToken);
            inputHost.Simulator.Send("Ctrl Up");
            await Task.Delay(delay, cancellationToken);
        }

        private static string? ReadClipboardText(Main main)
        {
            string itemContent = main.Invoke(() => Clipboard.GetText(TextDataFormat.Text));
            if (string.IsNullOrWhiteSpace(itemContent))
                return null;

            main.Invoke(Clipboard.Clear);
            return itemContent;
        }
    }
}
