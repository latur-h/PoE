using PoE.dlls.Automation;
using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Modes;
using PoE.dlls.InteropServices;
using PoE.dlls.Logger;

namespace PoE.dlls.Gamba
{
    internal class Gambler
    {
        private readonly IGamba gamba;

        private CancellationTokenSource? _cts;
        private CancellationToken _token;

        public Gambler(Main _main, InputSimulatorHost inputHost, TimeSpan delay, double speed, GambleType type, Coordinates itemXY, Coordinates baseXY, Coordinates secondXY, Coordinates thirdXY, List<Rule> rules)
        {
            GamblerLog.Info("Initialize 'Gambler'...");

            _cts = new CancellationTokenSource();
            _token = _cts.Token;

            gamba = type switch
            {
                GambleType.Alt => new Alt(_main, inputHost, _cts, delay, speed, itemXY, baseXY, rules),
                GambleType.Alt_Aug => new Alt_Aug(_main, inputHost, _cts, delay, speed, itemXY, baseXY, secondXY, rules),
                GambleType.Chaos => new Chaos(_main, inputHost, _cts, delay, speed, itemXY, baseXY, rules),
                GambleType.Chromatic => new Chromatic(_main, inputHost, _cts, delay, speed, itemXY, baseXY, rules),
                GambleType.Essence => new Essence(_main, inputHost, _cts, delay, speed, itemXY, baseXY, rules),
                GambleType.Map => new Map(_main, inputHost, _cts, delay, speed, itemXY, baseXY, secondXY, rules),
                GambleType.MapT17 => new MapT17(_main, inputHost, _cts, delay, speed, itemXY, baseXY, rules),
                GambleType.MapExalt => new MapExalt(_main, inputHost, _cts, delay, speed, itemXY, baseXY, secondXY, thirdXY, rules),
                GambleType.Harvest => new Harvest(_main, inputHost, _cts, delay, speed, itemXY, baseXY, rules),
                GambleType.Eldritch => new Eldritch(_main, inputHost, _cts, delay, speed, itemXY, baseXY, rules),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            GamblerLog.Info($"Gambler initialized with type: {type}, delay: {delay.TotalMilliseconds}");
        }

        public async Task StartGambling()
        {
            if (_cts is null || _token.IsCancellationRequested) return;

            GamblerLog.Info("Starting gambling...");

            await gamba.Gamble();

            _cts?.Dispose();
            _cts = null;
        }
        public void StopGambling()
        {
            if (_cts is not null && !_token.IsCancellationRequested)
            {
                GamblerLog.Info("Stop gambling is requested");
                _cts.Cancel();
            }
        }
        public bool IsRunning() => _cts is not null && !_token.IsCancellationRequested;
    }
}
