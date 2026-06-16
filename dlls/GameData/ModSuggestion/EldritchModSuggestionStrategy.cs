using PoE.dlls.Settings.Mods;

namespace PoE.dlls.GameData
{
    public sealed class EldritchModSuggestionStrategy : IModSuggestionStrategy
    {
        private readonly ModEldritchInfluence _influence;

        private EldritchModSuggestionStrategy(ModEldritchInfluence influence) => _influence = influence;

        public static EldritchModSuggestionStrategy For(EldritchInfluence influence) =>
            new(ToCatalogInfluence(influence));

        public IReadOnlyList<ModSuggestionItem> Search(ModCacheDatabase database, string term, int limit, int offset) =>
            database.SearchEldritchImplicit(_influence, term, limit, offset);

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
