using PoE.dlls.Settings.Macros;
using PoE.dlls.Style;

namespace PoE.dlls.Macros.UI
{
    public sealed class MacroBuildPresetBar : UserControl
    {
        private static readonly Font UiFont = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);

        private readonly Label _label;
        private readonly FlatComboBox _combo;
        private readonly Button _addButton;
        private readonly Button _removeButton;

        private MacroSettings? _macros;
        private bool _suppressEvents;

        public MacroBuildPresetBar()
        {
            BackColor = StaticColors.BackGround;
            Height = 34;

            _label = new Label
            {
                AutoSize = true,
                Location = new Point(0, 6),
                Text = "Build profile",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = UiFont,
            };

            _combo = new FlatComboBox
            {
                Location = new Point(108, 2),
                Size = new Size(200, 30),
                Font = UiFont,
            };
            _combo.SelectedIndexChanged += (_, _) => OnSelectionChanged();

            _addButton = CreateIconButton("+", 316);
            _addButton.Click += (_, _) => AddProfile();

            _removeButton = CreateIconButton("−", 352);
            _removeButton.Click += (_, _) => RemoveProfile();

            Controls.Add(_label);
            Controls.Add(_combo);
            Controls.Add(_addButton);
            Controls.Add(_removeButton);
        }

        public event EventHandler? ProfileChanging;
        public event EventHandler<MacroProfile>? ProfileRemoved;
        public event EventHandler? ProfileChanged;

        public void Bind(MacroSettings macros) => _macros = macros;

        public void RefreshProfiles()
        {
            if (_macros is null)
                return;

            _suppressEvents = true;
            _combo.BeginUpdate();
            try
            {
                _combo.Items.Clear();
                foreach (MacroProfile profile in _macros.BuildProfiles)
                    _combo.Items.Add(profile.Name);

                _combo.SelectedItem = _macros.ActiveBuildProfileName;
            }
            finally
            {
                _combo.EndUpdate();
            }

            UpdateRemoveButtonState();
            _suppressEvents = false;
        }

        private void OnSelectionChanged()
        {
            if (_suppressEvents || _macros is null || _combo.SelectedItem is not string name)
                return;

            if (string.Equals(_macros.ActiveBuildProfileName, name, StringComparison.OrdinalIgnoreCase))
                return;

            ProfileChanging?.Invoke(this, EventArgs.Empty);
            _macros.ActiveBuildProfileName = name;
            UpdateRemoveButtonState();
            ProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        private void AddProfile()
        {
            if (_macros is null || _macros.BuildProfiles.Count >= MacroSettingsHelper.MaxBuildProfiles)
                return;

            ProfileChanging?.Invoke(this, EventArgs.Empty);

            using var dialog = new MacroProfileNameDialog(
                _macros.BuildProfiles,
                MacroSettingsHelper.SuggestNewBuildProfileName(_macros.BuildProfiles));

            if (dialog.ShowDialog(FindForm()) != DialogResult.OK || dialog.ProfileName is null)
                return;

            var profile = new MacroProfile
            {
                Name = dialog.ProfileName,
                Triggers = [],
            };

            _macros.BuildProfiles.Add(profile);
            _macros.ActiveBuildProfileName = profile.Name;
            RefreshProfiles();
            ProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveProfile()
        {
            if (_macros is null || _macros.BuildProfiles.Count <= 1)
                return;

            var active = _macros.BuildProfiles.FirstOrDefault(p =>
                string.Equals(p.Name, _macros.ActiveBuildProfileName, StringComparison.OrdinalIgnoreCase));

            if (active is null ||
                string.Equals(active.Name, MacroProfile.DefaultBuildName, StringComparison.OrdinalIgnoreCase))
                return;

            ProfileChanging?.Invoke(this, EventArgs.Empty);
            _macros.BuildProfiles.Remove(active);
            ProfileRemoved?.Invoke(this, active);

            if (_macros.BuildProfiles.All(p =>
                    !string.Equals(p.Name, _macros.ActiveBuildProfileName, StringComparison.OrdinalIgnoreCase)))
            {
                _macros.ActiveBuildProfileName = _macros.BuildProfiles.First(p =>
                    string.Equals(p.Name, MacroProfile.DefaultBuildName, StringComparison.OrdinalIgnoreCase)).Name;
            }

            RefreshProfiles();
            ProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateRemoveButtonState()
        {
            if (_macros is null)
                return;

            bool canRemove = _macros.BuildProfiles.Count > 1
                && !string.Equals(_macros.ActiveBuildProfileName, MacroProfile.DefaultBuildName, StringComparison.OrdinalIgnoreCase);

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
            };
    }
}
