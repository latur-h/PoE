using System.Reflection;

namespace PoE.dlls.Updates
{
    internal static class AppVersion
    {
        public static Version Current => ResolveCurrentVersion();

        public static string CurrentDisplay => Current.ToString(3);

        public static bool IsNewerThanCurrent(Version candidate) =>
            candidate > Current;

        public static Version? ParseReleaseTag(string? tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return null;

            string trimmed = tagName.Trim();
            if (trimmed.StartsWith('v') || trimmed.StartsWith('V'))
                trimmed = trimmed[1..];

            return Version.TryParse(trimmed, out Version? version) ? version : null;
        }

        private static Version ResolveCurrentVersion()
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            string? informational = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

            if (!string.IsNullOrWhiteSpace(informational))
            {
                int plus = informational.IndexOf('+');
                string core = plus >= 0 ? informational[..plus] : informational;
                if (Version.TryParse(core, out Version? parsed))
                    return parsed;
            }

            return assembly.GetName().Version ?? new Version(0, 0, 0);
        }
    }
}
