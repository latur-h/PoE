using PoE.dlls.Flasks.Base;
using PoE.dlls.InteropServices;
using PoE.dlls.Settings;

namespace PoE.dlls.Flasks
{
    public static class FlaskRegistrationHelper
    {
        public static bool IsUtility(string? flaskType) =>
            string.Equals(flaskType, FlaskType.Utility.ToString(), StringComparison.OrdinalIgnoreCase);

        public static bool IsTincture(string? flaskType) =>
            string.Equals(flaskType, FlaskType.Tincture.ToString(), StringComparison.OrdinalIgnoreCase);

        public static bool UsesDualPixelDetection(string? flaskType) => IsUtility(flaskType) || IsTincture(flaskType);

        public static void Clear(UIFlask flask)
        {
            flask.IsRegistered = false;
            flask.RegisteredTopArgb = 0;
            flask.RegisteredBottomArgb = 0;
        }

        public static string DescribeRegistration(UIFlask flask) =>
            flask.IsRegistered ? "Reg" : "Not reg";

        public static string DescribeRuntimeState(UIFlask flask, bool isDrinking, bool isReady)
        {
            if (!flask.IsRegistered)
                return "Press F5 in town";

            if (!UsesDualPixelDetection(flask.FlaskType))
                return isDrinking ? "Drinking" : "Idle";

            if (!isDrinking)
                return "Idle";

            if (isReady)
                return "Ready";

            return IsTincture(flask.FlaskType) ? "Cooldown" : "Effect on";
        }
    }
}
