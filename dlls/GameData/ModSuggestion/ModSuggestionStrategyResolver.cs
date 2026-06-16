using PoE.dlls.Gamble;

namespace PoE.dlls.GameData
{
    public static class ModSuggestionStrategyResolver
    {
        public static IModSuggestionStrategy For(GambleType gambleType) =>
            gambleType is GambleType.Map or GambleType.MapExalt or GambleType.MapT17
                ? MapModSuggestionStrategy.Instance
                : ItemModSuggestionStrategy.Instance;
    }
}
