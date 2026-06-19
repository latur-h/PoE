namespace PoE.dlls.GameData
{
    public enum ModEldritchInfluence
    {
        None = 0,
        SearingExarch = 1,
        EaterOfWorlds = 2,
    }

    public readonly record struct ModCatalogEntry(
        string ModName,
        string ModContent,
        bool IsMap,
        ModEldritchInfluence EldritchInfluence,
        int ModDomain = 0,
        ModItemKind ItemKind = ModItemKind.Item,
        string SpawnTags = "");
}
