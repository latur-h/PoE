using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.Logger;
using System.Text.RegularExpressions;

namespace PoE.dlls.Gamble.Modes
{
    internal static class MapCheckHelper
    {
        public static bool TryEvaluateClipboard(
            Main main,
            CancellationTokenSource cts,
            string itemContent,
            IReadOnlyList<Rule> rules,
            ref int hash,
            ref int count,
            int maxAttempts,
            out MapRulesResult result)
        {
            result = default;

            if (string.IsNullOrEmpty(itemContent))
            {
                GamblerLog.ClipboardEmptyWarning();
                cts.Cancel();
                return false;
            }

            main.Invoke(Clipboard.Clear);

            int nextHash = itemContent.GetHashCode();
            if (hash != nextHash)
            {
                hash = nextHash;
            }
            else
            {
                if (count >= maxAttempts)
                {
                    GamblerLog.MaxAttemptsReached();
                    cts.Cancel();
                    return false;
                }

                count++;
            }

            result = MapRulesEvaluator.Evaluate(itemContent, rules);
            if (!result.IsMap)
            {
                GamblerLog.Warn("Item is not a map.");
                cts.Cancel();
                return false;
            }

            return true;
        }
    }
}
