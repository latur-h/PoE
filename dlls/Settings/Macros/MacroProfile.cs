namespace PoE.dlls.Settings.Macros
{
    public sealed class MacroProfile
    {
        public const string GlobalName = "Global";
        public const string DefaultBuildName = "Default";

        public string Name { get; set; } = DefaultBuildName;

        public List<MacroTrigger> Triggers { get; set; } = [];
    }
}
