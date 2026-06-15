namespace PoE.dlls.Settings.Macros
{
    public sealed class MacroSettings
    {
        public string EnableKey { get; set; } = "F9";

        public bool FeatureEnabled { get; set; } = true;

        public MacroProfile GlobalProfile { get; set; } = new() { Name = MacroProfile.GlobalName };

        public string ActiveBuildProfileName { get; set; } = MacroProfile.DefaultBuildName;

        public List<MacroProfile> BuildProfiles { get; set; } = [];
    }
}
