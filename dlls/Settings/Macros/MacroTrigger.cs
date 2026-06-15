namespace PoE.dlls.Settings.Macros
{
    public sealed class MacroTrigger
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public bool Active { get; set; }

        /// <summary>Send-key form for Single/Loop rising-edge or hold detection.</summary>
        public string TriggerKey { get; set; } = string.Empty;

        /// <summary>One InputSimulator stroke per line or separated by +, e.g. "Ctrl Down".</summary>
        public string FireSequence { get; set; } = string.Empty;

        public MacroBehavior Behavior { get; set; } = MacroBehavior.Single;

        public int KeyDelayMs { get; set; } = 20;

        public int CycleDelayMs { get; set; } = 100;

        /// <summary>Optional hotkey that flips <see cref="Active"/>.</summary>
        public string ToggleKey { get; set; } = string.Empty;

        public int PixelX { get; set; }

        public int PixelY { get; set; }

        /// <summary>Expected pixel color as #RRGGBB (JE/JNE).</summary>
        public string ExpectedColor { get; set; } = "#000000";

        /// <summary>Post-fire lock in ms to avoid multi-fire while the game updates (JE/JNE).</summary>
        public int LockMs { get; set; } = 200;
    }
}
