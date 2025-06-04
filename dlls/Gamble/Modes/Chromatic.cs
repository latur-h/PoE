using InputSimulator;
using PoE.dlls.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PoE.dlls.Gamble.Modes
{
    public class Chromatic : IGamba
    {
        private readonly Main _main;
        private readonly Simulator simulator;

        private double speed = 10.0;
        private TimeSpan delay = TimeSpan.FromMilliseconds(10);

        private readonly Coordinates item;
        private readonly Coordinates orb;

        private readonly List<Rule> rules = [];

        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;

        private int _hash = 0;
        private int count = 0;
        private int maxAttempts = 10;

        public Chromatic(Main main, Simulator simulator, CancellationTokenSource cts, TimeSpan delay, Coordinates item, Coordinates orb, List<Rule> rules)
        {
            _main = main;
            this.simulator = simulator;

            this.delay = delay;

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

            if (CheckItem())
            {
                Console.WriteLine("[Gambler] [Success] Item matches the rules");
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
                Console.WriteLine("[Gambler] [Cancelled] Gambling was cancelled");
                return;
            }

            Console.WriteLine("[Gambler] [Success] Item matches the rules");
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

            Regex getSockets = new(@"([rgbw](\s|-)?){4,6}", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!getSockets.IsMatch(itemContent))
                return false;

            string sockets = getSockets.Match(itemContent).Value;

            int r = sockets.Count(x => x == 'R');
            int g = sockets.Count(x => x == 'G');
            int b = sockets.Count(x => x == 'B');
            int w = sockets.Count(x => x == 'W');

            string rule = rules.FirstOrDefault(x => x.Content.Length > 0).Content.ToUpperInvariant();

            int requiredR = rule.Count(x => x.Equals('R'));
            int requiredG = rule.Count(x => x.Equals('G'));
            int requiredB = rule.Count(x => x.Equals('B'));
            int requiredW = rule.Count(x => x.Equals('W'));

            if (r == requiredR && g == requiredG && b == requiredB && w == requiredW)
                return true;

            return false;
        }
    }
}
