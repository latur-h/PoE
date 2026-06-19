using PoE.dlls.Settings.Mods;

namespace PoE.dlls.GameData
{
    public sealed class EldritchModSuggestionStrategy : IModSuggestionStrategy
    {
        private readonly ModEldritchInfluence _influence;
        private readonly string? _itemTypeFilter;

        private EldritchModSuggestionStrategy(ModEldritchInfluence influence, string? itemTypeFilter)
        {
            _influence = influence;
            _itemTypeFilter = ModSpawnTagFilter.Normalize(itemTypeFilter);
        }

        public static EldritchModSuggestionStrategy For(EldritchInfluence influence, string? itemTypeFilter = null) =>
            new(ToCatalogInfluence(influence), itemTypeFilter);

        public IReadOnlyList<ModSuggestionItem> Search(ModCacheDatabase database, string term, int limit, int offset) =>
            database.SearchEldritchImplicit(_influence, term, _itemTypeFilter, limit, offset);

        public string FormatDisplay(ModSuggestionItem item, string searchTerm) => item.ModContent;

        public string FormatInsert(ModSuggestionItem item, string searchTerm) =>
            ModTemplateNormalizer.ToHashTemplate(item.ModContent);

        public int SuggestionPopupMinWidth => 360;

        private static ModEldritchInfluence ToCatalogInfluence(EldritchInfluence influence) =>
            influence switch
            {
                EldritchInfluence.EaterOfWorlds => ModEldritchInfluence.EaterOfWorlds,
                _ => ModEldritchInfluence.SearingExarch,
            };
    }
}
