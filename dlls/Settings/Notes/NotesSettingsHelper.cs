namespace PoE.dlls.Settings.Notes
{
    public static class NotesSettingsHelper
    {
        public const int MaxProfiles = 100;

        public static void EnsureInitialized(NotesSettings settings)
        {
            settings.Profiles ??= [];
            NormalizeProfiles(settings);

            if (settings.Profiles.Count == 0)
            {
                settings.Profiles.Add(CreateDefaultProfile());
                settings.ActiveProfileName = NotesSettings.DefaultProfileName;
            }
        }

        public static NotesProfile? GetProfileByName(NotesSettings settings, string name) =>
            settings.Profiles.FirstOrDefault(p =>
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

        public static NotesProfile GetActiveProfile(NotesSettings settings)
        {
            EnsureInitialized(settings);
            return GetProfileByName(settings, settings.ActiveProfileName)
                ?? settings.Profiles[0];
        }

        public static string SuggestNewProfileName(IEnumerable<NotesProfile> profiles)
        {
            var names = profiles.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 2; i <= MaxProfiles + 1; i++)
            {
                string candidate = $"Profile {i}";
                if (!names.Contains(candidate))
                    return candidate;
            }

            return "New profile";
        }

        public static bool IsProfileNameAvailable(string name, IEnumerable<NotesProfile> profiles)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            string trimmed = name.Trim();
            return !profiles.Any(p => string.Equals(p.Name, trimmed, StringComparison.OrdinalIgnoreCase));
        }

        public static bool CanRemoveProfile(NotesSettings settings, string name)
        {
            if (settings.Profiles.Count <= 1)
                return false;

            return GetProfileByName(settings, name) is not null;
        }

        private static void NormalizeProfiles(NotesSettings settings)
        {
            settings.Profiles.RemoveAll(p => p is null);

            foreach (NotesProfile profile in settings.Profiles)
            {
                profile.Name = string.IsNullOrWhiteSpace(profile.Name)
                    ? SuggestNewProfileName(settings.Profiles)
                    : profile.Name.Trim();
                profile.Markdown ??= string.Empty;
            }

            var unique = new Dictionary<string, NotesProfile>(StringComparer.OrdinalIgnoreCase);
            foreach (NotesProfile profile in settings.Profiles)
            {
                if (!unique.TryGetValue(profile.Name, out NotesProfile? existing))
                {
                    unique[profile.Name] = profile;
                    continue;
                }

                if (profile.Markdown.Length > existing.Markdown.Length)
                    unique[profile.Name] = profile;
            }

            settings.Profiles = unique.Values
                .OrderBy(p => string.Equals(p.Name, NotesSettings.DefaultProfileName, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (settings.Profiles.Count > MaxProfiles)
                settings.Profiles = settings.Profiles.Take(MaxProfiles).ToList();

            if (string.IsNullOrWhiteSpace(settings.ActiveProfileName)
                || GetProfileByName(settings, settings.ActiveProfileName) is null)
            {
                settings.ActiveProfileName = settings.Profiles.Count > 0
                    ? settings.Profiles[0].Name
                    : NotesSettings.DefaultProfileName;
            }
            else
            {
                settings.ActiveProfileName = settings.Profiles.First(p =>
                    string.Equals(p.Name, settings.ActiveProfileName, StringComparison.OrdinalIgnoreCase)).Name;
            }
        }

        private static NotesProfile CreateDefaultProfile() => new()
        {
            Name = NotesSettings.DefaultProfileName,
            Markdown = string.Empty,
        };
    }
}
