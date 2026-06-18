using PoE.dlls.Gamble.Modes;
using PoE.dlls.InteropServices;

namespace PoE.dlls.Gamble.Bulk
{
    internal enum BulkMapAction
    {
        None,
        /// <summary>Rare maps: Alt+LMB scour then LMB alchemy.</summary>
        ScourAlchemy,
        /// <summary>Normal maps: LMB alchemy only. Magic maps use ScourAlchemy.</summary>
        AlchemyOnly,
        Exalt,
        Chaos,
        Vaal,
        StashBroken,
        Done,
    }

    internal sealed class BulkMapSlot
    {
        public required Coordinates Position { get; init; }

        public bool IsEmpty { get; set; }

        public bool IsFinished { get; set; }

        public string? Content { get; set; }

        public MapRulesResult Evaluation { get; set; }

        public BulkMapAction NextAction { get; set; } = BulkMapAction.None;

        public bool IsActive => !IsEmpty && !IsFinished;
    }
}
