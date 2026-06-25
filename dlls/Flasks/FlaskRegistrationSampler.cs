using PoE.dlls.Flasks.Base;
using PoE.dlls.InteropServices;

namespace PoE.dlls.Flasks
{
    public static class FlaskRegistrationSampler
    {
        public static FlaskRegistration Sample(FlaskType type, int numberOrPercent)
        {
            ResolutionType resolution = InteropHelper.GetScreenResolution();

            return type switch
            {
                FlaskType.HP => SampleTopOnly(GetHpCoordinates(resolution, numberOrPercent)),
                FlaskType.MP => SampleTopOnly(GetMpCoordinates(resolution, numberOrPercent)),
                FlaskType.Utility => SampleTopOnly(GetUtilityTopCoordinates(resolution, numberOrPercent)),
                FlaskType.Tincture => SampleTopOnly(GetTinctureTopCoordinates(resolution, numberOrPercent)),
                _ => throw new NotSupportedException("Unsupported flask type."),
            };
        }

        private static FlaskRegistration SampleTopOnly((int x, int y) point)
        {
            Color top = InteropHelper.GetColorAt(point.x, point.y);
            return new FlaskRegistration { TopArgb = top.ToArgb(), BottomArgb = Color.Empty.ToArgb() };
        }

        private static (int x, int y) GetHpCoordinates(ResolutionType resolution, int percent) =>
            resolution switch
            {
                ResolutionType.QHD => (150, (int)(1400 - ((1400 - 1175) * ((float)percent / 100)))),
                _ => (0, 0),
            };

        private static (int x, int y) GetMpCoordinates(ResolutionType resolution, int percent) =>
            resolution switch
            {
                ResolutionType.QHD => (2400, (int)(1420 - ((1420 - 1180) * ((float)percent / 100)))),
                _ => (0, 0),
            };

        private static (int topX, int topY, int bottomX, int bottomY) GetUtilityCoordinates(ResolutionType resolution, int number)
        {
            number--;
            return resolution switch
            {
                ResolutionType.QHD => (441 + 61 * number, 1344, 417 + 61 * number, 1432),
                _ => (0, 0, 0, 0),
            };
        }

        private static (int x, int y) GetUtilityTopCoordinates(ResolutionType resolution, int number)
        {
            (int topX, int topY, _, _) = GetUtilityCoordinates(resolution, number);
            return (topX, topY);
        }

        private static (int topX, int topY, int bottomX, int bottomY) GetTinctureCoordinates(ResolutionType resolution, int number)
        {
            number--;
            return resolution switch
            {
                ResolutionType.QHD => (458 + 61 * number, 1326, 417 + 61 * number, 1432),
                _ => (0, 0, 0, 0),
            };
        }

        private static (int x, int y) GetTinctureTopCoordinates(ResolutionType resolution, int number)
        {
            (int topX, int topY, _, _) = GetTinctureCoordinates(resolution, number);
            return (topX, topY);
        }
    }
}
