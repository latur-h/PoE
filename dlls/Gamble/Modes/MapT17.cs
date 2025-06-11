﻿using InputSimulator;
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
    public class MapT17 : IGamba
    {
        private readonly Main _main;
        private readonly Simulator simulator;

        private double speed = 10.0;
        private TimeSpan delay = TimeSpan.FromMilliseconds(10);

        private readonly Coordinates item;
        private readonly Coordinates chaos;

        private readonly List<Rule> rules = [];

        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;

        private int _hash = 0;
        private int count = 0;
        private int maxAttempts = 10;

        public MapT17(Main main, Simulator simulator, CancellationTokenSource cts, TimeSpan delay, double speed, Coordinates item, Coordinates chaos, List<Rule> rules)
        {
            _main = main;
            this.simulator = simulator;

            this.delay = delay;
            this.speed = speed;

            _cts = cts;
            _token = _cts.Token;

            this.item = item;
            this.chaos = chaos;

            this.rules = rules;
        }

        public async Task Gamble()
        {
            simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            await Copy();

            bool status = CheckItem();

            if (!status) 
            {
                simulator.MouseDeltaMove(chaos.X, chaos.Y, speed);
                await Task.Delay(delay);

                simulator.Send("RButton Down");
                await Task.Delay(delay);
                simulator.Send("RButton Up");
                await Task.Delay(delay);

                simulator.Send("Shift Down");
                await Task.Delay(delay);

                simulator.MouseDeltaMove(item.X, item.Y, speed);
                await Task.Delay(delay);

                while (!status && !_token.IsCancellationRequested)
                {
                    simulator.Send("LButton Down");
                    await Task.Delay(delay);
                    simulator.Send("LButton Up");
                    await Task.Delay(delay);

                    await Copy();

                    status = CheckItem();
                }

                simulator.Send("Shift Up");
                await Task.Delay(delay);
            }

            if (_token.IsCancellationRequested)
            {
                Console.WriteLine("[Gambler] [Cancelled] Gambling was cancelled");
                return;
            }

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
            
            var _mapPercents = rules.Where(x => x.Priority > -1 && x.Priority < 1 && !string.IsNullOrEmpty(x.Content));
            if (_mapPercents.Count() > 0)
            {
                if (_mapPercents.Any(x => Regex.IsMatch(x.Content, @"q\d+r\d+ps\d+", RegexOptions.IgnoreCase | RegexOptions.Singleline)))
                {
                    Rule mapPercent = _mapPercents.First(x => Regex.IsMatch(x.Content, @"q\d+r\d+ps\d+", RegexOptions.IgnoreCase | RegexOptions.Singleline));
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
                if (_mapPercents.Any(x => Regex.IsMatch(x.Content, @".*?:\d+(?>;|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline)))
                {
                    Rule moreRule = _mapPercents.First(x => Regex.IsMatch(x.Content, @".*?:\d+(?>;|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline));
                    Regex moreRuleRegex = new(@"(?'type'.*?):(?'number'\d+)(?>;|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    var moreRuleMatches = moreRuleRegex.Matches(moreRule.Content);

                    Regex moreMapRegex = new(@"more\s(?'type'.*?):\s\+(?'number'\d+)%\s\(augmented\)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    var moreMapMatches = moreMapRegex.Matches(itemContent).Cast<Match>().ToList();

                    int matchCount = 0;

                    foreach(Match matchbyRule in moreRuleMatches)
                    {
                        foreach(Match matchbyMap in moreMapMatches)
                        {
                            if (string.Equals(matchbyRule.Groups["type"].Value, matchbyMap.Groups["type"].Value, StringComparison.OrdinalIgnoreCase))
                            {
                                int leftOperand = 0, rightOperand = 0;

                                if (int.TryParse(matchbyRule.Groups["number"].Value, out int leftNumber))
                                    leftOperand = leftNumber;
                                if (int.TryParse(matchbyMap.Groups["number"].Value, out int rightNumber))
                                    rightOperand = rightNumber;

                                if (leftNumber < rightNumber)
                                    matchCount++;

                                Console.WriteLine($"{leftNumber}vs{rightNumber}");
                                Console.WriteLine($"{matchCount}");
                            }
                        }
                    }

                    if (moreRuleMatches.Count != matchCount)
                        return false;
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
                    if (Regex.IsMatch(mod.Content, _mod.Content))
                        includeCount++;
                }
                foreach (var _mod in exclude)
                {
                    if (Regex.IsMatch(mod.Content, _mod.Content))
                        return false;
                }
            }

            if (include.Count == includeCount)
                return true;

            return false;
        }
    }
}
