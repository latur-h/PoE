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
    public class Map : IGamba
    {
        private readonly Main _main;
        private readonly Simulator simulator;

        private double speed = 10.0;
        private TimeSpan delay = TimeSpan.FromMilliseconds(10);

        private readonly Coordinates item;
        private readonly Coordinates alchimka;
        private readonly Coordinates scouring;

        private readonly List<Rule> rules = [];

        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;

        private int _hash = 0;
        private int count = 0;
        private int maxAttempts = 3;

        private bool _isShiftHeld = false;

        public Map(Main main, Simulator simulator, CancellationTokenSource cts, TimeSpan delay, double speed, Coordinates item, Coordinates alchimka, Coordinates scouring, List<Rule> rules)
        {
            _main = main;
            this.simulator = simulator;

            this.delay = delay;
            this.speed = speed;

            _cts = cts;
            _token = _cts.Token;

            this.item = item;
            this.alchimka = alchimka;
            this.scouring = scouring;

            this.rules = rules;
        }

        public async Task Gamble() 
        {
            simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            await Copy();

            bool status = CheckItem();

            simulator.MouseDeltaMove(alchimka.X, alchimka.Y, speed);
            await Task.Delay(delay);

            simulator.Send("RButton Down");
            await Task.Delay(delay);
            simulator.Send("RButton Up");
            await Task.Delay(delay);

            simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            simulator.Send("Shift Down");
            await Task.Delay(delay);
            _isShiftHeld = true;

            while (!status && !_token.IsCancellationRequested)
            {
                simulator.Send("Alt Down");
                await Task.Delay(delay);
                simulator.Send("LButton Down");
                await Task.Delay(delay);
                simulator.Send("LButton Up");
                await Task.Delay(delay);
                simulator.Send("Alt Up");
                await Task.Delay(delay);

                simulator.Send("LButton Down");
                await Task.Delay(delay);
                simulator.Send("LButton Up");
                await Task.Delay(delay);

                await Copy();

                status = CheckItem();
            }

            if(_token.IsCancellationRequested)
            {
                Console.WriteLine("[Gambler] [Cancelled] Gambling was cancelled");

                if (_isShiftHeld)
                    simulator.Send("Shift Up");

                return;
            }

            if (_isShiftHeld)
                simulator.Send("Shift Up");

            Console.WriteLine($"[Gambler] [Success] Item matches the rules");
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
                Console.WriteLine("[Gambler] [Warning] Failed to get item content from clipboard. Try to increase the delay between actions if error is persist.");
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
                    Console.WriteLine("[Gambler] [Failed] Maximum attempts reached. Cancelling.");
                    _cts.Cancel();
                    return false;
                }

                count++;
            }

            if (!Regex.IsMatch(itemContent, @"item\sclass:\s(?>maps|expedition logbooks)", RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                Console.WriteLine("[Gambler] [Warning] Item is not a map.");
                _cts.Cancel();
                return false;
            }

            var _mapPercents = rules.Where(x => x.Priority > -1 && x.Priority < 1 && !string.IsNullOrEmpty(x.Content));
            if (_mapPercents.Count() > 0)
            {
                if (_mapPercents.Any(x => Regex.IsMatch(x.Content, @"q\d+r\d+ps\d+", RegexOptions.IgnoreCase | RegexOptions.Singleline)))
                {
                    Rule mapPercent = _mapPercents.First(x => x.Priority > -1 && x.Priority < 1 && Regex.IsMatch(x.Content, @"q\d+r\d+ps\d+", RegexOptions.IgnoreCase | RegexOptions.Singleline));
                    var percent = Regex.Match(mapPercent.Content, @"q(?'quantity'\d+)r(?'rarity'\d+)ps(?'packsize'\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                    if (percent.Success)
                    {
                        int _q = int.Parse(percent.Groups["quantity"].Value);
                        int _r = int.Parse(percent.Groups["rarity"].Value);
                        int _ps = int.Parse(percent.Groups["packsize"].Value);

                        Regex quantity = new(@"quantity:\s\+(?'number'\d+)%", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Regex rarity = new(@"rarity:\s\+(?'number'\d+)%", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        Regex packSize = new(@"pack\ssize:\s\+(?'number'\d+)%", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        int _mapQuantity = 0;
                        int _mapRarity = 0;
                        int _mapPackSize = 0;

                        if (quantity.IsMatch(itemContent))
                            _mapQuantity = int.Parse(quantity.Match(itemContent).Groups["number"].Value);
                        if (rarity.IsMatch(itemContent))
                            _mapRarity = int.Parse(rarity.Match(itemContent).Groups["number"].Value);
                        if (packSize.IsMatch(itemContent))
                            _mapPackSize = int.Parse(packSize.Match(itemContent).Groups["number"].Value);

                        Console.WriteLine($"Q{_mapQuantity}vs{_q};R{_mapRarity}vs{_r};PS{_mapPackSize}vs{_ps}");

                        if (_mapQuantity < _q)
                            return false;
                        if (_mapRarity < _r)
                            return false;
                        if (_mapPackSize < _ps)
                            return false;
                    }
                }
            }

            Regex getModifiers = new(@"\{.*?\}.*?(?={|--------|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            Regex getType = new(@"\{.*?(?'Type'implicit|prefix|suffix).*?\}", RegexOptions.IgnoreCase);
            Regex getName = new(@"\{.*?""(?'Name'.*?)"".*?\}", RegexOptions.IgnoreCase);
            Regex getTier = new(@"\{.*?\(Tier:\s(?'Tier'\d+)\).*?\}", RegexOptions.IgnoreCase);
            Regex getContent = new(@"}(?'Content'.*?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            Regex strip = new(@"\(\d+-\d+\)", RegexOptions.IgnoreCase);

            var mods = getModifiers.Matches(itemContent);

            if (mods.Count == 0)
                return false;

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

                if (!Regex.IsMatch(content, @"fractured", RegexOptions.IgnoreCase))
                    Console.WriteLine($"Type={type}, Tier={tier}, Name={name}, Content={content}");

                Modifier parsedMod = new(type, tier, name, content);
                modifiers.Add(parsedMod);
            }

            var include = rules.Where(x => x.Priority >= 1).ToList();
            int includeCount = 0;
            var exclude = rules.Where(x => x.Priority <= -1).ToList();

            foreach (var mod in modifiers)
            {
                foreach (var _mod in include)
                {
                    if (/*Regex.IsMatch(mod.Content, _mod.Content, RegexOptions.IgnoreCase) || */Regex.IsMatch(mod.Name, _mod.Content, RegexOptions.IgnoreCase))
                        includeCount++;
                }
                foreach (var _mod in exclude)
                {
                    if (/*Regex.IsMatch(mod.Content, _mod.Content, RegexOptions.IgnoreCase) || */Regex.IsMatch(mod.Name, _mod.Content, RegexOptions.IgnoreCase))
                        return false;
                }
            }

            if (include.Count == includeCount)
                return true;

            return false;
        }
    }
}
