using PoE.dlls.Automation;
using PoE.dlls.Gamble.Bulk;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.InteropServices;
using PoE.dlls.Logger;

namespace PoE.dlls.Gamble.Modes
{
    internal static class MapCorruptHelper
    {
        public static bool IsVaalConfigured(Coordinates vaal) => vaal.X > 0 || vaal.Y > 0;

        /// <summary>
        /// Runs optional Vaal corrupt + re-eval after rules already passed.
        /// Returns true when the map is kept, false when broken after Vaal, null when cancelled.
        /// </summary>
        public static async Task<bool?> TryFinishWithOptionalCorruptAsync(
            Main main,
            InputSimulatorHost inputHost,
            CancellationToken cancellationToken,
            TimeSpan delay,
            double speed,
            Coordinates item,
            Coordinates vaal,
            bool corruptOnSuccess,
            bool corruptRequireEightMods,
            List<Rule> rules)
        {
            if (!corruptOnSuccess || !IsVaalConfigured(vaal))
                return true;

            cancellationToken.ThrowIfCancellationRequested();

            inputHost.Simulator.MouseDeltaMove(vaal.X, vaal.Y, speed);
            await Task.Delay(delay, cancellationToken);

            inputHost.Simulator.Send("Shift Down");
            await Task.Delay(delay, cancellationToken);

            inputHost.Simulator.Send("RButton Down");
            await Task.Delay(delay, cancellationToken);
            inputHost.Simulator.Send("RButton Up");
            await Task.Delay(delay, cancellationToken);

            inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay, cancellationToken);

            inputHost.Simulator.Send("LButton Down");
            await Task.Delay(delay, cancellationToken);
            inputHost.Simulator.Send("LButton Up");
            await Task.Delay(delay, cancellationToken);

            inputHost.Simulator.Send("Shift Up");
            await Task.Delay(delay, cancellationToken);

            string? content = await MapClipboardHelper.CopyMapAsync(main, inputHost, delay, cancellationToken);
            if (content is null)
                return null;

            var evaluationRules = MapCorruptRulesHelper.RulesForPostCorruptEvaluation(
                rules,
                corruptRequireEightMods);
            var evaluation = MapRulesEvaluator.Evaluate(content, evaluationRules);
            if (evaluation.RulesPassed)
                return true;

            GamblerLog.Warn("Broken map");
            return false;
        }
    }
}
