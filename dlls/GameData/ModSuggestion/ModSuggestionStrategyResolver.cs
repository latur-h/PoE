using PoE.dlls.Gamble;
using PoE.dlls.Settings.Mods;

namespace PoE.dlls.GameData
{
    public static class ModSuggestionStrategyResolver
    {
        public static IModSuggestionStrategy For(GambleType gambleType) =>
            gambleType switch
            {
                GambleType.Map or GambleType.MapExalt or GambleType.MapT17 => MapModSuggestionStrategy.Instance,
                GambleType.Eldritch => EldritchModSuggestionStrategy.For(EldritchInfluence.SearingExarch),
                _ => ItemModSuggestionStrategy.Instance,
            };
    }
}
