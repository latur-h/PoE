using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.Logger;
using System.Text.RegularExpressions;

namespace PoE.dlls.Gamble
{
    internal enum AltAugResponse
    {
        Alt,
        Aug,
        Success,
        Failure,
    }

    internal static class GambleRuleEvaluator
    {
        public static bool MatchesRules(
            string itemContent,
            IReadOnlyList<Rule> rules,
            bool matchNameToo = false,
            bool logParse = true)
        {
            List<Modifier> modifiers = ParseModifiers(itemContent, logImplicitMods: true, logParse: logParse);

            var required = rules.Where(r => r.Priority >= 1).ToList();
            var optional = rules.Where(r => r.Priority > 0 && r.Priority < 1).ToList();

            int requiredCount = 0;
            int optionalCount = 0;

            foreach (Rule rule in required)
            {
                foreach (Modifier mod in modifiers)
                {
                    if (!GambleModContentMatcher.MatchesModRule(rule, mod, matchNameToo))
                        continue;

                    requiredCount++;
                }
            }

            foreach (Rule rule in optional)
            {
                foreach (Modifier mod in modifiers)
                {
                    if (!GambleModContentMatcher.MatchesModRule(rule, mod, matchNameToo))
                        continue;

                    optionalCount++;
                }
            }

            if (required.Count != requiredCount)
                return false;

            if (optional.Count > 0 && optionalCount == 0)
                return false;

            return true;
        }

        public static AltAugResponse EvaluateAltAug(string itemContent, IReadOnlyList<Rule> rules, bool logParse = true)
        {
            List<Modifier> modifiers = ParseModifiers(itemContent, logImplicitMods: false, logParse: logParse);

            var required = rules.Where(r => r.Priority >= 1).ToList();
            var optional = rules.Where(r => r.Priority > 0 && r.Priority < 1).ToList();

            int requiredCount = 0;
            int optionalCount = 0;
            int modsCount = modifiers.Count(x => x.Type is ModifierType.Suffix or ModifierType.Prefix);

            foreach (Rule rule in required)
            {
                foreach (Modifier mod in modifiers)
                {
                    if (!GambleModContentMatcher.MatchesModRule(rule, mod, matchNameToo: true))
                        continue;

                    requiredCount++;
                }
            }

            foreach (Rule rule in optional)
            {
                foreach (Modifier mod in modifiers)
                {
                    if (!GambleModContentMatcher.MatchesModRule(rule, mod, matchNameToo: true))
                        continue;

                    optionalCount++;
                }
            }

            if (required.Count <= requiredCount)
            {
                if (optional.Count > 0 && optionalCount == 0)
                    return modsCount == 1 ? AltAugResponse.Aug : AltAugResponse.Alt;

                return AltAugResponse.Success;
            }

            return modsCount == 1 ? AltAugResponse.Aug : AltAugResponse.Alt;
        }

        public static List<Modifier> ParseModifiers(string itemContent, bool logImplicitMods = true, bool logParse = true)
        {
            Regex getModifiers = new(@"\{.*?\}.*?(?={|--------|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Regex getType = new(@"\{.*?(?'Type'implicit|prefix|suffix).*?\}", RegexOptions.IgnoreCase);
            Regex getName = new(@"\{.*?""(?'Name'.*?)"".*?\}", RegexOptions.IgnoreCase);
            Regex getTier = new(@"\{.*?\(Tier:\s(?'Tier'\d+)\).*?\}", RegexOptions.IgnoreCase);
            Regex getContent = new(@"}(?'Content'.*?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Regex getEnchant = new(@".*?\(enchant\)", RegexOptions.IgnoreCase);

            var mods = getModifiers.Matches(itemContent);

            List<Modifier> modifiers = [];

            if (logParse)
                GamblerLog.DebugSeparator();
            foreach (Match mod in mods.Cast<Match>())
            {
                ModifierType type = Enum.Parse<ModifierType>(getType.Match(mod.Value).Groups["Type"].Value.Trim());

                string name = string.Empty;
                if (getName.IsMatch(mod.Value))
                    name = getName.Match(mod.Value).Groups["Name"].Value.Trim();

                int tier = 0;
                if (getTier.IsMatch(mod.Value))
                    tier = int.Parse(getTier.Match(mod.Value).Groups["Tier"].Value.Trim());

                string content = getContent.Match(mod.Value).Groups["Content"].Value.Trim();
                content = GambleModContentMatcher.NormalizeItemModContent(content);

                if (logParse
                    && !Regex.IsMatch(content, @"fractured", RegexOptions.IgnoreCase)
                    && (logImplicitMods || type != ModifierType.Implicit))
                {
                    GamblerLog.DebugMod(type, tier, name, content);
                }

                modifiers.Add(new Modifier(type, tier, name, content));
            }

            if (logParse)
            {
                foreach (Match enchant in getEnchant.Matches(itemContent).Cast<Match>())
                {
                    Modifier enchantMod = new(ModifierType.Implicit, 0, "Enchant", enchant.Value.Trim());
                    GamblerLog.DebugMod(enchantMod.Type, enchantMod.Tier, enchantMod.Name, enchantMod.Content);
                    modifiers.Add(enchantMod);
                }
            }
            else
            {
                foreach (Match enchant in getEnchant.Matches(itemContent).Cast<Match>())
                    modifiers.Add(new Modifier(ModifierType.Implicit, 0, "Enchant", enchant.Value.Trim()));
            }

            return modifiers;
        }
    }
}
