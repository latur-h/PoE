using PoE.dlls.KeyBindings;
using PoE.dlls.Style;

namespace PoE.dlls.Macros.UI
{
    /// <summary>
    /// Debounced trigger/toggle key field. Supports typed names (including mouse buttons like XButton1).
    /// Invalid in-progress input is highlighted red without clearing the last valid send-key.
    /// </summary>
    internal sealed class MacroKeyFieldBinder : IDisposable
    {
        private const int DebounceMs = 300;

        private readonly FlatTextBox _box;
        private readonly Action<string> _setter;
        private readonly EventHandler? _changed;
        private readonly System.Windows.Forms.Timer _debounce;

        public MacroKeyFieldBinder(FlatTextBox box, Action<string> setter, EventHandler? changed = null)
        {
            _box = box;
            _setter = setter;
            _changed = changed;
            _debounce = new System.Windows.Forms.Timer { Interval = DebounceMs };
            _debounce.Tick += (_, _) => Validate();
            _box._textBox.TextChanged += OnTextChanged;
        }

        /// <summary>
        /// False while the text box has a non-empty value that does not resolve to a key.
        /// Macros must not arm until this is true again.
        /// </summary>
        public bool AllowsRuntime
        {
            get
            {
                string raw = _box._textBox.Text;
                if (string.IsNullOrWhiteSpace(raw))
                    return true;

                return KeyBindingHelper.TryResolveStored(raw, out _, out _);
            }
        }

        public void LoadFromStored(string storedSendKey)
        {
            if (KeyBindingHelper.TryResolveStored(storedSendKey, out string sendKey, out string displayKey))
            {
                _setter(sendKey);
                _box._textBox.Text = displayKey;
                _box._textBox.ForeColor = StaticColors.ForeGround;
                return;
            }

            if (!string.IsNullOrWhiteSpace(storedSendKey))
            {
                _setter(storedSendKey.Trim());
                _box._textBox.Text = storedSendKey.Trim();
                _box._textBox.ForeColor = Color.Red;
                return;
            }

            _box._textBox.Text = string.Empty;
            _box._textBox.ForeColor = StaticColors.ForeGround;
            _setter(string.Empty);
        }

        private void OnTextChanged(object? sender, EventArgs e)
        {
            _debounce.Stop();
            _debounce.Start();
        }

        private void Validate()
        {
            _debounce.Stop();
            string raw = _box._textBox.Text;

            if (string.IsNullOrWhiteSpace(raw))
            {
                _setter(string.Empty);
                _box._textBox.ForeColor = StaticColors.ForeGround;
                _changed?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (KeyBindingHelper.TryResolveStored(raw, out string sendKey, out string displayKey))
            {
                _setter(sendKey);
                if (!string.Equals(_box._textBox.Text, displayKey, StringComparison.Ordinal))
                    _box._textBox.Text = displayKey;
                _box._textBox.ForeColor = StaticColors.ForeGround;
            }
            else
            {
                _box._textBox.ForeColor = Color.Red;
            }

            _changed?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose() => _debounce.Dispose();
    }
}
