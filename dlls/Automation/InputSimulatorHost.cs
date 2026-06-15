using Poss.Win.Automation.Input;

namespace PoE.dlls.Automation
{
    public sealed class InputSimulatorHost
    {
        public const string DefaultProcessName = "PathOfExile.exe";

        private InputSimulator _simulator;

        public InputSimulatorHost()
        {
            _simulator = new InputSimulator(DefaultProcessName);
            EffectiveProcessName = DefaultProcessName;
        }

        public InputSimulator Simulator => _simulator;

        /// <summary>
        /// Process filter string passed to <see cref="InputSimulator"/> (always includes .exe).
        /// </summary>
        public string EffectiveProcessName { get; private set; }

        public void Configure(string? configuredProcessName)
        {
            string resolved = ToInputSimulatorArgument(configuredProcessName);
            if (string.Equals(resolved, EffectiveProcessName, StringComparison.OrdinalIgnoreCase))
                return;

            EffectiveProcessName = resolved;
            _simulator = new InputSimulator(resolved);
        }

        /// <summary>
        /// Resolves configured/empty values to the process filter string shown in settings.
        /// </summary>
        public static string ResolveProcessName(string? configuredProcessName) =>
            ToInputSimulatorArgument(configuredProcessName);

        /// <summary>
        /// InputSimulator treats strings without ".exe" as window title filters.
        /// Always pass the ".exe" form so matching uses process name, not title substring.
        /// </summary>
        public static string ToInputSimulatorArgument(string? configuredProcessName)
        {
            if (string.IsNullOrWhiteSpace(configuredProcessName))
                return DefaultProcessName;

            string trimmed = configuredProcessName.Trim();
            if (trimmed.StartsWith("exe ", StringComparison.OrdinalIgnoreCase))
                trimmed = trimmed[4..].Trim();

            int exeIndex = trimmed.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            if (exeIndex >= 0)
                trimmed = trimmed[..exeIndex].Trim();

            if (string.IsNullOrEmpty(trimmed))
                return DefaultProcessName;

            return trimmed + ".exe";
        }

        public static string NormalizeForStorage(string? enteredProcessName)
        {
            string resolved = ToInputSimulatorArgument(enteredProcessName);
            return string.Equals(resolved, DefaultProcessName, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : resolved;
        }
    }
}
