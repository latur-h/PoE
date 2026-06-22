namespace PoE.dlls.Settings.Mods
{
    internal static class GamblePresetContentHelper
    {
        public static bool HasContent(GamblePreset preset) =>
            preset.Rules.Any(r => !string.IsNullOrWhiteSpace(r.Content));
    }
}
