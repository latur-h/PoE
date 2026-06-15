using Poss.Win.Automation.Common.Keys.Enums;
using Poss.Win.Automation.Common.Structs;
using Poss.Win.Automation.Input;

namespace PoE.dlls.KeyBindings
{
    /// <summary>
    /// Resolves and validates key bindings once at load/bind time.
    /// Send paths must use the returned <see cref="sendKey"/> without re-parsing.
    /// </summary>
    internal static class KeyBindingHelper
    {
        public static bool TryResolveStored(string raw, out string sendKey, out string displayKey)
        {
            sendKey = string.Empty;
            displayKey = string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
                return false;

            raw = raw.Trim();

            if (!KeyStroke.TryParseKey(raw, out VirtualKey vk))
                return false;

            return TryFinalize(vk, out sendKey, out displayKey);
        }

        public static bool TryBindFromWinForms(System.Windows.Forms.Keys keyCode, out string sendKey, out string displayKey)
        {
            sendKey = string.Empty;
            displayKey = string.Empty;

            keyCode &= System.Windows.Forms.Keys.KeyCode;
            if (keyCode == System.Windows.Forms.Keys.None)
                return false;

            var vk = (VirtualKey)(ushort)keyCode;
            return TryFinalize(vk, out sendKey, out displayKey);
        }

        private static bool TryFinalize(VirtualKey vk, out string sendKey, out string displayKey)
        {
            sendKey = ToSendKey(vk);
            displayKey = ToDisplayKey(vk);

            return InputSimulator.TryParse($"{sendKey} Down", out _);
        }

        private static string ToSendKey(VirtualKey vk)
        {
            if (vk >= VirtualKey.D0 && vk <= VirtualKey.D9)
                return ((char)(ushort)vk).ToString();

            if (vk >= VirtualKey.A && vk <= VirtualKey.Z)
                return ((char)(ushort)vk).ToString();

            return vk.ToString();
        }

        private static string ToDisplayKey(VirtualKey vk)
        {
            if (vk >= VirtualKey.D0 && vk <= VirtualKey.D9)
                return ((char)(ushort)vk).ToString();

            if (vk >= VirtualKey.A && vk <= VirtualKey.Z)
                return ((char)(ushort)vk).ToString();

            return vk switch
            {
                VirtualKey.Semicolon => ";",
                VirtualKey.Equal => "=",
                VirtualKey.Comma => ",",
                VirtualKey.Minus => "-",
                VirtualKey.Period => ".",
                VirtualKey.Slash => "/",
                VirtualKey.Backtick => "`",
                VirtualKey.OpenBracket => "[",
                VirtualKey.Backslash => "\\",
                VirtualKey.CloseBracket => "]",
                VirtualKey.Quote => "'",
                _ => vk.ToString()
            };
        }
    }
}
