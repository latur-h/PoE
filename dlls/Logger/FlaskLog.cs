using PoE.dlls.Flasks.Base;

namespace PoE.dlls.Logger
{
    public static class FlaskLog
    {
        public static void Registered(int number, FlaskType type, string key) =>
            AppLog.Flask(LogSeverity.Info, $"Registering flask | Number: {number}; Type: {type}; Key: {key}");

        public static void DrinkStarted() => AppLog.Flask(LogSeverity.Info, "Starting drinking...");
        public static void DrinkStopped() => AppLog.Flask(LogSeverity.Info, "Drinking stopped.");
        public static void StopRequested() => AppLog.Flask(LogSeverity.Info, "Requested stop drinking...");
    }
}
