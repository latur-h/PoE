using PoE.dlls.Settings.Notes;
using PoE.dlls.Style;
using PoE.dlls.UI;
using PoE.dlls.UI.Markdown;

namespace PoE.dlls.Notes.UI
{
    public sealed class NotesPanel : UserControl
    {
        private readonly ProfileSelectorBar _profileBar;
        private readonly MarkdownDocumentEditor _editor;
        private readonly Label _helpLabel;

        private NotesSettings? _settings;

        public NotesPanel()
        {
            BackColor = StaticColors.BackGround;

            _profileBar = new ProfileSelectorBar
            {
                Dock = DockStyle.Top,
                Height = 34,
            };
            _profileBar.SelectionChanging += (_, _) => CommitActiveProfile();
            _profileBar.SelectionChanged += (_, _) => LoadActiveProfile();

            _helpLabel = new Label
            {
                AutoSize = true,
                Text = "ⓘ",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Cursor = Cursors.Hand,
            };

            _editor = new MarkdownDocumentEditor
            {
                Dock = DockStyle.Fill,
            };
            _editor.MarkdownChanged += (_, _) => OnEditorChanged();

            Controls.Add(_editor);
            Controls.Add(_helpLabel);
            Controls.Add(_profileBar);

            Resize += (_, _) => LayoutHelpIcon();
        }

        public void SetupHints(ToolTip toolTip, IWin32Window helpOwner)
        {
            _helpLabel.Click += (_, _) => NotesHelpDialog.ShowHelp(helpOwner);
            toolTip.SetToolTip(_helpLabel, NotesHelp.Short.OpenHelp);
            _profileBar.AttachHints(
                toolTip,
                NotesHelp.Short.Profiles,
                NotesHelp.Short.AddProfile,
                NotesHelp.Short.RemoveProfile);
            _editor.AttachHints(toolTip, NotesHelp.Short.Editor, NotesHelp.Short.CopyChips);
            LayoutHelpIcon();
        }

        private void LayoutHelpIcon()
        {
            _helpLabel.Location = new Point(Math.Max(0, Width - 102), 4);
            _helpLabel.BringToFront();
        }

        public event EventHandler? Changed;

        public void Bind(NotesSettings settings)
        {
            NotesSettingsHelper.EnsureInitialized(settings);
            _settings = settings;

            _profileBar.Bind(new ProfileSelectorBinding
            {
                LabelText = "Profile",
                MaxProfiles = NotesSettingsHelper.MaxProfiles,
                GetProfileNames = () => _settings!.Profiles.Select(p => p.Name).ToList(),
                GetActiveProfileName = () => _settings!.ActiveProfileName,
                SetActiveProfileName = name => _settings!.ActiveProfileName = name,
                SuggestNewProfileName = () => NotesSettingsHelper.SuggestNewProfileName(_settings!.Profiles),
                IsProfileNameAvailable = name => NotesSettingsHelper.IsProfileNameAvailable(name, _settings!.Profiles),
                CanRemoveProfile = name => NotesSettingsHelper.CanRemoveProfile(_settings!, name),
                AddProfile = name =>
                {
                    _settings!.Profiles.Add(new NotesProfile { Name = name });
                    _settings.ActiveProfileName = name;
                },
                RemoveProfile = name =>
                {
                    NotesProfile? profile = NotesSettingsHelper.GetProfileByName(_settings!, name);
                    if (profile is null)
                        return;

                    _settings!.Profiles.Remove(profile);
                    if (string.Equals(_settings.ActiveProfileName, name, StringComparison.OrdinalIgnoreCase))
                        _settings.ActiveProfileName = _settings.Profiles[0].Name;
                },
            });

            LoadActiveProfile();
        }

        public void Commit()
        {
            CommitActiveProfile();
        }

        private void LoadActiveProfile()
        {
            if (_settings is null)
                return;

            NotesProfile profile = NotesSettingsHelper.GetActiveProfile(_settings);
            _editor.Markdown = profile.Markdown;
        }

        private void CommitActiveProfile()
        {
            if (_settings is null)
                return;

            _editor.CommitEdit();
            NotesProfile profile = NotesSettingsHelper.GetActiveProfile(_settings);
            profile.Markdown = _editor.Markdown;
        }

        private void OnEditorChanged()
        {
            if (_settings is null)
                return;

            NotesProfile profile = NotesSettingsHelper.GetActiveProfile(_settings);
            profile.Markdown = _editor.Markdown;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
