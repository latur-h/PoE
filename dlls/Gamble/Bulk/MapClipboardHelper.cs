using PoE.dlls.Automation;
using PoE.dlls.Logger;

namespace PoE.dlls.Gamble.Bulk
{
    internal static class MapClipboardHelper
    {
        private static readonly TimeSpan RetryCooldown = TimeSpan.FromMilliseconds(50);

        public static async Task<string?> CopyMapAsync(
            Main main,
            InputSimulatorHost inputHost,
            TimeSpan delay,
            CancellationToken cancellationToken)
        {
            string? content = await SendCopyAsync(main, inputHost, delay, cancellationToken);
            if (!string.IsNullOrWhiteSpace(content))
                return content;

            GamblerLog.Warn("Clipboard empty — retrying copy in 1s...");
            await Task.Delay(RetryCooldown, cancellationToken);
            return await SendCopyAsync(main, inputHost, delay, cancellationToken);
        }

        private static async Task<string?> SendCopyAsync(
            Main main,
            InputSimulatorHost inputHost,
            TimeSpan delay,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            string itemContent = main.Invoke(() => Clipboard.GetText(TextDataFormat.Text));
            if (string.IsNullOrWhiteSpace(itemContent))
                return null;

            main.Invoke(Clipboard.Clear);
            return itemContent;
        }
    }
}
