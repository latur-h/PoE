using PoE.dlls.Automation;
using PoE.dlls.Gamba;
using PoE.dlls.Logger;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.InteropServices;
using System.Text.RegularExpressions;

namespace PoE.dlls.Gamble.Modes
{
    public class Map : IGamba
    {
        private readonly Main _main;
        private readonly InputSimulatorHost inputHost;

        private double speed = 10.0;
        private TimeSpan delay = TimeSpan.FromMilliseconds(10);

        private readonly Coordinates item;
        private readonly Coordinates alchimka;
        private readonly Coordinates scouring;

        private readonly List<Rule> rules = [];

        private readonly MapGambleSession _session;

        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;

        private int _hash = 0;
        private int count = 0;
        private int maxAttempts = 3;

        private bool _isShiftHeld = false;

        public Map(Main main, InputSimulatorHost inputHost, CancellationTokenSource cts, TimeSpan delay, double speed, Coordinates item, Coordinates alchimka, Coordinates scouring, List<Rule> rules, MapGambleSession session)
        {
            _main = main;
            this.inputHost = inputHost;

            this.delay = delay;
            this.speed = speed;

            _cts = cts;
            _token = _cts.Token;

            this.item = item;
            this.alchimka = alchimka;
            this.scouring = scouring;

            this.rules = rules;
            _session = session;
        }

        public async Task Gamble()
        {
            inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            await Copy();

            bool status = CheckItem();

            inputHost.Simulator.MouseDeltaMove(alchimka.X, alchimka.Y, speed);
            await Task.Delay(delay);

            inputHost.Simulator.Send("RButton Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("RButton Up");
            await Task.Delay(delay);

            inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            inputHost.Simulator.Send("Shift Down");
            await Task.Delay(delay);
            _isShiftHeld = true;

            while (!status && !_token.IsCancellationRequested)
            {
                inputHost.Simulator.Send("Alt Down");
                await Task.Delay(delay);
                inputHost.Simulator.Send("LButton Down");
                await Task.Delay(delay);
                inputHost.Simulator.Send("LButton Up");
                await Task.Delay(delay);
                inputHost.Simulator.Send("Alt Up");
                await Task.Delay(delay);

                inputHost.Simulator.Send("LButton Down");
                await Task.Delay(delay);
                inputHost.Simulator.Send("LButton Up");
                await Task.Delay(delay);

                await Copy();

                status = CheckItem();
            }

            if (_token.IsCancellationRequested)
            {
                GamblerLog.Cancelled();

                if (_isShiftHeld)
                    inputHost.Simulator.Send("Shift Up");

                return;
            }

            if (_isShiftHeld)
                inputHost.Simulator.Send("Shift Up");

            var corruptResult = await MapCorruptHelper.TryFinishWithOptionalCorruptAsync(
                _main,
                inputHost,
                _token,
                delay,
                speed,
                item,
                _session.Vaal,
                _session.CorruptOnSuccess,
                _session.RequiresEightModsAfterCorrupt,
                rules);

            if (_token.IsCancellationRequested)
            {
                GamblerLog.Cancelled();
                return;
            }

            if (corruptResult == false)
                return;

            if (corruptResult == true)
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
            if (!MapCheckHelper.TryEvaluateClipboard(_main, _cts, itemContent, rules, ref _hash, ref count, maxAttempts, out var result))
                return false;

            return result.RulesPassed;
        }
    }
}
