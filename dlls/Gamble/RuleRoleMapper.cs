using System.Text.RegularExpressions;

namespace PoE.dlls.Gamble
{
    public enum RuleRole
    {
        None,
        Exclude,
        Optional,
        Required,
        Include,
        Stat,
    }

    internal static class RuleRoleMapper
    {
        private static readonly Regex CompactStatRuleRegex = new(
            @"q\d+r\d+ps\d+",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex MoreStatRuleRegex = new(
            @".*?:\d+(?>;|$)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex ModCountStatRuleRegex = new(
            @"^(?'kind'mods|affixes):(?'min'\d+);?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsMapMode(GambleType type) =>
            type is GambleType.Map or GambleType.MapExalt or GambleType.MapT17;

        public static bool IsRoleColumnVisible(GambleType type) => type != GambleType.Chromatic;

        public static string GetDisplayName(RuleRole role) => role switch
        {
            RuleRole.None => "None",
            RuleRole.Exclude => "Exclude",
            RuleRole.Optional => "Optional",
            RuleRole.Required => "Required",
            RuleRole.Include => "Include",
            RuleRole.Stat => "Stat",
            _ => "None",
        };

        public static RuleRole FromDisplayName(GambleType type, string? displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return RuleRole.None;

            foreach (RuleRole role in GetRolesForMode(type))
            {
                if (string.Equals(GetDisplayName(role), displayName.Trim(), StringComparison.OrdinalIgnoreCase))
                    return role;
            }

            return RuleRole.None;
        }

        public static IReadOnlyList<RuleRole> GetRolesForMode(GambleType type)
        {
            if (type == GambleType.Chromatic)
                return [];

            if (IsMapMode(type))
                return [RuleRole.None, RuleRole.Stat, RuleRole.Include, RuleRole.Exclude];

            return [RuleRole.None, RuleRole.Required, RuleRole.Optional, RuleRole.Exclude];
        }

        public static RuleRole FromPriority(GambleType type, decimal priority, string? content)
        {
            priority = NormalizePriority(priority);

            if (priority <= -1)
                return RuleRole.Exclude;

            if (priority >= 1)
                return IsMapMode(type) ? RuleRole.Include : RuleRole.Required;

            if (priority > 0)
                return IsMapMode(type) ? RuleRole.None : RuleRole.Optional;

            if (IsMapMode(type) && LooksLikeStatContent(content))
                return RuleRole.Stat;

            return RuleRole.None;
        }

        public static decimal ToPriority(GambleType type, RuleRole role) => role switch
        {
            RuleRole.Exclude => -1,
            RuleRole.Optional => 0.5m,
            RuleRole.Required or RuleRole.Include => 1,
            RuleRole.Stat or RuleRole.None => 0,
            _ => 0,
        };

        public static decimal NormalizePriority(decimal priority)
        {
            if (priority <= -1)
                return -1;

            if (priority >= 1)
                return 1;

            if (priority > 0)
                return 0.5m;

            return 0;
        }

        public static bool LooksLikeStatContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            string trimmed = content.Trim();
            return CompactStatRuleRegex.IsMatch(trimmed)
                || MoreStatRuleRegex.IsMatch(trimmed)
                || ModCountStatRuleRegex.IsMatch(trimmed);
        }
    }
}
