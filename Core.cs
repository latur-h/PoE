using GlobalHotKeys.Structs;
using PoE.dlls.Flasks.Base;
using PoE.dlls.Gamba;
using PoE.dlls.Gamble;
using PoE.dlls.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE
{
    public partial class Main
    {
        private Bind regFlasks;
        private Bind drinkFlasks;
        private Bind stopDrinking;

        private Bind clickerStart;
        private Bind clickerStop;

        private Gambler? Gambler;
        private Bind gamblerGetCoordinates;
        private Bind gamblerStart;
        private Bind gamblerStop;

        private void Init()
        {
            _hotkeys.Start();

            regFlasks = new Bind("Register Flask", RegisterFlasks, KeyCode.F5);
            drinkFlasks = new Bind("Drink Flask", DrinkFlasks, KeyCode.F2);
            stopDrinking = new Bind("Stop Drinking", StopDrinking, KeyCode.F4);

            clickerStart = new Bind("Clicker start", ClickerStart, KeyCode.XButton1);
            clickerStop = new Bind("Clicker stop", ClickerStop, KeyCode.XButton1Up);

            gamblerGetCoordinates = new Bind("Gambler get coordinates", GamblerGetCoordinates, Enum.Parse<KeyCode>(_settings.Modifiers.GetCoorinatesKey));
            gamblerStart = new Bind("Gambler start", GamblerStart, Enum.Parse<KeyCode>(_settings.Modifiers.GamblerStart));
            gamblerStop = new Bind("Gambler stop", GamblerStop, Enum.Parse<KeyCode>(_settings.Modifiers.GamblerStop));

            _hotkeys.Register(regFlasks);
            _hotkeys.Register(drinkFlasks);
            _hotkeys.Register(stopDrinking);

            _hotkeys.Register(clickerStart);
            _hotkeys.Register(clickerStop);

            _hotkeys.Register(gamblerGetCoordinates);
            _hotkeys.Register(gamblerStart);
            _hotkeys.Register(gamblerStop);
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
        private CancellationTokenSource cts = null!;
        private CancellationToken token;
        private bool isClickerRunning = false;
        private async Task ClickerStart()
        {
            if (isClickerRunning) return;

            isClickerRunning = true;

            cts = new CancellationTokenSource();
            token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                _input.Send("LButton Down");
                await Task.Delay(20);
                _input.Send("LButton Up");
                await Task.Delay(20);
            }

            cts.Dispose();
            token = CancellationToken.None;
        }
        private Task ClickerStop()
        {
            if (!isClickerRunning) return Task.CompletedTask;

            isClickerRunning = false;
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

            List<Rule> rules = new List<Rule>();

            if (_settings.Modifiers.Mode.Priority1 > 0)
                rules.Add(new Rule(_settings.Modifiers.Mode.Priority1, _settings.Modifiers.Mode.modifierType1, _settings.Modifiers.Mode.Tier1, _settings.Modifiers.Mode.Content1));
            if (_settings.Modifiers.Mode.Priority2 > 0)
                rules.Add(new Rule(_settings.Modifiers.Mode.Priority2, _settings.Modifiers.Mode.modifierType2, _settings.Modifiers.Mode.Tier2, _settings.Modifiers.Mode.Content2));
            if (_settings.Modifiers.Mode.Priority3 > 0)
                rules.Add(new Rule(_settings.Modifiers.Mode.Priority3, _settings.Modifiers.Mode.modifierType3, _settings.Modifiers.Mode.Tier3, _settings.Modifiers.Mode.Content3));
            if (_settings.Modifiers.Mode.Priority4 > 0)
                rules.Add(new Rule(_settings.Modifiers.Mode.Priority4, _settings.Modifiers.Mode.modifierType4, _settings.Modifiers.Mode.Tier4, _settings.Modifiers.Mode.Content4));
            if (_settings.Modifiers.Mode.Priority5 > 0)
                rules.Add(new Rule(_settings.Modifiers.Mode.Priority5, _settings.Modifiers.Mode.modifierType5, _settings.Modifiers.Mode.Tier5, _settings.Modifiers.Mode.Content5));
            if (_settings.Modifiers.Mode.Priority6 > 0)
                rules.Add(new Rule(_settings.Modifiers.Mode.Priority6, _settings.Modifiers.Mode.modifierType6, _settings.Modifiers.Mode.Tier6, _settings.Modifiers.Mode.Content6));
            if (_settings.Modifiers.Mode.Priority7 > 0)
                rules.Add(new Rule(_settings.Modifiers.Mode.Priority7, _settings.Modifiers.Mode.modifierType7, _settings.Modifiers.Mode.Tier7, _settings.Modifiers.Mode.Content7));
            if (_settings.Modifiers.Mode.Priority8 > 0)
                rules.Add(new Rule(_settings.Modifiers.Mode.Priority8, _settings.Modifiers.Mode.modifierType8, _settings.Modifiers.Mode.Tier8, _settings.Modifiers.Mode.Content8));

            if (rules.Count == 0)
                return;

            Gambler = new Gambler(this, _input, TimeSpan.FromMilliseconds(_settings.Modifiers.Delay),
                _settings.Modifiers.GambleType, _settings.Modifiers.Mode.Item, _settings.Modifiers.Mode.Base, _settings.Modifiers.Mode.Second, rules);

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
