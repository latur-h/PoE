namespace PoE.dlls.Settings.Mods
{
    public class GamblePreset
    {
        public const string DefaultName = "Default";

        public string Name { get; set; } = DefaultName;
        public List<GambleRuleRow> Rules { get; set; } = [];
    }
}
