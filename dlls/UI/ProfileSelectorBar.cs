using PoE.dlls.Style;

namespace PoE.dlls.UI
{
    public sealed class ProfileSelectorBinding
    {
        public required Func<IReadOnlyList<string>> GetProfileNames { get; init; }

        public required Func<string> GetActiveProfileName { get; init; }

        public required Action<string> SetActiveProfileName { get; init; }

        public required Func<string> SuggestNewProfileName { get; init; }

        public required Func<string, bool> IsProfileNameAvailable { get; init; }

        public required Func<string, bool> CanRemoveProfile { get; init; }

        public required Action<string> AddProfile { get; init; }

        public required Action<string> RemoveProfile { get; init; }

        public int MaxProfiles { get; init; } = 100;

        public string LabelText { get; init; } = "Profile";
    }

    public sealed class ProfileSelectorBar : UserControl
    {
        private static readonly Font UiFont = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);

        private readonly Label _label;
        private readonly FlatComboBox _combo;
        private readonly Button _addButton;
        private readonly Button _removeButton;

        private ProfileSelectorBinding? _binding;
        private bool _suppressEvents;

        public ProfileSelectorBar()
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
                Size = new Size(220, 30),
                Font = UiFont,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            _combo.SelectedIndexChanged += (_, _) => OnSelectionChanged();

            _addButton = CreateIconButton("+");
            _addButton.Click += (_, _) => AddProfile();

            _removeButton = CreateIconButton("−");
            _removeButton.Click += (_, _) => RemoveProfile();

            Controls.Add(_label);
            Controls.Add(_combo);
            Controls.Add(_addButton);
            Controls.Add(_removeButton);

            Resize += (_, _) => LayoutControls();
        }

        public event EventHandler? SelectionChanging;

        public event EventHandler? SelectionChanged;

        public string SelectedProfileName { get; private set; } = string.Empty;

        public void Bind(ProfileSelectorBinding binding)
        {
            _binding = binding;
            _label.Text = binding.LabelText;
            RefreshProfiles();
        }

        public void RefreshProfiles()
        {
            if (_binding is null)
                return;

            SelectedProfileName = _binding.GetActiveProfileName();

            _suppressEvents = true;
            _combo.BeginUpdate();
            try
            {
                _combo.Items.Clear();
                foreach (string name in _binding.GetProfileNames())
                    _combo.Items.Add(name);

                _combo.SelectedItem = _binding.GetProfileNames()
                    .FirstOrDefault(n => string.Equals(n, SelectedProfileName, StringComparison.OrdinalIgnoreCase))
                    ?? (_combo.Items.Count > 0 ? _combo.Items[0] : null);
            }
            finally
            {
                _combo.EndUpdate();
            }

            UpdateRemoveButtonState();
            LayoutControls();
            _suppressEvents = false;
        }

        private void OnSelectionChanged()
        {
            if (_suppressEvents || _binding is null || _combo.SelectedItem is not string name)
                return;

            if (string.Equals(SelectedProfileName, name, StringComparison.OrdinalIgnoreCase))
                return;

            SelectionChanging?.Invoke(this, EventArgs.Empty);

            SelectedProfileName = name;
            _binding.SetActiveProfileName(name);
            UpdateRemoveButtonState();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void AddProfile()
        {
            if (_binding is null || _binding.GetProfileNames().Count >= _binding.MaxProfiles)
                return;

            SelectionChanging?.Invoke(this, EventArgs.Empty);

            using var dialog = new ProfileNameDialog(
                "New profile",
                _binding.SuggestNewProfileName(),
                name => _binding.IsProfileNameAvailable(name)
                    ? null
                    : string.IsNullOrWhiteSpace(name)
                        ? "Name is required."
                        : "That name is already in use.");

            if (dialog.ShowDialog(FindForm()) != DialogResult.OK || dialog.ProfileName is null)
                return;

            _binding.AddProfile(dialog.ProfileName);
            SelectedProfileName = dialog.ProfileName;
            RefreshProfiles();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveProfile()
        {
            if (_binding is null || !_binding.CanRemoveProfile(SelectedProfileName))
                return;

            SelectionChanging?.Invoke(this, EventArgs.Empty);
            string removed = SelectedProfileName;
            _binding.RemoveProfile(removed);
            SelectedProfileName = _binding.GetActiveProfileName();
            RefreshProfiles();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateRemoveButtonState()
        {
            bool canRemove = _binding?.CanRemoveProfile(SelectedProfileName) == true;
            _removeButton.Enabled = canRemove;
            _removeButton.ForeColor = canRemove ? StaticColors.ForeGround : Color.Gray;
        }

        private void LayoutControls()
        {
            int width = Math.Max(280, ClientSize.Width);
            _addButton.Location = new Point(width - 68, 2);
            _removeButton.Location = new Point(width - 34, 2);
            _combo.Width = Math.Max(120, _addButton.Left - _combo.Left - 8);
        }

        private static Button CreateIconButton(string text) =>
            new()
            {
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
