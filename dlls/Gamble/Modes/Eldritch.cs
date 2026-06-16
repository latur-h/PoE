using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.Logger;
using PoE.dlls.InteropServices;
using PoE.dlls.Settings.Mods;
using Poss.Win.Automation.Input;
using PoE.dlls.Automation;

namespace PoE.dlls.Gamble.Modes
{
    public class Eldritch : IGamba
    {
        private readonly Main _main;
        private readonly InputSimulatorHost inputHost;

        private double speed = 10.0;
        private TimeSpan delay = TimeSpan.FromMilliseconds(50);

        private readonly Coordinates item;
        private readonly Coordinates exarchOrb;
        private readonly Coordinates eaterOrb;

        private readonly List<Rule> rules = [];

        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;

        private bool _isShiftHeld;
        private EldritchInfluence _currentOrb = EldritchInfluence.SearingExarch;
        private readonly GambleItemClipboardHelper.HashState _hashState = new();

        private const int MaxUnchangedReads = 3;

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
            await Task.Delay(delay, _token);

            (bool ok, List<Modifier> modifiers) = await TryReadItemAsync(requireHashChange: false);
            if (!ok)
            {
                GamblerLog.Cancelled();
                return;
            }

            if (await TryConfirmCompleteAsync(modifiers))
            {
                GamblerLog.Success();
                return;
            }

            await PickUpOrbAsync(EldritchInfluence.SearingExarch);

            bool afterSlam = false;
            int? baselineHash = null;
            bool succeeded = false;

            while (!_token.IsCancellationRequested)
            {
                (ok, modifiers) = await TryReadItemAsync(afterSlam, baselineHash);
                if (!ok)
                    break;

                if (await TryConfirmCompleteAsync(modifiers))
                {
                    succeeded = true;
                    break;
                }

                EldritchInfluence targetOrb = SelectOrb(modifiers);
                if (targetOrb != _currentOrb)
                {
                    await ReleaseShift();
                    await PickOrbAndMoveToItem(targetOrb);
                    _currentOrb = targetOrb;
                    await HoldShift();
                    _hashState.Reset();
                }

                baselineHash = _hashState.Hash;
                await Slam();
                afterSlam = true;
            }

            await ReleaseShift();

            if (_token.IsCancellationRequested)
            {
                GamblerLog.Cancelled();
                return;
            }

            if (succeeded)
                GamblerLog.Success();
            else
                GamblerLog.Error("Failed to check item!");
        }

        private async Task PickUpOrbAsync(EldritchInfluence influence)
        {
            await PickOrbAndMoveToItem(influence);
            _currentOrb = influence;
            await HoldShift();
        }

        private async Task PickOrbAndMoveToItem(EldritchInfluence influence) =>
            await PickOrbAndMoveToItem(OrbFor(influence));

        private async Task PickOrbAndMoveToItem(Coordinates orb)
        {
            inputHost.Simulator.MouseDeltaMove(orb.X, orb.Y, speed);
            await Task.Delay(delay);
            inputHost.Simulator.Send("RButton Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("RButton Up");
            await Task.Delay(delay);
            inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);
        }

        private async Task HoldShift()
        {
            if (_isShiftHeld)
                return;

            inputHost.Simulator.Send("Shift Down");
            await Task.Delay(delay);
            _isShiftHeld = true;
        }

        private async Task ReleaseShift()
        {
            if (!_isShiftHeld)
                return;

            inputHost.Simulator.Send("Shift Up");
            await Task.Delay(delay);
            _isShiftHeld = false;
        }

        private async Task Slam()
        {
            inputHost.Simulator.Send("LButton Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("LButton Up");
            await Task.Delay(delay);
        }

        private EldritchInfluence SelectOrb(List<Modifier> modifiers) =>
            SlotSatisfied(modifiers, EldritchInfluence.SearingExarch)
                ? EldritchInfluence.EaterOfWorlds
                : EldritchInfluence.SearingExarch;

        private Coordinates OrbFor(EldritchInfluence influence) =>
            influence == EldritchInfluence.EaterOfWorlds ? eaterOrb : exarchOrb;

        private bool IsComplete(List<Modifier> modifiers) =>
            SlotSatisfied(modifiers, EldritchInfluence.SearingExarch)
            && SlotSatisfied(modifiers, EldritchInfluence.EaterOfWorlds);

        private async Task<(bool Ok, List<Modifier> Modifiers)> TryReadItemAsync(
            bool requireHashChange,
            int? baselineHash = null)
        {
            string? content = await GambleItemClipboardHelper.CopyAndReadAsync(
                _main,
                inputHost,
                delay,
                _token,
                baselineHash,
                requireHashChange);

            if (content is null)
            {
                GamblerLog.ClipboardEmptyWarning();
                _cts.Cancel();
                return (false, []);
            }

            if (!_hashState.Register(content, MaxUnchangedReads, _cts))
                return (false, []);

            List<Modifier> modifiers = GambleRuleEvaluator.ParseModifiers(content, logImplicitMods: true, logParse: true);
            return (true, modifiers);
        }

        private async Task<bool> TryConfirmCompleteAsync(List<Modifier> modifiers)
        {
            if (!IsComplete(modifiers))
                return false;

            string? confirm = await GambleItemClipboardHelper.ConfirmMatchAsync(_main, inputHost, delay, _token);
            if (confirm is null)
            {
                GamblerLog.ClipboardEmptyWarning();
                _cts.Cancel();
                return false;
            }

            if (!_hashState.Register(confirm, MaxUnchangedReads, _cts))
                return false;

            List<Modifier> confirmed = GambleRuleEvaluator.ParseModifiers(confirm, logImplicitMods: true, logParse: true);
            return IsComplete(confirmed);
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
    }
}
