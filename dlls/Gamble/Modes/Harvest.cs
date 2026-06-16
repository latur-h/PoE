using PoE.dlls.Automation;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.InteropServices;
using PoE.dlls.Logger;
using PoE.dlls.Settings.Mods;
using Poss.Win.Automation.Input;

namespace PoE.dlls.Gamble.Modes
{
    public class Harvest : IGamba
    {
        private readonly Main _main;
        private readonly InputSimulatorHost inputHost;
        private readonly double speed;
        private readonly TimeSpan delay;
        private readonly Coordinates item;
        private readonly Coordinates button;
        private readonly List<Rule> rules;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;
        private readonly GambleItemClipboardHelper.HashState _hashState = new();

        public Harvest(Main main, InputSimulatorHost inputHost, CancellationTokenSource cts, TimeSpan delay, double speed, Coordinates item, Coordinates button, List<Rule> rules)
        {
            _main = main;
            this.inputHost = inputHost;
            this.delay = delay;
            this.speed = speed;
            _cts = cts;
            _token = _cts.Token;
            this.item = item;
            this.button = button;
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

            bool afterCraft = false;
            int? baselineHash = null;
            bool matched = false;

            while (!_token.IsCancellationRequested)
            {
                if (await GambleItemMatcher.TryMatchRulesAfterCopyAsync(
                        _main, inputHost, delay, _token, _cts, _hashState, rules, afterCraft, baselineHash))
                {
                    matched = true;
                    break;
                }

                baselineHash = _hashState.Hash;
                await CraftAsync();
                afterCraft = true;
            }

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

        private async Task CraftAsync()
        {
            inputHost.Simulator.MouseDeltaMove(button.X, button.Y, speed);
            await Task.Delay(delay, _token);
            inputHost.Simulator.Send("LButton Down");
            await Task.Delay(delay, _token);
            inputHost.Simulator.Send("LButton Up");
            await Task.Delay(delay, _token);
            inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay, _token);
        }
    }
}
