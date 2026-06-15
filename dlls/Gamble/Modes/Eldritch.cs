using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.Logger;
using PoE.dlls.InteropServices;
using Poss.Win.Automation.Input;
using System.Text.RegularExpressions;
namespace PoE.dlls.Gamble.Modes
{
    public class Eldritch : IGamba
    {
        private readonly Main _main;
        private readonly InputSimulator simulator;

        private double speed = 10.0;
        private TimeSpan delay = TimeSpan.FromMilliseconds(10);

        private readonly Coordinates item;
        private readonly Coordinates orb;

        private readonly List<Rule> rules = [];

        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;

        private int _hash = 0;
        private int count = 0;
        private int maxAttempts = 3;

        public Eldritch(Main main, InputSimulator simulator, CancellationTokenSource cts, TimeSpan delay, double speed, Coordinates item, Coordinates orb, List<Rule> rules)
        {
            _main = main;
            this.simulator = simulator;

            this.delay = delay;
            this.speed = speed;

            _cts = cts;
            _token = _cts.Token;

            this.item = item;
            this.orb = orb;

            this.rules = rules;
        }

        public async Task Gamble()
        {
            simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            await Copy();

            if (CheckItem() == true)
            {
                GamblerLog.Success();
                return;
            }

            await Task.Delay(delay);
            simulator.MouseDeltaMove(orb.X, orb.Y, speed);
            simulator.Send("RButton Down");
            await Task.Delay(delay);
            simulator.Send("RButton Up");
            await Task.Delay(delay);
            simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            simulator.Send("Shift Down");
            await Task.Delay(delay);
            while (!_token.IsCancellationRequested)
            {
                simulator.Send("LButton Down");
                await Task.Delay(delay);
                simulator.Send("LButton Up");
                await Task.Delay(delay);

                await Copy();

                if (CheckItem())
                    break;
            }
            await Task.Delay(delay);
            simulator.Send("Shift Up");

            if (_token.IsCancellationRequested)
            {
                GamblerLog.Cancelled();
                return;
            }

            GamblerLog.Success();
        }
        private async Task Copy()
        {
            simulator.Send("Ctrl Down");
            await Task.Delay(delay);
            simulator.Send("Alt Down");
            await Task.Delay(delay);
            simulator.Send("C Down");
            await Task.Delay(delay);
            simulator.Send("C Up");
            await Task.Delay(delay);
            simulator.Send("Alt Up");
            await Task.Delay(delay);
            simulator.Send("Ctrl Up");
            await Task.Delay(delay);
        }
        private bool CheckItem()
        {
            string itemContent = _main.Invoke(() => Clipboard.GetText(TextDataFormat.Text));
            if (string.IsNullOrEmpty(itemContent))
            {
                GamblerLog.ClipboardEmptyWarning();
                _cts.Cancel();
                return false;
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
                    return false;
                }

                count++;
            }

            Regex getModifiers = new(@"\{.*?\}.*?(?={|--------|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            Regex getType = new(@"\{.*?(?'Type'implicit|prefix|suffix).*?\}", RegexOptions.IgnoreCase);
            Regex getName = new(@"\{.*?""(?'Name'.*?)"".*?\}", RegexOptions.IgnoreCase);
            Regex getTier = new(@"\{.*?\(Tier:\s(?'Tier'\d+)\).*?\}", RegexOptions.IgnoreCase);
            Regex getContent = new(@"}(?'Content'.*?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            Regex strip = new(@"\(\d+-\d+\)", RegexOptions.IgnoreCase);

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
                content = strip.Replace(content, string.Empty).Trim();

                if (!Regex.IsMatch(content, @"fractured", RegexOptions.IgnoreCase))
                    GamblerLog.DebugMod(type, tier, name, content);

                Modifier parsedMod = new(type, tier, name, content);
                modifiers.Add(parsedMod);
            }

            var required = rules.Where(r => r.Priority >= 1).ToList();
            var optional = rules.Where(r => r.Priority > 0 && r.Priority < 1).ToList();

            int requiredCount = 0;
            int optionalCount = 0;

            foreach (var rule in required)
            {
                foreach (var mod in modifiers)
                {
                    if (mod.Type != ModifierType.Implicit)
                        continue;

                    Regex content = new(rule.Content, RegexOptions.IgnoreCase);
                    if (!content.IsMatch(mod.Content))
                        continue;

                    requiredCount++;
                }
            }

            if (optional is not null)
                foreach (var rule in optional)
                {
                    foreach (var mod in modifiers)
                    {
                        if (mod.Type != ModifierType.Implicit)
                            continue;

                        Regex content = new(rule.Content, RegexOptions.IgnoreCase);
                        if (!content.IsMatch(mod.Content))
                            continue;

                        optionalCount++;
                    }
                }

            if (required.Count == requiredCount)
            {
                if (optional?.Count > 0 && optionalCount == 0)
                    return false;

                return true;
            }

            return false;
        }
    }
}
