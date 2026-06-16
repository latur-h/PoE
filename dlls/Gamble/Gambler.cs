using PoE.dlls.Automation;
using PoE.dlls.GameData;
using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Bulk;
using PoE.dlls.Gamble.Modes;
using PoE.dlls.InteropServices;
using PoE.dlls.Logger;

namespace PoE.dlls.Gamba
{
    internal class Gambler
    {
        private readonly InputSimulatorHost _inputHost;
        private readonly IGamba gamba;

        private CancellationTokenSource? _cts;
        private CancellationToken _token;

        private readonly GambleType _gambleType;
        private readonly ModCacheDatabase? _modCacheDatabase;

        public Gambler(
            Main _main,
            InputSimulatorHost inputHost,
            TimeSpan delay,
            double speed,
            GambleType type,
            Coordinates itemXY,
            Coordinates baseXY,
            Coordinates secondXY,
            Coordinates thirdXY,
            List<Rule> rules,
            MapGambleSession? mapSession = null,
            ModCacheDatabase? modCacheDatabase = null)
        {
            _inputHost = inputHost;
            _gambleType = type;
            _modCacheDatabase = modCacheDatabase;
            GamblerLog.Info("Initialize 'Gambler'...");

            _cts = new CancellationTokenSource();
            _token = _cts.Token;

            var session = mapSession ?? new MapGambleSession(false, new Coordinates(0, 0), thirdXY, baseXY, null, null);

            if (session.BulkCells is { Count: > 0 } cells && type is GambleType.Map or GambleType.MapExalt or GambleType.MapT17)
            {
                gamba = new MapBulkGambler(
                    _main,
                    inputHost,
                    _cts,
                    delay,
                    speed,
                    type,
                    cells,
                    baseXY,
                    session.Exalt,
                    session.Chaos,
                    session.Vaal,
                    session.CorruptOnSuccess,
                    session.BulkGrid,
                    rules);
            }
            else
            {
                gamba = type switch
                {
                    GambleType.Alt => new Alt(_main, inputHost, _cts, delay, speed, itemXY, baseXY, rules),
                    GambleType.Alt_Aug => new Alt_Aug(_main, inputHost, _cts, delay, speed, itemXY, baseXY, secondXY, rules),
                    GambleType.Chaos => new Chaos(_main, inputHost, _cts, delay, speed, itemXY, baseXY, rules),
                    GambleType.Chromatic => new Chromatic(_main, inputHost, _cts, delay, speed, itemXY, baseXY, rules),
                    GambleType.Essence => new Essence(_main, inputHost, _cts, delay, speed, itemXY, baseXY, rules),
                    GambleType.Map => new Map(_main, inputHost, _cts, delay, speed, itemXY, baseXY, secondXY, rules, session),
                    GambleType.MapT17 => new MapT17(_main, inputHost, _cts, delay, speed, itemXY, baseXY, rules, session),
                    GambleType.MapExalt => new MapExalt(_main, inputHost, _cts, delay, speed, itemXY, baseXY, secondXY, thirdXY, rules, session),
                    GambleType.Harvest => new Harvest(_main, inputHost, _cts, delay, speed, itemXY, baseXY, rules),
                    GambleType.Eldritch => new Eldritch(_main, inputHost, _cts, delay, speed, itemXY, baseXY, secondXY, rules),
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
            }

            GamblerLog.Info($"Gambler initialized with type: {type}, delay: {delay.TotalMilliseconds}");
        }

        public async Task StartGambling()
        {
            if (_cts is null || _token.IsCancellationRequested) return;

            GamblerLog.Info("Starting gambling...");

            try
            {
                GambleModContentMatcher.SetCatalogContext(_modCacheDatabase, _gambleType);
                await gamba.Gamble();
            }
            finally
            {
                GambleModContentMatcher.ClearCatalogContext();
                GambleInputReleaseHelper.ReleaseModifiers(_inputHost);
                _cts?.Dispose();
                _cts = null;
            }
        }

        public void StopGambling()
        {
            if (_cts is not null && !_token.IsCancellationRequested)
            {
                GamblerLog.Info("Stop gambling is requested");
                _cts.Cancel();
            }

            GambleInputReleaseHelper.ReleaseAll(_inputHost);
        }
        public bool IsRunning() => _cts is not null && !_token.IsCancellationRequested;
    }
}
