using PoE.dlls.Automation;
using PoE.dlls.Logger;

namespace PoE.dlls.Gamble
{
    internal static class GambleItemMatcher
    {
        private const int MaxUnchangedReads = 3;

        public static async Task<bool> TryMatchRulesAfterCopyAsync(
            Main main,
            InputSimulatorHost inputHost,
            TimeSpan delay,
            CancellationToken cancellationToken,
            CancellationTokenSource cts,
            GambleItemClipboardHelper.HashState hashState,
            IReadOnlyList<Rule> rules,
            bool requireHashChange = false,
            int? baselineHash = null)
        {
            string? content = await GambleItemClipboardHelper.CopyAndReadAsync(
                main,
                inputHost,
                delay,
                cancellationToken,
                baselineHash,
                requireHashChange);

            if (content is null)
            {
                GamblerLog.ClipboardEmptyWarning();
                cts.Cancel();
                return false;
            }

            if (!hashState.Register(content, MaxUnchangedReads, cts))
                return false;

            if (!GambleRuleEvaluator.MatchesRules(content, rules, logParse: false))
                return false;

            string? confirm = await GambleItemClipboardHelper.ConfirmMatchAsync(main, inputHost, delay, cancellationToken);
            if (confirm is null)
            {
                GamblerLog.ClipboardEmptyWarning();
                cts.Cancel();
                return false;
            }

            if (!GambleRuleEvaluator.MatchesRules(confirm, rules, logParse: true))
                return false;

            return true;
        }

        public static async Task<AltAugResponse> ReadAltAugAfterCopyAsync(
            Main main,
            InputSimulatorHost inputHost,
            TimeSpan delay,
            CancellationToken cancellationToken,
            CancellationTokenSource cts,
            GambleItemClipboardHelper.HashState hashState,
            IReadOnlyList<Rule> rules,
            bool requireHashChange = false,
            int? baselineHash = null)
        {
            string? content = await GambleItemClipboardHelper.CopyAndReadAsync(
                main,
                inputHost,
                delay,
                cancellationToken,
                baselineHash,
                requireHashChange);

            if (content is null)
            {
                GamblerLog.ClipboardEmptyWarning();
                cts.Cancel();
                return AltAugResponse.Failure;
            }

            if (!hashState.Register(content, MaxUnchangedReads, cts))
                return AltAugResponse.Failure;

            AltAugResponse response = GambleRuleEvaluator.EvaluateAltAug(content, rules, logParse: false);
            if (response != AltAugResponse.Success)
                return response;

            string? confirm = await GambleItemClipboardHelper.ConfirmMatchAsync(main, inputHost, delay, cancellationToken);
            if (confirm is null)
            {
                GamblerLog.ClipboardEmptyWarning();
                cts.Cancel();
                return AltAugResponse.Failure;
            }

            return GambleRuleEvaluator.EvaluateAltAug(confirm, rules, logParse: true);
        }
    }
}
