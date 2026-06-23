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

            _editor = new MarkdownDocumentEditor
            {
                Dock = DockStyle.Fill,
            };
            _editor.MarkdownChanged += (_, _) => OnEditorChanged();

            Controls.Add(_editor);
            Controls.Add(_profileBar);
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
