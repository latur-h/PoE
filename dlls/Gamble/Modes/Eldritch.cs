using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.Logger;
using PoE.dlls.InteropServices;
using PoE.dlls.Settings.Mods;
using Poss.Win.Automation.Input;
using PoE.dlls.Automation;
using System.Text.RegularExpressions;

namespace PoE.dlls.Gamble.Modes
{
    public class Eldritch : IGamba
    {
        private readonly Main _main;
        private readonly InputSimulatorHost inputHost;

        private double speed = 10.0;
        private TimeSpan delay = TimeSpan.FromMilliseconds(10);

        private readonly Coordinates item;
        private readonly Coordinates exarchOrb;
        private readonly Coordinates eaterOrb;

        private readonly List<Rule> rules = [];

        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;

        private int _hash = 0;
        private int count = 0;
        private int maxAttempts = 3;

        public Eldritch(
            Main main,
            InputSimulatorHost inputHost,
            CancellationTokenSource cts,
            TimeSpan delay,
            double speed,
            Coordinates item,
            Coordinates exarchOrb,
            Coordinates eaterOrb,
            List<Rule> rules)
        {
            _main = main;
            this.inputHost = inputHost;

            this.delay = delay;
            this.speed = speed;

            _cts = cts;
            _token = _cts.Token;

            this.item = item;
            this.exarchOrb = exarchOrb;
            this.eaterOrb = eaterOrb;

            this.rules = rules;
        }

        public async Task Gamble()
        {
            inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            await Copy();

            if (IsComplete())
            {
                GamblerLog.Success();
                return;
            }

            while (!_token.IsCancellationRequested)
            {
                var targetOrb = SelectOrb();
                await ApplyOrb(targetOrb);

                inputHost.Simulator.Send("Shift Down");
                await Task.Delay(delay);
                while (!_token.IsCancellationRequested)
                {
                    inputHost.Simulator.Send("LButton Down");
                    await Task.Delay(delay);
                    inputHost.Simulator.Send("LButton Up");
                    await Task.Delay(delay);

                    await Copy();

                    if (IsComplete())
                        break;
                }
                await Task.Delay(delay);
                inputHost.Simulator.Send("Shift Up");

                if (_token.IsCancellationRequested)
                    break;

                if (IsComplete())
                    break;
            }

            if (_token.IsCancellationRequested)
            {
                GamblerLog.Cancelled();
                return;
            }

            GamblerLog.Success();
        }

        private EldritchInfluence SelectOrb()
        {
            var modifiers = ParseClipboard();
            if (modifiers is null)
                return EldritchInfluence.SearingExarch;

            return SlotSatisfied(modifiers, EldritchInfluence.SearingExarch)
                ? EldritchInfluence.EaterOfWorlds
                : EldritchInfluence.SearingExarch;
        }

        private Coordinates OrbFor(EldritchInfluence influence) =>
            influence == EldritchInfluence.EaterOfWorlds ? eaterOrb : exarchOrb;

        private async Task ApplyOrb(Coordinates orb)
        {
            await Task.Delay(delay);
            inputHost.Simulator.MouseDeltaMove(orb.X, orb.Y, speed);
            inputHost.Simulator.Send("RButton Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("RButton Up");
            await Task.Delay(delay);
            inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);
        }

        private async Task ApplyOrb(EldritchInfluence influence) => await ApplyOrb(OrbFor(influence));

        private bool IsComplete()
        {
            var modifiers = ParseClipboard();
            return modifiers is not null
                   && SlotSatisfied(modifiers, EldritchInfluence.SearingExarch)
                   && SlotSatisfied(modifiers, EldritchInfluence.EaterOfWorlds);
        }

        private bool SlotSatisfied(List<Modifier> modifiers, EldritchInfluence influence)
        {
            var slotRules = RulesFor(influence);
            if (slotRules.Count == 0)
                return true;

            var slotMod = ResolveImplicit(modifiers, influence);
            if (slotMod is null)
                return false;

            return RulesMatch(slotRules, slotMod.Value);
        }

        private List<Rule> RulesFor(EldritchInfluence influence) =>
            rules
                .Where(r => ResolveInfluence(r) == influence)
                .ToList();

        private static EldritchInfluence ResolveInfluence(Rule rule) =>
            rule.EldritchInfluence ?? EldritchInfluence.SearingExarch;

