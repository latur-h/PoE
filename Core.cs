using PoE.dlls.Flasks.Base;
using PoE.dlls.Gamba;
using PoE.dlls.Gamble;
using PoE.dlls.InteropServices;
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
                    if (_input.GetKeyState("XButton1"))
                    {
                        _input.Send("LButton Down");
                        await Task.Delay(20);
                        _input.Send("LButton Up");
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

            Console.WriteLine("Clicker run...");

            Console.WriteLine(token.IsCancellationRequested);

            while (!token.IsCancellationRequested)
            {
                _input.Send("LButton Down");
                await Task.Delay(20);
                _input.Send("LButton Up");
                await Task.Delay(20);
            }

            Console.WriteLine("Clicker stop...");

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
        private bool _getCoordinatesItem = false;
        private bool _getCoordinatesBase = false;
        private bool _getCoordinatesSecond = false;

        private Task GamblerGetCoordinates()
        {
            if (_getCoordinatesItem)
            {
                var coordinates = InteropHelper.GetMousePos();
                _settings.Modifiers.Mode.Item = coordinates;

                Invoke(() =>
                {
                    textBox_ItemXY._textBox.Text = $"{_settings.Modifiers.Mode.Item.X}, {_settings.Modifiers.Mode.Item.Y}";

                    button_Record1.Text = "Rec";
                    button_Record1.ForeColor = Color.Black;
                });

                _getCoordinatesItem = false;

                return Task.CompletedTask;
            }
            else if (_getCoordinatesBase)
            {
                var coordinates = InteropHelper.GetMousePos();
                _settings.Modifiers.Mode.Base = coordinates;

                Invoke(() =>
                {
                    textBox_BaseXY._textBox.Text = $"{_settings.Modifiers.Mode.Base.X}, {_settings.Modifiers.Mode.Base.Y}";
                    button_Record2.Text = "Rec";
                    button_Record2.ForeColor = Color.Black;
                });

                _getCoordinatesBase = false;

                return Task.CompletedTask;
            }
            else if (_getCoordinatesSecond)
            {
                var coordinates = InteropHelper.GetMousePos();
                _settings.Modifiers.Mode.Second = coordinates;

                Invoke(() =>
                {
                    textBox_SecondXY._textBox.Text = $"{_settings.Modifiers.Mode.Second.X}, {_settings.Modifiers.Mode.Second.Y}";
                    button_Record3.Text = "Rec";
                    button_Record3.ForeColor = Color.Black;
                });

                _getCoordinatesSecond = false;

                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
        private async Task GamblerStart()
        {
            if (Gambler is not null && Gambler.IsRunning())
                return;

            var store = _settings.Modifiers.GetModeStore(_settings.Modifiers.GambleType);
            var preset = _settings.Modifiers.GetActivePreset();

            List<Rule> rules = preset.Rules
                .Where(r => !string.IsNullOrEmpty(r.Content))
                .Take(GambleModeLayout.MaxRules)
                .Select(r => new Rule(r.Priority, r.ModifierType, r.Tier, r.Content))
                .ToList();

            if (rules.Count == 0)
                return;

            Gambler = new Gambler(this, _input, TimeSpan.FromMilliseconds(_settings.Modifiers.Delay), _settings.Modifiers.Speed,
                _settings.Modifiers.GambleType, store.Item, store.Base, store.Second, rules);

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
