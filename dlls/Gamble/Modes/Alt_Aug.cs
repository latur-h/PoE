using Poss.Win.Automation.Input;
using PoE.dlls.Automation;
using PoE.dlls.Logger;
using PoE.dlls.InteropServices;

namespace PoE.dlls.Gamble.Modes
{
    public class Alt_Aug : IGamba
    {
        private readonly Main _main;
        private readonly InputSimulatorHost _inputHost;
        private readonly double speed;
        private readonly TimeSpan delay;
        private readonly Coordinates item;
        private readonly Coordinates alt;
        private readonly Coordinates aug;
        private readonly List<Rule> rules;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;
        private readonly GambleItemClipboardHelper.HashState _hashState = new();
        private bool _isShiftHeld;

        public Alt_Aug(Main main, InputSimulatorHost inputHost, CancellationTokenSource cts, TimeSpan delay, double speed, Coordinates item, Coordinates alt, Coordinates aug, List<Rule> rules)
        {
            _main = main;
            _inputHost = inputHost;
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
            _inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay, _token);

            AltAugResponse response = await GambleItemMatcher.ReadAltAugAfterCopyAsync(
                _main, _inputHost, delay, _token, _cts, _hashState, rules);

            if (response == AltAugResponse.Success)
            {
                GamblerLog.Success();
                return;
            }

            if (response == AltAugResponse.Failure)
            {
                GamblerLog.Cancelled();
                return;
            }

            await PickUpOrbAsync();

            bool afterSlam = false;
            int? baselineHash = null;

            while (response != AltAugResponse.Success && !_token.IsCancellationRequested)
            {
                response = await GambleItemMatcher.ReadAltAugAfterCopyAsync(
                    _main, _inputHost, delay, _token, _cts, _hashState, rules, afterSlam, baselineHash);

                if (response == AltAugResponse.Success)
                    break;

                if (response == AltAugResponse.Failure)
                    break;

                baselineHash = _hashState.Hash;

                if (response == AltAugResponse.Alt)
                    await SlamAltAsync();
                else
                    await SlamAugAsync();

                afterSlam = true;
            }

            if (_isShiftHeld)
                _inputHost.Simulator.Send("Shift Up");

            if (_token.IsCancellationRequested || response == AltAugResponse.Failure)
            {
                GamblerLog.Cancelled();
                return;
            }

            if (response == AltAugResponse.Success)
                GamblerLog.Success();
            else
                GamblerLog.Error("Failed to check item!");
        }

        private async Task PickUpOrbAsync()
        {
            _inputHost.Simulator.MouseDeltaMove(alt.X, alt.Y, speed);
            await Task.Delay(delay, _token);
            _inputHost.Simulator.Send("RButton Down");
            await Task.Delay(delay, _token);
            _inputHost.Simulator.Send("RButton Up");
            await Task.Delay(delay, _token);
            _inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay, _token);
            _inputHost.Simulator.Send("Shift Down");
            await Task.Delay(delay, _token);
            _isShiftHeld = true;
        }

        private async Task SlamAltAsync()
        {
            _inputHost.Simulator.Send("LButton Down");
            await Task.Delay(delay, _token);
            _inputHost.Simulator.Send("LButton Up");
            await Task.Delay(delay, _token);
        }

        private async Task SlamAugAsync()
        {
            _inputHost.Simulator.Send("Alt Down");
            await Task.Delay(delay, _token);
            _inputHost.Simulator.Send("LButton Down");
            await Task.Delay(delay, _token);
            _inputHost.Simulator.Send("LButton Up");
            await Task.Delay(delay, _token);
            _inputHost.Simulator.Send("Alt Up");
            await Task.Delay(delay, _token);
        }
    }
}
