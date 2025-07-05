using InputSimulator;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PoE.dlls.Gamble.Modes
{
    public class Alt_Aug : IGamba
    {
        private enum Response
        {
            Alt,
            Aug,
            Success,
            Failure
        }

        private readonly Main _main;
        private readonly Simulator _simulator;

        private double speed = 10.0;
        private TimeSpan delay = TimeSpan.FromMilliseconds(50);
        private readonly Coordinates item;
        private readonly Coordinates alt;
        private readonly Coordinates aug;

        private readonly List<Rule> rules = [];

        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;

        private bool _isShiftHeld = false;

        private int _hash = 0;
        private int count = 0;
        private int maxAttempts = 3;

        public Alt_Aug(Main main, Simulator simulator, CancellationTokenSource cts, TimeSpan delay, double speed, Coordinates item, Coordinates alt, Coordinates aug, List<Rule> rules)
        {
            _main = main;
            _simulator = simulator;

            this.delay = delay;
            this.speed = speed;

            this.item = item;
            this.alt = alt;
            this.aug = aug;

            this.rules = rules;

            _cts = cts;
            _token = _cts.Token;
        }

        public async Task Gamble()
        {
            _simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            await Copy();

            Response response = CheckItem();
            while (response != Response.Success && !_token.IsCancellationRequested)
            {
                if (response == Response.Alt)
                    await SlamAlt();
                else if (response == Response.Aug)
                    await SlamAug();

                await Copy();

                response = CheckItem();
            }

            if (_isShiftHeld)
                _simulator.Send("Shift Up");

            if (_token.IsCancellationRequested)
            {
                Console.WriteLine("[Gambler] [Cancelled] Gambling was cancelled");
                return;
            }

            if (response == Response.Success)
                Console.WriteLine("[Gambler] [Success] Item matches the rules");
            else            
                Console.WriteLine("[Gambler] [Failed] Failed to check item!");
        }
        private async Task SlamAlt()
        {
            if (!_isShiftHeld)
            {
                _simulator.MouseDeltaMove(alt.X, alt.Y, speed);
                await Task.Delay(delay);
                _simulator.Send("RButton Down");
                await Task.Delay(delay);
                _simulator.Send("RButton Up");
                await Task.Delay(delay);
                _simulator.MouseDeltaMove(item.X, item.Y, speed);
                await Task.Delay(delay);
                _simulator.Send("Shift Down");
                await Task.Delay(delay);

                _isShiftHeld = true;
            }

            _simulator.Send("LButton Down");
            await Task.Delay(delay);
            _simulator.Send("LButton Up");
            await Task.Delay(delay);
        }
        private async Task SlamAug()
        {
            if (_isShiftHeld)
            {
                _simulator.Send("Shift Up");
                await Task.Delay(delay);

                _isShiftHeld = false;
            }

            _simulator.MouseDeltaMove(aug.X, aug.Y, speed);
            await Task.Delay(delay);
            _simulator.Send("RButton Down");
            await Task.Delay(delay);
            _simulator.Send("RButton Up");
            await Task.Delay(delay);
            _simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            _simulator.Send("LButton Down");
            await Task.Delay(delay);
            _simulator.Send("LButton Up");
            await Task.Delay(delay);
        }
        private async Task Copy()
        {
            _simulator.Send("Ctrl Down");
            await Task.Delay(delay);
            _simulator.Send("Alt Down");
            await Task.Delay(delay);
            _simulator.Send("C Down");
            await Task.Delay(delay);
            _simulator.Send("C Up");
            await Task.Delay(delay);
            _simulator.Send("Alt Up");
            await Task.Delay(delay);
            _simulator.Send("Ctrl Up");
            await Task.Delay(delay);
        }
        private Response CheckItem()
        {
            string itemContent = _main.Invoke(() => Clipboard.GetText(TextDataFormat.Text));
            if (string.IsNullOrEmpty(itemContent))
            {
                Console.WriteLine("[Gambler] [Failed] Clipboard is empty or item content is not available.");
                _cts.Cancel();
                return Response.Failure;
            }
            _main.Invoke(Clipboard.Clear);

            int hash = itemContent.GetHashCode();

            if (_hash != hash)
                _hash = hash;
            else
            {
                if (count >= maxAttempts)
                {
                    Console.WriteLine("[Gambler] [Failed] Maximum attempts reached. Cancelling.");
                    _cts.Cancel();
                    return Response.Failure;
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

            Console.WriteLine($"----------------------------");
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

                if (!Regex.IsMatch(content, @"fractured", RegexOptions.IgnoreCase) && type != ModifierType.Implicit)
                    Console.WriteLine($"Type={type}, Tier={tier}, Name={name}, Content={content}");

                Modifier parsedMod = new(type, tier, name, content);
                modifiers.Add(parsedMod);
            }

            var required = rules.Where(r => r.Priority >= 1).ToList();
            var optional = rules.Where(r => r.Priority > 0 && r.Priority < 1).ToList();

            int requiredCount = 0;
            int optionalCount = 0;

            int modsCount = modifiers.Count(x => x.Type == ModifierType.Suffix || x.Type == ModifierType.Prefix);

            foreach (var rule in required)
            {
                foreach (var mod in modifiers)
                {
                    if (rule.Type != ModifierType.Any)
                        if (mod.Type != rule.Type)
                            continue;

                    if (mod.Tier > rule.Tier)
                        continue;

                    Regex content = new(rule.Content, RegexOptions.IgnoreCase);
                    if (!content.IsMatch(mod.Content) && !content.IsMatch(mod.Name))
                        continue;

                    requiredCount++;
                }
            }

            if (optional is not null)
                foreach (var rule in optional)
                {
                    foreach (var mod in modifiers)
                    {
                        if (rule.Type != ModifierType.Any && mod.Type != rule.Type)
                            continue;

                        if (mod.Tier > rule.Tier)
                            continue;

                        Regex content = new(rule.Content, RegexOptions.IgnoreCase);
                        if (!content.IsMatch(mod.Content) && !content.IsMatch(mod.Name))
                            continue;

                        optionalCount++;
                    }
                }

            if (required.Count == requiredCount)
            {
                if (optional?.Count > 0 && optionalCount == 0)
                {
                    if (modsCount == 1)
                    {
                        return Response.Aug;
                    }
                    else
                    {
                        return Response.Alt;
                    }
                }

                return Response.Success;
            }

            if (modsCount == 1)
            {
                return Response.Aug;
            }
            else
            {
                return Response.Alt;
            }
        }
    }
}
