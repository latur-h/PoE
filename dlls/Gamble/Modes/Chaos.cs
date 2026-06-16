using Poss.Win.Automation.Input;
using PoE.dlls.Automation;
using PoE.dlls.Logger;
using PoE.dlls.InteropServices;

namespace PoE.dlls.Gamble.Modes
{
    public class Chaos : IGamba
    {
        private readonly Main _main;
        private readonly InputSimulatorHost inputHost;
        private readonly double speed;
        private readonly TimeSpan delay;
        private readonly Coordinates item;
        private readonly Coordinates orb;
        private readonly List<Rule> rules;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;
        private readonly GambleItemClipboardHelper.HashState _hashState = new();

        public Chaos(Main main, InputSimulatorHost inputHost, CancellationTokenSource cts, TimeSpan delay, double speed, Coordinates item, Coordinates orb, List<Rule> rules)
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
            await Task.Delay(delay, _token);

            if (await GambleItemMatcher.TryMatchRulesAfterCopyAsync(
                    _main, inputHost, delay, _token, _cts, _hashState, rules))
            {
                GamblerLog.Success();
                return;
            }

            await PickUpOrbAsync();

            bool afterSlam = false;
            int? baselineHash = null;
            bool matched = false;

            while (!_token.IsCancellationRequested)
            {
                if (await GambleItemMatcher.TryMatchRulesAfterCopyAsync(
                        _main, inputHost, delay, _token, _cts, _hashState, rules, afterSlam, baselineHash))
                {
                    matched = true;
                    break;
                }

                baselineHash = _hashState.Hash;
                await SlamAsync();
                afterSlam = true;
            }

            inputHost.Simulator.Send("Shift Up");

            if (_token.IsCancellationRequested)
            {
                GamblerLog.Cancelled();
                return;
            }

            if (matched)
                GamblerLog.Success();
            else
                GamblerLog.Error("Failed to check item!");
        }

        private async Task PickUpOrbAsync()
        {
            await Task.Delay(delay, _token);
            inputHost.Simulator.MouseDeltaMove(orb.X, orb.Y, speed);
            inputHost.Simulator.Send("RButton Down");
            await Task.Delay(delay, _token);
            inputHost.Simulator.Send("RButton Up");
            await Task.Delay(delay, _token);
            inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay, _token);
            inputHost.Simulator.Send("Shift Down");
            await Task.Delay(delay, _token);
        }

        private async Task SlamAsync()
        {
            inputHost.Simulator.Send("LButton Down");
            await Task.Delay(delay, _token);
            inputHost.Simulator.Send("LButton Up");
            await Task.Delay(delay, _token);
        }
    }
}
