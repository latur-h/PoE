namespace PoE.dlls.Flasks
{
    internal static class FlaskDualPixelReadiness
    {
        /// <summary>Utility flask pixel when the effect border is active (effect on).</summary>
        internal static readonly Color UtilityEffectBottomColor = ColorTranslator.FromHtml("#F9D799");

        /// <summary>Tincture flask pixel when on cooldown (not ready to press).</summary>
        internal static readonly Color TinctureCooldownBottomColor = ColorTranslator.FromHtml("#F9D799");

        /// <summary>
        /// Utility: registered top = full flask in town (effect off). Fire when still full and effect off.
        /// </summary>
        public static bool UtilityIsReady(Color currentTop, Color currentBottom, Color registeredTop) =>
            currentTop == registeredTop
            && currentBottom.ToArgb() != UtilityEffectBottomColor.ToArgb();

        /// <summary>
        /// Tincture: registered top = effect off in town. Fire when effect still off and not on cooldown.
        /// </summary>
        public static bool TinctureIsReady(Color currentTop, Color currentBottom, Color registeredTop) =>
            currentTop == registeredTop
            && currentBottom.ToArgb() != TinctureCooldownBottomColor.ToArgb();
    }
}
