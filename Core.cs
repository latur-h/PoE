using PoE.dlls.Flasks.Base;
using PoE.dlls.Gamba;
using PoE.dlls.Gamble;
using PoE.dlls.InteropServices;
using PoE.dlls.Logger;
using PoE.dlls.Settings.Mods;

namespace PoE
{
    public partial class Main
    {
        private Gambler? Gambler;

        private async Task Init()
        {
            _hotkeys.Start();

            _hotkeys.Register("Register Flask", RegisterFlasks, _settings.FlaskControls.RegisterKey);
            _hotkeys.Register("Drink Flask", DrinkFlasks, _settings.FlaskControls.DrinkKey);
            _hotkeys.Register("Stop Drinking", StopDrinking, _settings.FlaskControls.StopKey);

            _hotkeys.Register("Gambler get coordinates", GamblerGetCoordinates, _settings.Modifiers.GetCoorinatesKey);
            _hotkeys.Register("Gambler start", GamblerStart, _settings.Modifiers.GamblerStart);
            _hotkeys.Register("Gambler stop", GamblerStop, _settings.Modifiers.GamblerStop);

            await Task.Run(async () =>
            {
                while (true)
                {
                    if (_inputHost.Simulator.GetKeyState("XButton1"))
                    {
                        _inputHost.Simulator.Send("LButton Down");
                        await Task.Delay(20);
                        _inputHost.Simulator.Send("LButton Up");
                        await Task.Delay(20);
                    }
                }
            });
        }

        #region Flasks
        private Task RegisterFlasks()
        {
            Invoke(() =>
            {
                _flaskManager.Flush();

                if (_settings.Flasks["1"].Active)
                {
                    FlaskType type = Enum.Parse<FlaskType>(_settings.Flasks["1"].FlaskType);
                    int number = 1;

                    if (type == FlaskType.HP || type == FlaskType.MP)
                        number = _settings.Flasks["1"].Percent;

                    string key = _settings.Flasks["1"].Key;

                    _flaskManager.RegisterFlask(type, number, key);
                }

                if (_settings.Flasks["2"].Active)
                {
                    FlaskType type = Enum.Parse<FlaskType>(_settings.Flasks["2"].FlaskType);
                    int number = 2;

                    if (type == FlaskType.HP || type == FlaskType.MP)
                        number = _settings.Flasks["2"].Percent;

                    string key = _settings.Flasks["2"].Key;

                    _flaskManager.RegisterFlask(type, number, key);
                }

                if (_settings.Flasks["3"].Active)
                {
                    FlaskType type = Enum.Parse<FlaskType>(_settings.Flasks["3"].FlaskType);
                    int number = 3;

                    if (type == FlaskType.HP || type == FlaskType.MP)
                        number = _settings.Flasks["3"].Percent;

                    string key = _settings.Flasks["3"].Key;

                    _flaskManager.RegisterFlask(type, number, key);
                }

                if (_settings.Flasks["4"].Active)
                {
                    FlaskType type = Enum.Parse<FlaskType>(_settings.Flasks["4"].FlaskType);
                    int number = 4;

                    if (type == FlaskType.HP || type == FlaskType.MP)
                        number = _settings.Flasks["4"].Percent;

                    string key = _settings.Flasks["4"].Key;

                    _flaskManager.RegisterFlask(type, number, key);
                }

                if (_settings.Flasks["5"].Active)
                {
                    FlaskType type = Enum.Parse<FlaskType>(_settings.Flasks["5"].FlaskType);
                    int number = 5;

                    if (type == FlaskType.HP || type == FlaskType.MP)
                        number = _settings.Flasks["5"].Percent;

                    string key = _settings.Flasks["5"].Key;

                    _flaskManager.RegisterFlask(type, number, key);
                }
            });

            return Task.CompletedTask;
        }
        private Task DrinkFlasks() => _flaskManager.DrinkFlasks();
        private Task StopDrinking()
        {
            _flaskManager.Stop();
            return Task.CompletedTask;
        }
        #endregion
        #region Clicker
        private CancellationTokenSource? cts = null;
        private CancellationToken token;
        private async Task ClickerStart()
        {
            if (cts is null)
            {
                cts = new CancellationTokenSource();
                token = cts.Token;
            }

            if (cts is not null && !token.IsCancellationRequested) return;

            AppLog.System(LogSeverity.Info, "Clicker run...");

            AppLog.System(LogSeverity.Debug, token.IsCancellationRequested.ToString());

            while (!token.IsCancellationRequested)
            {
                _inputHost.Simulator.Send("LButton Down");
                await Task.Delay(20);
                _inputHost.Simulator.Send("LButton Up");
                await Task.Delay(20);
            }

            AppLog.System(LogSeverity.Info, "Clicker stop...");

            cts?.Dispose();
            cts = null;
            token = CancellationToken.None;
        }
        private Task ClickerStop()
        {
            if (cts is null || token.IsCancellationRequested) return Task.CompletedTask;

            cts.Cancel();

            return Task.CompletedTask;
        }
        #endregion
        #region Gambler
        private Task GamblerGetCoordinates()
        {
            Invoke(() => TryApplyRecordedCoordinate());
            return Task.CompletedTask;
        }

        private async Task GamblerStart()
        {
            if (Gambler is not null && Gambler.IsRunning())
                return;

            var preset = _settings.Modifiers.GetActivePreset();

            List<Rule> rules = preset.Rules
                .Where(r => !string.IsNullOrEmpty(r.Content))
                .Take(GambleModeLayout.MaxRules)
                .Select(r => new Rule(r.Priority, r.ModifierType, r.Tier, r.Content))
                .ToList();

            if (rules.Count == 0)
                return;

            var coords = GambleCoordinateResolver.Resolve(
                _settings.Modifiers.GambleType,
                _settings.Modifiers.Items,
                _settings.Modifiers.Orbs);

            Gambler = new Gambler(this, _inputHost, TimeSpan.FromMilliseconds(_settings.Modifiers.Delay), _settings.Modifiers.Speed,
                _settings.Modifiers.GambleType, coords.Item, coords.Base, coords.Second, coords.Third, rules);

            await Gambler.StartGambling();

            Gambler = null;
        }
        private Task GamblerStop()
        {
            if (Gambler is not null && Gambler.IsRunning())
                Gambler.StopGambling();

            return Task.CompletedTask;
        }
        #endregion
    }
}
