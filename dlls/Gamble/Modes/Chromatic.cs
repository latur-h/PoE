using PoE.dlls.InteropServices;
using PoE.dlls.Logger;
using Poss.Win.Automation.Input;
using PoE.dlls.Automation;
using System.Text.RegularExpressions;

namespace PoE.dlls.Gamble.Modes
{
    public class Chromatic : IGamba
    {
        private readonly Main _main;
        private readonly InputSimulatorHost inputHost;

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

        public Chromatic(Main main, InputSimulatorHost inputHost, CancellationTokenSource cts, TimeSpan delay, double speed, Coordinates item, Coordinates orb, List<Rule> rules)
        {
            _main = main;
            this.inputHost = inputHost;

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
            inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            await Copy();

            if (CheckItem())
            {
                GamblerLog.Success();
                return;
            }

            await Task.Delay(delay);
            inputHost.Simulator.MouseDeltaMove(orb.X, orb.Y, speed);
            inputHost.Simulator.Send("RButton Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("RButton Up");
            await Task.Delay(delay);
            inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            inputHost.Simulator.Send("Shift Down");
            await Task.Delay(delay);
            while (!_token.IsCancellationRequested)
            {
                inputHost.Simulator.Send("LButton Down");
                await Task.Delay(delay);
                inputHost.Simulator.Send("LButton Up");
                await Task.Delay(delay);

                await Copy();

                if (CheckItem())
                    break;
            }
            await Task.Delay(delay);
            inputHost.Simulator.Send("Shift Up");

            if (_token.IsCancellationRequested)
            {
                GamblerLog.Cancelled();
                return;
            }

            GamblerLog.Success();
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
