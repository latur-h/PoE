namespace PoE.dlls.Settings.Macros
{
    public sealed class MacroTrigger
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public bool Active { get; set; }

        /// <summary>Send-key form for Single/Loop rising-edge or hold detection.</summary>
        public string TriggerKey { get; set; } = string.Empty;

        /// <summary>One InputSimulator stroke per line, e.g. "Ctrl Down".</summary>
        public string FireSequence { get; set; } = string.Empty;

        public MacroBehavior Behavior { get; set; } = MacroBehavior.Single;

        public int KeyDelayMs { get; set; } = 20;

        public int CycleDelayMs { get; set; } = 100;

        /// <summary>Repeat: start/stop spam. Single/Loop: optional runtime Active toggle.</summary>
        public string ToggleKey { get; set; } = string.Empty;
    }
}
