namespace PoE.dlls.GameData
{
    public sealed class ModSuggestionBehavior
    {
        public static ModSuggestionBehavior Default { get; } = new();

        public ModSuggestionScope Scope { get; init; } = ModSuggestionScope.All;

        public bool InsertModNameOnly { get; init; }

        public bool ShowNameAndDescription { get; init; }
    }
}
