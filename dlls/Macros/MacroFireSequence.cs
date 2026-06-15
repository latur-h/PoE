using Poss.Win.Automation.Input;

namespace PoE.dlls.Macros
{
    public static class MacroFireSequence
    {
        public static IReadOnlyList<string> ParseStrokes(string? sequence)
        {
            if (string.IsNullOrWhiteSpace(sequence))
                return [];

            return sequence
                .Split(['\r', '\n', '+'], StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => part.Length > 0)
                .ToList();
        }

        public static IReadOnlyList<string> ParseLines(string? sequence) => ParseStrokes(sequence);

        public static bool IsValid(string? sequence)
        {
            var strokes = ParseStrokes(sequence);
            if (strokes.Count == 0)
                return false;

            return strokes.All(stroke => InputSimulator.TryParse(stroke, out _));
        }

        public static async Task ExecuteAsync(
            InputSimulator simulator,
            string? sequence,
            int keyDelayMs,
            CancellationToken cancellationToken = default)
        {
            foreach (string stroke in ParseStrokes(sequence))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!InputSimulator.TryParse(stroke, out _))
                    continue;

                simulator.Send(stroke);

                if (keyDelayMs > 0)
                    await Task.Delay(keyDelayMs, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
