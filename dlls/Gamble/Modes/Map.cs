using Poss.Win.Automation.Input;
using PoE.dlls.Logger;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.InteropServices;
using System.Text.RegularExpressions;

namespace PoE.dlls.Gamble.Modes
{
    public class Map : IGamba
    {
        private readonly Main _main;
        private readonly InputSimulator simulator;

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

        public Map(Main main, InputSimulator simulator, CancellationTokenSource cts, TimeSpan delay, double speed, Coordinates item, Coordinates alchimka, Coordinates scouring, List<Rule> rules)
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

            if (_token.IsCancellationRequested)
            {
                GamblerLog.Cancelled();

                if (_isShiftHeld)
                    simulator.Send("Shift Up");

                return;
            }

            if (_isShiftHeld)
                simulator.Send("Shift Up");

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
            if (!MapCheckHelper.TryEvaluateClipboard(_main, _cts, itemContent, rules, ref _hash, ref count, maxAttempts, out var result))
                return false;

            return result.RulesPassed;
        }
    }
}
