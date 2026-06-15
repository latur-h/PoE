using PoE.dlls.Settings.Macros;
using PoE.dlls.Style;

namespace PoE.dlls.Macros.UI
{
    public sealed class MacroProfileBar : UserControl
    {
        private static readonly Font UiFont = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);

        private readonly Label _label;
        private readonly FlatComboBox _combo;
        private readonly Label _runtimeHint;
        private readonly Button _addButton;
        private readonly Button _removeButton;

        private MacroSettings? _macros;
        private bool _suppressEvents;

        public MacroProfileBar()
        {
            BackColor = StaticColors.BackGround;
            Height = 34;

            _label = new Label
            {
                AutoSize = true,
                Location = new Point(0, 6),
                Text = "Profile",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = UiFont,
            };

            _combo = new FlatComboBox
            {
                Location = new Point(68, 2),
                Size = new Size(180, 30),
                Font = UiFont,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            _combo.SelectedIndexChanged += (_, _) => OnSelectionChanged();

            _runtimeHint = new Label
            {
                AutoSize = true,
                Location = new Point(256, 8),
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = new Font("Segoe UI", 10F),
            };

            _addButton = CreateIconButton("+", 0);
            _addButton.Click += (_, _) => AddProfile();

            _removeButton = CreateIconButton("−", 0);
            _removeButton.Click += (_, _) => RemoveProfile();

            Controls.Add(_label);
            Controls.Add(_combo);
            Controls.Add(_runtimeHint);
            Controls.Add(_addButton);
            Controls.Add(_removeButton);

            Resize += (_, _) => LayoutControls();
        }

        public event EventHandler? ProfileChanging;
        public event EventHandler<MacroProfile>? ProfileRemoved;
        public event EventHandler? ProfileChanged;

        public string SelectedProfileName { get; private set; } = MacroProfile.GlobalName;

        public void Bind(MacroSettings macros) => _macros = macros;

        public MacroProfile? GetSelectedProfile()
        {
            if (_macros is null)
                return null;

            return MacroSettingsHelper.GetProfileByName(_macros, SelectedProfileName);
        }

        public void RefreshProfiles()
        {
            if (_macros is null)
                return;

            SelectedProfileName = ResolveSelectedProfileName(_macros);

            _suppressEvents = true;
            _combo.BeginUpdate();
            try
            {
                _combo.Items.Clear();
                _combo.Items.Add(MacroProfile.GlobalName);
                foreach (MacroProfile profile in _macros.BuildProfiles)
                    _combo.Items.Add(profile.Name);

                _combo.SelectedItem = SelectedProfileName;
            }
            finally
            {
                _combo.EndUpdate();
            }

            UpdateRuntimeHint();
            UpdateRemoveButtonState();
            LayoutControls();
            _suppressEvents = false;
        }

        private static string ResolveSelectedProfileName(MacroSettings macros)
        {
            if (MacroSettingsHelper.IsAdditionalBuildProfileActive(macros))
                return macros.ActiveBuildProfileName;

            return MacroProfile.GlobalName;
        }

        private void OnSelectionChanged()
        {
            if (_suppressEvents || _macros is null || _combo.SelectedItem is not string name)
                return;

            if (string.Equals(SelectedProfileName, name, StringComparison.OrdinalIgnoreCase))
                return;

            ProfileChanging?.Invoke(this, EventArgs.Empty);

            SelectedProfileName = name;
            _macros.ActiveBuildProfileName = string.Equals(name, MacroProfile.GlobalName, StringComparison.OrdinalIgnoreCase)
                ? MacroProfile.GlobalName
                : name;

            UpdateRuntimeHint();
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
            SelectedProfileName = profile.Name;
            RefreshProfiles();
            ProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveProfile()
        {
            if (_macros is null || !CanRemoveSelectedProfile())
                return;

            var profile = MacroSettingsHelper.GetProfileByName(_macros, SelectedProfileName);
            if (profile is null || ReferenceEquals(profile, _macros.GlobalProfile))
                return;

            ProfileChanging?.Invoke(this, EventArgs.Empty);
            _macros.BuildProfiles.Remove(profile);
            ProfileRemoved?.Invoke(this, profile);

            _macros.ActiveBuildProfileName = MacroProfile.GlobalName;
            SelectedProfileName = MacroProfile.GlobalName;
            RefreshProfiles();
            ProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateRuntimeHint()
        {
            if (_macros is null)
            {
                _runtimeHint.Text = string.Empty;
                return;
            }

            _runtimeHint.Text = MacroSettingsHelper.IsAdditionalBuildProfileActive(_macros)
                ? $"Runtime: Global + { _macros.ActiveBuildProfileName }"
                : "Runtime: Global only";
        }

        private void UpdateRemoveButtonState()
        {
            bool canRemove = CanRemoveSelectedProfile();
            _removeButton.Enabled = canRemove;
            _removeButton.ForeColor = canRemove ? StaticColors.ForeGround : Color.Gray;
        }

        private bool CanRemoveSelectedProfile()
        {
            if (_macros is null)
                return false;

            return !string.Equals(SelectedProfileName, MacroProfile.GlobalName, StringComparison.OrdinalIgnoreCase);
        }

        private void LayoutControls()
        {
            int width = Math.Max(360, ClientSize.Width);
            _addButton.Location = new Point(width - 68, 2);
            _removeButton.Location = new Point(width - 34, 2);

            int hintRight = _addButton.Left - 8;
            _runtimeHint.MaximumSize = new Size(Math.Max(120, hintRight - _runtimeHint.Left), 0);
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
