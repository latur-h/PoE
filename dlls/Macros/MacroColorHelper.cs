using PoE.dlls.Settings.Macros;

namespace PoE.dlls.Macros
{
    public static class MacroColorHelper
    {
        public static bool TryParseHex(string? raw, out Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            string text = raw.Trim();
            if (!text.StartsWith('#'))
                text = "#" + text;

            if (text.Length != 7)
                return false;

            for (int i = 1; i < text.Length; i++)
            {
                char c = text[i];
                bool hex = c is >= '0' and <= '9'
                    or >= 'a' and <= 'f'
                    or >= 'A' and <= 'F';
                if (!hex)
                    return false;
            }

            int argb = Convert.ToInt32(text[1..], 16);
            color = Color.FromArgb(0xFF, (argb >> 16) & 0xFF, (argb >> 8) & 0xFF, argb & 0xFF);
            return true;
        }

        public static string ToHex(Color color) =>
            $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        public static bool MatchesStrict(Color sampled, Color expected) =>
            sampled.R == expected.R && sampled.G == expected.G && sampled.B == expected.B;

        public static void RememberColor(MacroSettings settings, string hex)
        {
            if (!TryParseHex(hex, out Color parsed))
                return;

            string normalized = ToHex(parsed);
            settings.RememberedColors ??= [];
            settings.RememberedColors.RemoveAll(c =>
                string.Equals(c, normalized, StringComparison.OrdinalIgnoreCase));
            settings.RememberedColors.Insert(0, normalized);

            const int maxRemembered = 32;
            if (settings.RememberedColors.Count > maxRemembered)
                settings.RememberedColors = settings.RememberedColors.Take(maxRemembered).ToList();
        }
    }
}
