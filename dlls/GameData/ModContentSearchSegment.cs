namespace PoE.dlls.GameData
{
    internal readonly record struct ModContentSearchSegment(int Start, int Length, string Phrase)
    {
        public int End => Start + Length;

        public static ModContentSearchSegment Resolve(string text, int caret)
        {
            if (string.IsNullOrEmpty(text))
                return new ModContentSearchSegment(0, 0, string.Empty);

            if (!text.Contains('|'))
                return new ModContentSearchSegment(0, text.Length, text.Trim());

            int clampedCaret = Math.Clamp(caret, 0, text.Length);
            var parts = new List<(int Start, int End)>();
            int partStart = 0;
            for (int i = 0; i <= text.Length; i++)
            {
                if (i < text.Length && text[i] != '|')
                    continue;

                parts.Add((partStart, i));
                partStart = i + 1;
            }

            for (int i = 0; i < parts.Count; i++)
            {
                (int start, int end) = parts[i];
                if (clampedCaret >= start && clampedCaret <= end)
                    return new ModContentSearchSegment(start, end - start, text[start..end].Trim());
            }

            for (int i = 0; i < parts.Count - 1; i++)
            {
                if (clampedCaret == parts[i].End)
                    return new ModContentSearchSegment(parts[i + 1].Start, parts[i + 1].End - parts[i + 1].Start, text[parts[i + 1].Start..parts[i + 1].End].Trim());
            }

            (int lastStart, int lastEnd) = parts[^1];
            return new ModContentSearchSegment(lastStart, lastEnd - lastStart, text[lastStart..lastEnd].Trim());
        }

        public static void ReplaceActiveSegment(TextBox textBox, string replacement)
        {
            string text = textBox.Text;
            int caret = textBox.SelectionStart;
            ModContentSearchSegment segment = Resolve(text, caret);
            textBox.Text = ReplaceSegment(text, caret, replacement);
            textBox.SelectionStart = !text.Contains('|') ? replacement.Length : segment.Start + replacement.Length;
            textBox.SelectionLength = 0;
        }

        public static string ReplaceSegment(string text, int caret, string replacement)
        {
            ModContentSearchSegment segment = Resolve(text, caret);
            if (!text.Contains('|'))
                return replacement;

            return text[..segment.Start] + replacement + text[segment.End..];
        }
    }
}