        private static bool RulesMatch(List<Rule> slotRules, Modifier mod)
        {
            var required = slotRules.Where(r => r.Priority >= 1).ToList();
            var optional = slotRules.Where(r => r.Priority > 0 && r.Priority < 1).ToList();

            int requiredCount = 0;
            int optionalCount = 0;

            foreach (var rule in required)
            {
                if (GambleModContentMatcher.MatchesModRule(rule, mod))
                    requiredCount++;
            }

            foreach (var rule in optional)
            {
                if (GambleModContentMatcher.MatchesModRule(rule, mod))
                    optionalCount++;
            }

            if (required.Count != requiredCount)
                return false;

            if (optional.Count > 0 && optionalCount == 0)
                return false;

            return true;
        }

        private static Modifier? ResolveImplicit(List<Modifier> modifiers, EldritchInfluence influence)
        {
            var implicits = modifiers
                .Where(m => m.Type == ModifierType.Implicit && !IsEnchant(m))
                .ToList();

            if (implicits.Count == 0)
                return null;

            int eaterIndex = implicits.FindIndex(IsEaterImplicit);
            int exarchIndex = implicits.FindIndex(IsExarchImplicit);

            if (influence == EldritchInfluence.EaterOfWorlds)
            {
                if (eaterIndex >= 0)
                    return implicits[eaterIndex];

                return implicits.Count >= 2 ? implicits[^1] : null;
            }

            if (exarchIndex >= 0)
                return implicits[exarchIndex];

            if (eaterIndex >= 0 && implicits.Count >= 2)
            {
                int otherIndex = eaterIndex == 0 ? 1 : 0;
                return implicits[otherIndex];
            }

            return implicits[0];
        }

        private static bool IsEnchant(Modifier mod) =>
            mod.Name.Contains("Enchant", StringComparison.OrdinalIgnoreCase)
            || mod.Content.Contains("(enchant)", StringComparison.OrdinalIgnoreCase);

        private static bool IsEaterImplicit(Modifier mod) =>
            mod.Content.Contains("Eater of Worlds", StringComparison.OrdinalIgnoreCase)
            || mod.Name.Contains("Eater of Worlds", StringComparison.OrdinalIgnoreCase);

        private static bool IsExarchImplicit(Modifier mod) =>
            mod.Content.Contains("Searing Exarch", StringComparison.OrdinalIgnoreCase)
            || mod.Name.Contains("Searing Exarch", StringComparison.OrdinalIgnoreCase)
            || mod.Name.Contains("Exarch", StringComparison.OrdinalIgnoreCase);

        private List<Modifier>? ParseClipboard()
        {
            string itemContent = _main.Invoke(() => Clipboard.GetText(TextDataFormat.Text));
            if (string.IsNullOrEmpty(itemContent))
            {
                GamblerLog.ClipboardEmptyWarning();
                _cts.Cancel();
                return null;
            }
            _main.Invoke(Clipboard.Clear);

            int hash = itemContent.GetHashCode();

            if (_hash != hash)
                _hash = hash;
            else
            {
                if (count >= maxAttempts)
                {
                    GamblerLog.MaxAttemptsReached();
                    _cts.Cancel();
                    return null;
                }

                count++;
            }

            Regex getModifiers = new(@"\{.*?\}.*?(?={|--------|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Regex getType = new(@"\{.*?(?'Type'implicit|prefix|suffix).*?\}", RegexOptions.IgnoreCase);
            Regex getName = new(@"\{.*?""(?'Name'.*?)"".*?\}", RegexOptions.IgnoreCase);
            Regex getTier = new(@"\{.*?\(Tier:\s(?'Tier'\d+)\).*?\}", RegexOptions.IgnoreCase);
            Regex getContent = new(@"}(?'Content'.*?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var mods = getModifiers.Matches(itemContent);

            List<Modifier> modifiers = [];

            GamblerLog.DebugSeparator();
            foreach (var mod in mods.Cast<Match>())
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

                if (!Regex.IsMatch(content, @"fractured", RegexOptions.IgnoreCase))
                    GamblerLog.DebugMod(type, tier, name, content);

                modifiers.Add(new Modifier(type, tier, name, content));
            }

            return modifiers;
        }

        private async Task Copy()
        {
            inputHost.Simulator.Send("Ctrl Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("Alt Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("C Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("C Up");
            await Task.Delay(delay);
            inputHost.Simulator.Send("Alt Up");
            await Task.Delay(delay);
            inputHost.Simulator.Send("Ctrl Up");
            await Task.Delay(delay);
        }
    }
}
