namespace PoE.dlls.Settings.Mods
{
    public static class GamblePresetHelper
    {
        public static bool IsNameAvailable(string name, IEnumerable<GamblePreset> presets)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            string trimmed = name.Trim();
            return !presets.Any(p => string.Equals(p.Name, trimmed, StringComparison.OrdinalIgnoreCase));
        }

        public static string SuggestNewPresetName(IEnumerable<GamblePreset> presets)
        {
            var names = presets.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 2; i <= 100; i++)
            {
                string candidate = $"Preset {i}";
                if (!names.Contains(candidate))
                    return candidate;
            }

            return "New preset";
        }

        public static void NormalizeModeStore(GambleModeStore store)
        {
            store.Presets ??= [];

            foreach (var preset in store.Presets)
            {
                if (preset is null)
                    continue;

                preset.Rules ??= [];
                preset.Name = string.IsNullOrWhiteSpace(preset.Name)
                    ? GamblePreset.DefaultName
                    : preset.Name.Trim();
            }

            store.Presets.RemoveAll(p => p is null);

            var unique = new Dictionary<string, GamblePreset>(StringComparer.OrdinalIgnoreCase);
            foreach (var preset in store.Presets)
            {
                if (!unique.TryGetValue(preset.Name, out var existing))
                {
                    unique[preset.Name] = preset;
                    continue;
                }

                if (preset.Rules.Count > existing.Rules.Count)
                    unique[preset.Name] = preset;
            }

            store.Presets = unique.Values
                .OrderBy(p => string.Equals(p.Name, GamblePreset.DefaultName, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (store.Presets.Count == 0)
            {
                store.Presets.Add(new GamblePreset
                {
                    Name = GamblePreset.DefaultName,
                    Rules = [],
                });
            }

            if (string.IsNullOrWhiteSpace(store.ActivePresetName) ||
                !store.Presets.Any(p => string.Equals(p.Name, store.ActivePresetName, StringComparison.OrdinalIgnoreCase)))
            {
                store.ActivePresetName = store.Presets.First(p =>
                    string.Equals(p.Name, GamblePreset.DefaultName, StringComparison.OrdinalIgnoreCase)).Name;
            }
            else
            {
                store.ActivePresetName = store.Presets.First(p =>
                    string.Equals(p.Name, store.ActivePresetName, StringComparison.OrdinalIgnoreCase)).Name;
            }
        }
    }
}
