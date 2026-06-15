using PoE.dlls.Settings.Mods;
using PoE.dlls.Style;

namespace PoE.dlls.Gamble.UI
{
    public sealed class GamblePresetBar : UserControl
    {
        private static readonly Font UiFont = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);

        private readonly Label _label;
        private readonly FlatComboBox _combo;
        private readonly Button _addButton;
        private readonly Button _removeButton;

        private UIModifiers? _modifiers;
        private bool _suppressEvents;

        public GamblePresetBar()
        {
            BackColor = StaticColors.BackGround;
            Height = 34;

            _label = new Label
            {
                AutoSize = true,
                Location = new Point(0, 6),
                Text = "Preset",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = UiFont,
            };

            _combo = new FlatComboBox
            {
                Location = new Point(62, 2),
                Size = new Size(200, 30),
                Font = UiFont,
            };
            _combo.SelectedIndexChanged += (_, _) => OnPresetSelectionChanged();

            _addButton = CreateIconButton("+", 268);
            _addButton.Click += (_, _) => AddPreset();

            _removeButton = CreateIconButton("−", 304);
            _removeButton.Click += (_, _) => RemovePreset();

            Controls.Add(_label);
            Controls.Add(_combo);
            Controls.Add(_addButton);
            Controls.Add(_removeButton);
        }

        public event EventHandler? PresetChanging;
        public event EventHandler<GamblePreset>? PresetRemoved;
        public event EventHandler? PresetChanged;

        public void Bind(UIModifiers modifiers) => _modifiers = modifiers;

        public void RefreshPresets()
        {
            if (_modifiers is null)
                return;

            _suppressEvents = true;

            var store = _modifiers.GetModeStore(_modifiers.GambleType);
            _combo.BeginUpdate();
            try
            {
                _combo.Items.Clear();
                foreach (GamblePreset preset in store.Presets)
                    _combo.Items.Add(preset.Name);

                _combo.SelectedItem = store.ActivePresetName;
            }
            finally
            {
                _combo.EndUpdate();
            }

            UpdateRemoveButtonState(store);

            _suppressEvents = false;
        }

        private void OnPresetSelectionChanged()
        {
            if (_suppressEvents || _modifiers is null || _combo.SelectedItem is not string name)
                return;

            var store = _modifiers.GetModeStore(_modifiers.GambleType);
            if (string.Equals(store.ActivePresetName, name, StringComparison.OrdinalIgnoreCase))
                return;

            PresetChanging?.Invoke(this, EventArgs.Empty);
            store.ActivePresetName = name;
            UpdateRemoveButtonState(store);
            PresetChanged?.Invoke(this, EventArgs.Empty);
        }

        private void AddPreset()
        {
            if (_modifiers is null)
                return;

            var store = _modifiers.GetModeStore(_modifiers.GambleType);
            PresetChanging?.Invoke(this, EventArgs.Empty);

            using var dialog = new GamblePresetNameDialog(
                store.Presets,
                GamblePresetHelper.SuggestNewPresetName(store.Presets));

            if (dialog.ShowDialog(FindForm()) != DialogResult.OK || dialog.PresetName is null)
                return;

            var preset = new GamblePreset
            {
                Name = dialog.PresetName,
                Rules = [],
            };

            store.Presets.Add(preset);
            store.ActivePresetName = preset.Name;
            RefreshPresets();
            PresetChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RemovePreset()
        {
            if (_modifiers is null)
                return;

            var store = _modifiers.GetModeStore(_modifiers.GambleType);
            if (store.Presets.Count <= 1)
                return;

            var active = store.Presets.FirstOrDefault(p => p.Name == store.ActivePresetName);
            if (active is null || string.Equals(active.Name, GamblePreset.DefaultName, StringComparison.Ordinal))
                return;

            PresetChanging?.Invoke(this, EventArgs.Empty);

            store.Presets.Remove(active);
            PresetRemoved?.Invoke(this, active);

            if (store.Presets.All(p => p.Name != store.ActivePresetName))
            {
                store.ActivePresetName = store.Presets.FirstOrDefault(p =>
                    string.Equals(p.Name, GamblePreset.DefaultName, StringComparison.Ordinal))?.Name
                    ?? store.Presets[0].Name;
            }

            RefreshPresets();
            PresetChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateRemoveButtonState(GambleModeStore store)
        {
            bool canRemove = store.Presets.Count > 1
                && !string.Equals(store.ActivePresetName, GamblePreset.DefaultName, StringComparison.Ordinal);

            _removeButton.Enabled = canRemove;
            _removeButton.ForeColor = canRemove ? StaticColors.ForeGround : Color.Gray;
        }

        private static Button CreateIconButton(string text, int x) =>
            new()
            {
                Location = new Point(x, 2),
                Size = new Size(30, 30),
                Text = text,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = Padding.Empty,
                FlatStyle = FlatStyle.Flat,
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point),
                Cursor = Cursors.Hand,
                TabStop = false,
                UseCompatibleTextRendering = true,
            };
    }
}
