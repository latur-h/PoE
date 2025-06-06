using InputSimulator;
using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Modes;
using PoE.dlls.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Gamba
{
    internal class Gambler
    {
        private readonly IGamba gamba;

        private CancellationTokenSource? _cts;
        private CancellationToken _token;

        public Gambler(Main _main, Simulator simulator, TimeSpan delay, GambleType type, Coordinates itemXY, Coordinates baseXY, Coordinates secondXY, List<Rule> rules)
        {
            Console.WriteLine($"[Gambler] Initialize 'Gambler'...");

            _cts = new CancellationTokenSource();
            _token = _cts.Token;

            gamba = type switch
            {
                GambleType.Alt => new Alt(_main, simulator, _cts, delay, itemXY, baseXY, rules),
                GambleType.Alt_Aug => new Alt_Aug(_main, simulator, _cts, delay, itemXY, baseXY, secondXY, rules),
                GambleType.Chaos => new Chaos(_main, simulator, _cts, delay, itemXY, baseXY, rules),
                GambleType.Chromatic => new Chromatic(_main, simulator, _cts, delay, itemXY, baseXY, rules),
                GambleType.Essence => new Essence(_main, simulator, _cts, delay, itemXY, baseXY, rules),
                GambleType.Map => new Map(_main, simulator, _cts, delay, itemXY, baseXY, secondXY, rules),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            Console.WriteLine($"[Gambler] Gambler initialized with type: {type}, delay: {delay.TotalMilliseconds}");
        }

        public async Task StartGambling()
        {
            if (_cts is null || _token.IsCancellationRequested) return;

            Console.WriteLine("[Gambler] Starting gambling...");

            await gamba.Gamble();

            _cts?.Dispose();
            _cts = null;
        }
        public void StopGambling()
        {
            if (_cts is not null && !_token.IsCancellationRequested)
            {
                Console.WriteLine("[Gambler] Stop gambling is requested");
                _cts.Cancel();
            }
        }
        public bool IsRunning() => _cts is not null && !_token.IsCancellationRequested;
    }
}
