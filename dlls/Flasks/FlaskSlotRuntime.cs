namespace PoE.dlls.Flasks
{
    public sealed class FlaskSlotRuntime
    {
        public required string Slot { get; init; }

        public bool IsReady { get; init; }

        public bool UsesDualPixel { get; init; }
    }
}
