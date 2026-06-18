using PoE.dlls.Gamble.Modifiers;
using System.Text.RegularExpressions;
using PoE.dlls.Logger;

namespace PoE.dlls.Gamble.Modes
{
    public enum MapRuleFailure
    {
        None,
        Exclude,
        Include,
        Stats,
        NoMods,
    }

    public readonly record struct MapRulesResult(
        bool IsMap,
        bool IsRare,
        bool IsCorrupted,
        int ModCount,
        int AffixModCount,
        MapRuleFailure Failure,
        bool RulesPassed)
    {
        public bool ExcludeHit => Failure == MapRuleFailure.Exclude;
    }

    public static class MapRulesEvaluator
    {
        private static readonly Regex MapClassRegex = new(
            @"item\sclass:\s(?>maps|expedition logbooks)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex RarityRareRegex = new(
            @"Rarity:\s*Rare",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RarityMagicRegex = new(
            @"Rarity:\s*Magic",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RarityNormalRegex = new(
            @"Rarity:\s*Normal",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CorruptedRegex = new(
            @"(?m)^Corrupted\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CompactStatRuleRegex = new(
            @"q\d+r\d+ps\d+",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex MoreStatRuleRegex = new(
            @".*?:\d+(?>;|$)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex ModCountStatRuleRegex = new(
            @"^(?'kind'mods|affixes):(?'min'\d+);?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsRare(string itemContent) => RarityRareRegex.IsMatch(itemContent);

        public static bool IsMagic(string itemContent) => RarityMagicRegex.IsMatch(itemContent);

        public static bool IsNormal(string itemContent) => RarityNormalRegex.IsMatch(itemContent);

        public static bool IsCorrupted(string itemContent) => CorruptedRegex.IsMatch(itemContent);

        public static MapRulesResult Evaluate(string itemContent, IReadOnlyList<Rule> rules, bool logMods = true)
        {
            if (!MapClassRegex.IsMatch(itemContent))
                return new MapRulesResult(false, false, false, 0, 0, MapRuleFailure.None, false);

            bool isRare = IsRare(itemContent);
            bool isCorrupted = IsCorrupted(itemContent);
            var modifiers = ParseModifiers(itemContent, logMods);
            int modCount = modifiers.Count;
            int affixModCount = CountAffixMods(modifiers);

            if (modCount == 0)
                return new MapRulesResult(true, isRare, isCorrupted, 0, 0, MapRuleFailure.NoMods, false);

            if (!CheckStats(itemContent, rules))
                return new MapRulesResult(true, isRare, isCorrupted, modCount, affixModCount, MapRuleFailure.Stats, false);

            var include = rules.Where(x => x.Priority >= 1).ToList();
            var exclude = rules.Where(x => x.Priority <= -1).ToList();
            int includeCount = 0;

            foreach (var mod in modifiers)
            {
                foreach (var rule in exclude)
                {
                    if (Regex.IsMatch(mod.Name, rule.Content, RegexOptions.IgnoreCase))
                        return new MapRulesResult(true, isRare, isCorrupted, modCount, affixModCount, MapRuleFailure.Exclude, false);
                }

                foreach (var rule in include)
                {
                    if (Regex.IsMatch(mod.Name, rule.Content, RegexOptions.IgnoreCase))
                        includeCount++;
                }
            }

            bool rulesPassed = include.Count == includeCount;
            return new MapRulesResult(
                true,
                isRare,
                isCorrupted,
                modCount,
                affixModCount,
                rulesPassed ? MapRuleFailure.None : MapRuleFailure.Include,
                rulesPassed);
        }

        public static int CountAffixMods(IReadOnlyList<Modifier> modifiers) =>
            modifiers.Count(m => m.Type is ModifierType.Prefix or ModifierType.Suffix);

        private static bool CheckStats(string itemContent, IReadOnlyList<Rule> rules)
        {
            var statRules = rules
                .Where(x => x.Priority > -1 && x.Priority < 1 && !string.IsNullOrEmpty(x.Content))
                .ToList();

            if (statRules.Count == 0)
                return true;

            foreach (var rule in statRules)
            {
                string content = rule.Content.Trim();
                if (ModCountStatRuleRegex.IsMatch(content))
                {
                    if (!CheckModCountStat(itemContent, content))
                        return false;

                    continue;
                }

                if (CompactStatRuleRegex.IsMatch(rule.Content))
                {
                    if (!CheckCompactStats(itemContent, rule.Content))
                        return false;
                }
                else if (MoreStatRuleRegex.IsMatch(rule.Content))
                {
                    if (!CheckMoreStats(itemContent, rule.Content))
                        return false;
                }
            }

            return true;
        }

        private static bool CheckModCountStat(string itemContent, string ruleContent)
        {
            Match match = ModCountStatRuleRegex.Match(ruleContent.Trim());
            if (!match.Success)
                return true;

            if (!int.TryParse(match.Groups["min"].Value, out int minimum))
                return true;

            var modifiers = ParseModifiers(itemContent, logMods: false);
            string kind = match.Groups["kind"].Value;
            int count = string.Equals(kind, "affixes", StringComparison.OrdinalIgnoreCase)
                ? CountAffixMods(modifiers)
                : modifiers.Count;

            GamblerLog.Debug($"Affixes {count} vs {minimum}");
            return count >= minimum;
        }

        private static bool CheckCompactStats(string itemContent, string ruleContent)
        {
            var percent = Regex.Match(
                ruleContent,
                @"q(?'quantity'\d+)r(?'rarity'\d+)ps(?'packsize'\d+)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!percent.Success)
                return true;

            int requiredQ = int.Parse(percent.Groups["quantity"].Value);
            int requiredR = int.Parse(percent.Groups["rarity"].Value);
            int requiredPs = int.Parse(percent.Groups["packsize"].Value);

            Regex quantity = new(@"quantity:\s\+(?'number'\d+)%", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Regex rarity = new(@"rarity:\s\+(?'number'\d+)%", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Regex packSize = new(@"pack\ssize:\s\+(?'number'\d+)%", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            int mapQuantity = quantity.IsMatch(itemContent)
                ? int.Parse(quantity.Match(itemContent).Groups["number"].Value)
                : 0;
            int mapRarity = rarity.IsMatch(itemContent)
                ? int.Parse(rarity.Match(itemContent).Groups["number"].Value)
                : 0;
            int mapPackSize = packSize.IsMatch(itemContent)
                ? int.Parse(packSize.Match(itemContent).Groups["number"].Value)
                : 0;

            GamblerLog.Debug($"Q{mapQuantity}vs{requiredQ};R{mapRarity}vs{requiredR};PS{mapPackSize}vs{requiredPs}");

            return mapQuantity >= requiredQ && mapRarity >= requiredR && mapPackSize >= requiredPs;
        }

        private static bool CheckMoreStats(string itemContent, string ruleContent)
        {
            Regex moreRuleRegex = new(@"(?'type'.*?):(?'number'\d+)(?>;|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var moreRuleMatches = moreRuleRegex.Matches(ruleContent);
            if (moreRuleMatches.Count == 0)
                return true;

            Regex moreMapRegex = new(
                @"more\s(?'type'.*?):\s\+(?'number'\d+)%\s\(augmented\)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var moreMapMatches = moreMapRegex.Matches(itemContent).Cast<Match>().ToList();

            int matchCount = 0;

            foreach (Match ruleMatch in moreRuleMatches)
            {
                string ruleType = ruleMatch.Groups["type"].Value.Trim();
                if (!int.TryParse(ruleMatch.Groups["number"].Value, out int ruleMinimum))
                    continue;

                foreach (Match mapMatch in moreMapMatches)
                {
                    if (!string.Equals(ruleType, mapMatch.Groups["type"].Value.Trim(), StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!int.TryParse(mapMatch.Groups["number"].Value, out int mapValue))
                        continue;

                    GamblerLog.Debug($"{ruleMinimum}vs{mapValue}");

                    if (mapValue >= ruleMinimum)
                        matchCount++;
                }
            }

            return moreRuleMatches.Count == matchCount;
        }

        private static List<Modifier> ParseModifiers(string itemContent, bool logMods)
        {
            Regex getModifiers = new(@"\{.*?\}.*?(?={|--------|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Regex getType = new(@"\{.*?(?'Type'implicit|prefix|suffix).*?\}", RegexOptions.IgnoreCase);
            Regex getName = new(@"\{.*?""(?'Name'.*?)"".*?\}", RegexOptions.IgnoreCase);
            Regex getTier = new(@"\{.*?\(Tier:\s(?'Tier'\d+)\).*?\}", RegexOptions.IgnoreCase);
            Regex getContent = new(@"}(?'Content'.*?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var mods = getModifiers.Matches(itemContent);
            List<Modifier> modifiers = [];

            if (logMods)
                GamblerLog.DebugSeparator();

            foreach (var mod in mods.Cast<Match>())
            {
                ModifierType type = Enum.Parse<ModifierType>(getType.Match(mod.Value).Groups["Type"].Value.Trim());

                string name = getName.IsMatch(mod.Value)
                    ? getName.Match(mod.Value).Groups["Name"].Value.Trim()
                    : string.Empty;

                int tier = getTier.IsMatch(mod.Value)
                    ? int.Parse(getTier.Match(mod.Value).Groups["Tier"].Value.Trim())
                    : 0;

                string content = getContent.Match(mod.Value).Groups["Content"].Value.Trim();
                content = GambleModContentMatcher.NormalizeItemModContent(content);

                if (logMods && !Regex.IsMatch(content, @"fractured", RegexOptions.IgnoreCase))
                    GamblerLog.DebugMod(type, tier, name, content);

                modifiers.Add(new Modifier(type, tier, name, content));
            }

            return modifiers;
        }
    }
}
