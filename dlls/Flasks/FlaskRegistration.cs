namespace PoE.dlls.Flasks
{
    public sealed class FlaskRegistration
    {
        public int TopArgb { get; init; }

        public int BottomArgb { get; init; }

        public static Color ToColor(int argb) => Color.FromArgb(argb);

        public Color TopColor => ToColor(TopArgb);

        public Color BottomColor => ToColor(BottomArgb);
    }
}
