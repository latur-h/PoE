namespace PoE.dlls.Settings.Macros
{
    public sealed class MacroSettings
    {
        public string EnableKey { get; set; } = "F9";

        public bool FeatureEnabled { get; set; } = true;

        public bool OverlayEnabled { get; set; }

        public MacroOverlayCorner OverlayCorner { get; set; } = MacroOverlayCorner.TopLeft;

        public MacroProfile GlobalProfile { get; set; } = new() { Name = MacroProfile.GlobalName };

        public string ActiveBuildProfileName { get; set; } = MacroProfile.GlobalName;

        public List<MacroProfile> BuildProfiles { get; set; } = [];

        /// <summary>Saved #RRGGBB colors from prior eyedropper picks.</summary>
        public List<string> RememberedColors { get; set; } = [];
    }
}
