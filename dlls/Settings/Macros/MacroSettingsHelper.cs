namespace PoE.dlls.Settings.Macros
{
    public static class MacroSettingsHelper
    {
        public const int MaxBuildProfiles = 100;

        public static void EnsureInitialized(MacroSettings settings)
        {
            settings.GlobalProfile ??= CreateDefaultGlobalProfile();
            settings.GlobalProfile.Name = MacroProfile.GlobalName;
            settings.GlobalProfile.Triggers ??= [];

            settings.BuildProfiles ??= [];
            NormalizeBuildProfiles(settings);

            foreach (var trigger in EnumerateAllTriggers(settings))
            {
                if (trigger.Id == Guid.Empty)
                    trigger.Id = Guid.NewGuid();
            }

            if (settings.GlobalProfile.Triggers.Count == 0)
                settings.GlobalProfile.Triggers.Add(CreateDefaultClickerTrigger());
        }

        public static MacroProfile? GetActiveBuildProfile(MacroSettings settings)
        {
            if (!IsAdditionalBuildProfileActive(settings))
                return null;

            return settings.BuildProfiles.FirstOrDefault(p =>
                string.Equals(p.Name, settings.ActiveBuildProfileName, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsAdditionalBuildProfileActive(MacroSettings settings) =>
            !string.IsNullOrWhiteSpace(settings.ActiveBuildProfileName)
            && !string.Equals(settings.ActiveBuildProfileName, MacroProfile.GlobalName, StringComparison.OrdinalIgnoreCase)
            && settings.BuildProfiles.Any(p =>
                string.Equals(p.Name, settings.ActiveBuildProfileName, StringComparison.OrdinalIgnoreCase));

        public static MacroProfile? GetProfileByName(MacroSettings settings, string name)
        {
            if (string.Equals(name, MacroProfile.GlobalName, StringComparison.OrdinalIgnoreCase))
                return settings.GlobalProfile;

            return settings.BuildProfiles.FirstOrDefault(p =>
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<MacroTrigger> EnumerateAllTriggers(MacroSettings settings)
        {
            foreach (var trigger in settings.GlobalProfile.Triggers)
                yield return trigger;

            foreach (var profile in settings.BuildProfiles)
            {
                foreach (var trigger in profile.Triggers)
                    yield return trigger;
            }
        }

        public static IEnumerable<(MacroProfile Profile, MacroTrigger Trigger)> EnumerateTriggers(
            MacroSettings settings,
            bool includeGlobal,
            bool includeActiveBuild)
        {
            if (includeGlobal)
            {
                foreach (var trigger in settings.GlobalProfile.Triggers)
                    yield return (settings.GlobalProfile, trigger);
            }

            if (!includeActiveBuild)
                yield break;

            var build = GetActiveBuildProfile(settings);
            if (build is null)
                yield break;

            foreach (var trigger in build.Triggers)
                yield return (build, trigger);
        }

        public static string SuggestNewBuildProfileName(IEnumerable<MacroProfile> profiles)
        {
            var names = profiles.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 2; i <= MaxBuildProfiles + 1; i++)
            {
                string candidate = $"Profile {i}";
                if (!names.Contains(candidate))
                    return candidate;
            }

            return "New profile";
        }

        public static bool IsBuildProfileNameAvailable(string name, IEnumerable<MacroProfile> profiles)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            string trimmed = name.Trim();
            if (string.Equals(trimmed, MacroProfile.GlobalName, StringComparison.OrdinalIgnoreCase))
                return false;

            return !profiles.Any(p => string.Equals(p.Name, trimmed, StringComparison.OrdinalIgnoreCase));
        }

        private static void NormalizeBuildProfiles(MacroSettings settings)
        {
            settings.BuildProfiles.RemoveAll(p => p is null);

            foreach (var profile in settings.BuildProfiles)
            {
                profile.Triggers ??= [];
                profile.Name = string.IsNullOrWhiteSpace(profile.Name)
                    ? SuggestNewBuildProfileName(settings.BuildProfiles)
                    : profile.Name.Trim();
            }

            var unique = new Dictionary<string, MacroProfile>(StringComparer.OrdinalIgnoreCase);
            foreach (var profile in settings.BuildProfiles)
            {
                if (!unique.TryGetValue(profile.Name, out var existing))
                {
                    unique[profile.Name] = profile;
                    continue;
                }

                if (profile.Triggers.Count > existing.Triggers.Count)
                    unique[profile.Name] = profile;
            }

            settings.BuildProfiles = unique.Values
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (settings.BuildProfiles.Count > MaxBuildProfiles)
                settings.BuildProfiles = settings.BuildProfiles.Take(MaxBuildProfiles).ToList();

            if (string.IsNullOrWhiteSpace(settings.ActiveBuildProfileName)
                || string.Equals(settings.ActiveBuildProfileName, MacroProfile.GlobalName, StringComparison.OrdinalIgnoreCase))
            {
                settings.ActiveBuildProfileName = MacroProfile.GlobalName;
            }
            else if (!settings.BuildProfiles.Any(p =>
                string.Equals(p.Name, settings.ActiveBuildProfileName, StringComparison.OrdinalIgnoreCase)))
            {
                settings.ActiveBuildProfileName = MacroProfile.GlobalName;
            }
            else
            {
                settings.ActiveBuildProfileName = settings.BuildProfiles.First(p =>
                    string.Equals(p.Name, settings.ActiveBuildProfileName, StringComparison.OrdinalIgnoreCase)).Name;
            }
        }

        private static MacroProfile CreateDefaultGlobalProfile() => new()
        {
            Name = MacroProfile.GlobalName,
            Triggers = [CreateDefaultClickerTrigger()],
        };

        private static MacroTrigger CreateDefaultClickerTrigger() => new()
        {
            Active = true,
            TriggerKey = "XButton1",
            FireSequence = "LButton Down\nLButton Up",
            Behavior = MacroBehavior.Loop,
            KeyDelayMs = 20,
            CycleDelayMs = 20,
        };
    }
}
