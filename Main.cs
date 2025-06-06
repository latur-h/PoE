using GlobalHotKeys;
using GlobalHotKeys.Structs;
using InputSimulator;
using PoE.dlls;
using PoE.dlls.Flasks;
using PoE.dlls.Flasks.Base;
using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.InteropServices;
using PoE.dlls.Logger;
using PoE.dlls.Settings;
using PoE.dlls.Style;
using System.Windows.Forms;

namespace PoE
{
    public partial class Main : Form
    {
        private readonly Simulator _input;
        private readonly HotKeys _hotkeys;
        private readonly FlaskManager _flaskManager;
        private readonly UserSettings _userSettings;
        private Settings _settings = null!;
        private TextBoxLogger _logger = null!;

        public Main(Simulator input, HotKeys hotkeys, FlaskManager flaskManager, UserSettings userSettings)
        {
            _input = input;
            _hotkeys = hotkeys;
            _flaskManager = flaskManager;
            _userSettings = userSettings;

            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            textBox_Logs.BackColor = StaticColors.BackGround;
            textBox_Logs.ForeColor = StaticColors.ForeGround;
            textBox_Logs.BorderStyle = BorderStyle.None;
            textBox_Logs.Cursor = Cursors.Arrow;
            textBox_Logs.ScrollBars = ScrollBars.Vertical;

            _logger = new TextBoxLogger(textBox_Logs);
            Console.SetOut(new ConsoleLogger(_logger));
            Console.SetError(new ConsoleLogger(_logger));

            _settings = _userSettings.LoadSettings();

            BackColor = StaticColors.BackGround;
            tabPage_Main.BackColor = StaticColors.BackGround;
            tabPage_Gamble.BackColor = StaticColors.BackGround;
            tabPage_Settings.BackColor = StaticColors.BackGround;
            tabPage_Logs.BackColor = StaticColors.BackGround;

            checkBox_Flask1.ForeColor = StaticColors.ForeGround;
            checkBox_Flask2.ForeColor = StaticColors.ForeGround;
            checkBox_Flask3.ForeColor = StaticColors.ForeGround;
            checkBox_Flask4.ForeColor = StaticColors.ForeGround;
            checkBox_Flask5.ForeColor = StaticColors.ForeGround;

            label_Active.ForeColor = StaticColors.ForeGround;
            label_Key.ForeColor = StaticColors.ForeGround;
            label_FlaskType.ForeColor = StaticColors.ForeGround;
            label_Percent.ForeColor = StaticColors.ForeGround;

            label_Flask1_Slider.ForeColor = StaticColors.ForeGround;
            label_Flask2_Slider.ForeColor = StaticColors.ForeGround;
            label_Flask3_Slider.ForeColor = StaticColors.ForeGround;
            label_Flask4_Slider.ForeColor = StaticColors.ForeGround;
            label_Flask5_Slider.ForeColor = StaticColors.ForeGround;

            slider_Flask1.ValueChanged += (s, e) => 
            { 
                label_Flask1_Slider.Text = $"{slider_Flask1.Value}";

                _settings.Flasks["1"].Percent = slider_Flask1.Value;
            };
            slider_Flask2.ValueChanged += (s, e) =>
            {
                label_Flask2_Slider.Text = $"{slider_Flask2.Value}";

                _settings.Flasks["2"].Percent = slider_Flask2.Value;
            };
            slider_Flask3.ValueChanged += (s, e) =>
            {
                label_Flask3_Slider.Text = $"{slider_Flask3.Value}";

                _settings.Flasks["3"].Percent = slider_Flask3.Value;
            };
            slider_Flask4.ValueChanged += (s, e) =>
            {
                label_Flask4_Slider.Text = $"{slider_Flask4.Value}";

                _settings.Flasks["4"].Percent = slider_Flask4.Value;
            };
            slider_Flask5.ValueChanged += (s, e) =>
            {
                label_Flask5_Slider.Text = $"{slider_Flask5.Value}";

                _settings.Flasks["5"].Percent = slider_Flask5.Value;
            };

            checkBox_Flask1.CheckedChanged += (s, e) => _settings.Flasks["1"].Active = checkBox_Flask1.Checked;
            checkBox_Flask2.CheckedChanged += (s, e) => _settings.Flasks["2"].Active = checkBox_Flask2.Checked;
            checkBox_Flask3.CheckedChanged += (s, e) => _settings.Flasks["3"].Active = checkBox_Flask3.Checked;
            checkBox_Flask4.CheckedChanged += (s, e) => _settings.Flasks["4"].Active = checkBox_Flask4.Checked;
            checkBox_Flask5.CheckedChanged += (s, e) => _settings.Flasks["5"].Active = checkBox_Flask5.Checked;

            comboBox_Flask1.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask1.SelectedIndex = 0;
            comboBox_Flask1.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox_Flask1.SelectedItem?.ToString() == FlaskType.Utility.ToString() || comboBox_Flask1.SelectedItem?.ToString() == FlaskType.Tincture.ToString())                
                    groupBox_Flask1.Hide();
                else
                    groupBox_Flask1.Show();

                LabelPercent();

                _settings.Flasks["1"].FlaskType = comboBox_Flask1.SelectedItem?.ToString() ?? string.Empty;
            };

            comboBox_Flask2.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask2.SelectedIndex = 0;
            comboBox_Flask2.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox_Flask2.SelectedItem?.ToString() == FlaskType.Utility.ToString() || comboBox_Flask2.SelectedItem?.ToString() == FlaskType.Tincture.ToString())                
                   groupBox_Flask2.Hide();                
                else                
                    groupBox_Flask2.Show();

                LabelPercent();

                _settings.Flasks["2"].FlaskType = comboBox_Flask2.SelectedItem?.ToString() ?? string.Empty;
            };

            comboBox_Flask3.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask3.SelectedIndex = 0;
            comboBox_Flask3.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox_Flask3.SelectedItem?.ToString() == FlaskType.Utility.ToString() || comboBox_Flask3.SelectedItem?.ToString() == FlaskType.Tincture.ToString())                
                    groupBox_Flask3.Hide();                
                else                
                    groupBox_Flask3.Show();

                LabelPercent();

                _settings.Flasks["3"].FlaskType = comboBox_Flask3.SelectedItem?.ToString() ?? string.Empty;
            };

            comboBox_Flask4.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask4.SelectedIndex = 0;
            comboBox_Flask4.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox_Flask4.SelectedItem?.ToString() == FlaskType.Utility.ToString() || comboBox_Flask4.SelectedItem?.ToString() == FlaskType.Tincture.ToString())                
                    groupBox_Flask4.Hide();                
                else                
                    groupBox_Flask4.Show();

                LabelPercent();

                _settings.Flasks["4"].FlaskType = comboBox_Flask4.SelectedItem?.ToString() ?? string.Empty;
            };

            comboBox_Flask5.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask5.SelectedIndex = 0;
            comboBox_Flask5.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox_Flask5.SelectedItem?.ToString() == FlaskType.Utility.ToString() || comboBox_Flask5.SelectedItem?.ToString() == FlaskType.Tincture.ToString())
                    groupBox_Flask5.Hide();
                else
                    groupBox_Flask5.Show();

                LabelPercent();

                _settings.Flasks["5"].FlaskType = comboBox_Flask5.SelectedItem?.ToString() ?? string.Empty;
            };

            textBox_Flask1._textBox.Text = _settings.Flasks["1"].Key;
            textBox_Flask1._textBox.KeyDown += (s, e) =>
            {
                e.SuppressKeyPress = true;
            };
            textBox_Flask1._textBox.KeyUp += (s, e) =>
            {
                string key = _input.KeyMap.FirstOrDefault(x => x.Value == (int)e.KeyCode).Key;
                textBox_Flask1._textBox.Text = key;

                _settings.Flasks["1"].Key = key;
            };

            textBox_Flask2._textBox.Text = _settings.Flasks["2"].Key;
            textBox_Flask2._textBox.KeyDown += (s, e) =>
            {
                e.SuppressKeyPress = true;
            };
            textBox_Flask2._textBox.KeyUp += (s, e) =>
            {
                string key = _input.KeyMap.FirstOrDefault(x => x.Value == (int)e.KeyCode).Key;
                textBox_Flask2._textBox.Text = key;

                _settings.Flasks["2"].Key = key;
            };

            textBox_Flask3._textBox.Text = _settings.Flasks["3"].Key;
            textBox_Flask3._textBox.KeyDown += (s, e) =>
            {
                e.SuppressKeyPress = true;
            };
            textBox_Flask3._textBox.KeyUp += (s, e) =>
            {
                string key = _input.KeyMap.FirstOrDefault(x => x.Value == (int)e.KeyCode).Key;
                textBox_Flask3._textBox.Text = key;

                _settings.Flasks["3"].Key = key;
            };

            textBox_Flask4._textBox.Text = _settings.Flasks["4"].Key;
            textBox_Flask4._textBox.KeyDown += (s, e) =>
            {
                e.SuppressKeyPress = true;
            };
            textBox_Flask4._textBox.KeyUp += (s, e) =>
            {
                string key = _input.KeyMap.FirstOrDefault(x => x.Value == (int)e.KeyCode).Key;
                textBox_Flask4._textBox.Text = key;

                _settings.Flasks["4"].Key = key;
            };

            textBox_Flask5._textBox.Text = _settings.Flasks["5"].Key;
            textBox_Flask5._textBox.KeyDown += (s, e) =>
            {
                e.SuppressKeyPress = true;
            };
            textBox_Flask5._textBox.KeyUp += (s, e) =>
            {
                string key = _input.KeyMap.FirstOrDefault(x => x.Value == (int)e.KeyCode).Key;
                textBox_Flask5._textBox.Text = key;

                _settings.Flasks["5"].Key = key;
            };

            checkBox_Flask1.Checked = _settings.Flasks["1"].Active;
            checkBox_Flask2.Checked = _settings.Flasks["2"].Active;
            checkBox_Flask3.Checked = _settings.Flasks["3"].Active;
            checkBox_Flask4.Checked = _settings.Flasks["4"].Active;
            checkBox_Flask5.Checked = _settings.Flasks["5"].Active;

            comboBox_Flask1.SelectedItem = _settings.Flasks["1"].FlaskType;
            comboBox_Flask2.SelectedItem = _settings.Flasks["2"].FlaskType;                
            comboBox_Flask3.SelectedItem = _settings.Flasks["3"].FlaskType;
            comboBox_Flask4.SelectedItem = _settings.Flasks["4"].FlaskType;
            comboBox_Flask5.SelectedItem = _settings.Flasks["5"].FlaskType;

            slider_Flask1.Value = _settings.Flasks["1"].Percent;
            slider_Flask2.Value = _settings.Flasks["2"].Percent;
            slider_Flask3.Value = _settings.Flasks["3"].Percent;
            slider_Flask4.Value = _settings.Flasks["4"].Percent;
            slider_Flask5.Value = _settings.Flasks["5"].Percent;

            label_Flask1_Slider.Text = $"{slider_Flask1.Value}";
            label_Flask2_Slider.Text = $"{slider_Flask2.Value}";
            label_Flask3_Slider.Text = $"{slider_Flask3.Value}";
            label_Flask4_Slider.Text = $"{slider_Flask4.Value}";
            label_Flask5_Slider.Text = $"{slider_Flask5.Value}";

            label_Modifier.ForeColor = StaticColors.ForeGround;
            label_ModifierType.ForeColor = StaticColors.ForeGround;
            label_Priority.ForeColor = StaticColors.ForeGround;
            label_Tier.ForeColor = StaticColors.ForeGround;
            label_GambleSettings.ForeColor = StaticColors.ForeGround;
            label_GambleType.ForeColor = StaticColors.ForeGround;
            label_ItemXY.ForeColor = StaticColors.ForeGround;
            label_BaseXY.ForeColor = StaticColors.ForeGround;
            label_SecondXY.ForeColor = StaticColors.ForeGround;
            label_GamblerGetCoorinatesKey.ForeColor = StaticColors.ForeGround;
            label_GamblerStartKey.ForeColor = StaticColors.ForeGround;
            label_GamblerStopKey.ForeColor = StaticColors.ForeGround;
            label_GamblerDelay.ForeColor = StaticColors.ForeGround;

            comboBox_GambleType.Items.AddRange(Enum.GetNames<GambleType>());
            comboBox_GambleType.SelectedIndex = 0;
            comboBox_GambleType.SelectedIndexChanged += (s, e) =>
            {
                GambleType gambleType = Enum.Parse<GambleType>(comboBox_GambleType.SelectedItem?.ToString() ?? string.Empty);
                _settings.Modifiers.GambleType = gambleType;

                _settings.Modifiers.Mode = gambleType switch
                {
                    GambleType.Alt => _settings.Modifiers._uialt,
                    GambleType.Alt_Aug => _settings.Modifiers._uialt_aug,
                    GambleType.Chaos => _settings.Modifiers._uichaos,
                    GambleType.Chromatic => _settings.Modifiers._uichromatic,
                    GambleType.Eldritch => _settings.Modifiers._uieldritch,
                    GambleType.Essence => _settings.Modifiers._uiesscence,
                    GambleType.Harvest => _settings.Modifiers._uiharvest,
                    GambleType.Map => _settings.Modifiers._uimap,
                    GambleType.MapT17 => _settings.Modifiers._uimapT17,
                    _ => _settings.Modifiers._uialt
                };

                textBox_ItemXY._textBox.Text = $"{_settings.Modifiers.Mode.Item.X}, {_settings.Modifiers.Mode.Item.Y}";
                textBox_BaseXY._textBox.Text = $"{_settings.Modifiers.Mode.Base.X}, {_settings.Modifiers.Mode.Base.Y}";
                textBox_SecondXY._textBox.Text = $"{_settings.Modifiers.Mode.Second.X}, {_settings.Modifiers.Mode.Second.Y}";

                comboBox_Mod1.SelectedItem = _settings.Modifiers.Mode.modifierType1.ToString();
                comboBox_Mod2.SelectedItem = _settings.Modifiers.Mode.modifierType2.ToString();
                comboBox_Mod3.SelectedItem = _settings.Modifiers.Mode.modifierType3.ToString();
                comboBox_Mod4.SelectedItem = _settings.Modifiers.Mode.modifierType4.ToString();
                comboBox_Mod5.SelectedItem = _settings.Modifiers.Mode.modifierType5.ToString();
                comboBox_Mod6.SelectedItem = _settings.Modifiers.Mode.modifierType6.ToString();
                comboBox_Mod7.SelectedItem = _settings.Modifiers.Mode.modifierType7.ToString();
                comboBox_Mod8.SelectedItem = _settings.Modifiers.Mode.modifierType8.ToString();

                textBox_Priority1._textBox.Text = _settings.Modifiers.Mode.Priority1.ToString();
                textBox_Priority2._textBox.Text = _settings.Modifiers.Mode.Priority2.ToString();
                textBox_Priority3._textBox.Text = _settings.Modifiers.Mode.Priority3.ToString();
                textBox_Priority4._textBox.Text = _settings.Modifiers.Mode.Priority4.ToString();
                textBox_Priority5._textBox.Text = _settings.Modifiers.Mode.Priority5.ToString();
                textBox_Priority6._textBox.Text = _settings.Modifiers.Mode.Priority6.ToString();
                textBox_Priority7._textBox.Text = _settings.Modifiers.Mode.Priority7.ToString();
                textBox_Priority8._textBox.Text = _settings.Modifiers.Mode.Priority8.ToString();

                textBox_Tier1._textBox.Text = _settings.Modifiers.Mode.Tier1.ToString();
                textBox_Tier2._textBox.Text = _settings.Modifiers.Mode.Tier2.ToString();
                textBox_Tier3._textBox.Text = _settings.Modifiers.Mode.Tier3.ToString();
                textBox_Tier4._textBox.Text = _settings.Modifiers.Mode.Tier4.ToString();
                textBox_Tier5._textBox.Text = _settings.Modifiers.Mode.Tier5.ToString();
                textBox_Tier6._textBox.Text = _settings.Modifiers.Mode.Tier6.ToString();
                textBox_Tier7._textBox.Text = _settings.Modifiers.Mode.Tier7.ToString();
                textBox_Tier8._textBox.Text = _settings.Modifiers.Mode.Tier8.ToString();

                textBox_Mod1._textBox.Text = _settings.Modifiers.Mode.Content1;
                textBox_Mod2._textBox.Text = _settings.Modifiers.Mode.Content2;
                textBox_Mod3._textBox.Text = _settings.Modifiers.Mode.Content3;
                textBox_Mod4._textBox.Text = _settings.Modifiers.Mode.Content4;
                textBox_Mod5._textBox.Text = _settings.Modifiers.Mode.Content5;
                textBox_Mod6._textBox.Text = _settings.Modifiers.Mode.Content6;
                textBox_Mod7._textBox.Text = _settings.Modifiers.Mode.Content7;
                textBox_Mod8._textBox.Text = _settings.Modifiers.Mode.Content8;
            };

            comboBox_Mod1.Items.AddRange(Enum.GetNames<ModifierType>());
            comboBox_Mod1.SelectedIndex = 0;
            comboBox_Mod1.SelectedIndexChanged += (s, e) =>
            {
                ModifierType modifierType = Enum.Parse<ModifierType>(comboBox_Mod1.SelectedItem?.ToString() ?? string.Empty);
                _settings.Modifiers.Mode.modifierType1 = modifierType;
            };

            comboBox_Mod2.Items.AddRange(Enum.GetNames<ModifierType>());
            comboBox_Mod2.SelectedIndex = 0;
            comboBox_Mod2.SelectedIndexChanged += (s, e) =>
            {
                ModifierType modifierType = Enum.Parse<ModifierType>(comboBox_Mod2.SelectedItem?.ToString() ?? string.Empty);
                _settings.Modifiers.Mode.modifierType2 = modifierType;
            };

            comboBox_Mod3.Items.AddRange(Enum.GetNames<ModifierType>());
            comboBox_Mod3.SelectedIndex = 0;
            comboBox_Mod3.SelectedIndexChanged += (s, e) =>
            {
                ModifierType modifierType = Enum.Parse<ModifierType>(comboBox_Mod3.SelectedItem?.ToString() ?? string.Empty);
                _settings.Modifiers.Mode.modifierType3 = modifierType;
            };

            comboBox_Mod4.Items.AddRange(Enum.GetNames<ModifierType>());
            comboBox_Mod4.SelectedIndex = 0;
            comboBox_Mod4.SelectedIndexChanged += (s, e) =>
            {
                ModifierType modifierType = Enum.Parse<ModifierType>(comboBox_Mod4.SelectedItem?.ToString() ?? string.Empty);
                _settings.Modifiers.Mode.modifierType4 = modifierType;
            };

            comboBox_Mod5.Items.AddRange(Enum.GetNames<ModifierType>());
            comboBox_Mod5.SelectedIndex = 0;
            comboBox_Mod5.SelectedIndexChanged += (s, e) =>
            {
                ModifierType modifierType = Enum.Parse<ModifierType>(comboBox_Mod5.SelectedItem?.ToString() ?? string.Empty);
                _settings.Modifiers.Mode.modifierType5 = modifierType;
            };

            comboBox_Mod6.Items.AddRange(Enum.GetNames<ModifierType>());
            comboBox_Mod6.SelectedIndex = 0;
            comboBox_Mod6.SelectedIndexChanged += (s, e) =>
            {
                ModifierType modifierType = Enum.Parse<ModifierType>(comboBox_Mod6.SelectedItem?.ToString() ?? string.Empty);
                _settings.Modifiers.Mode.modifierType6 = modifierType;
            };

            comboBox_Mod7.Items.AddRange(Enum.GetNames<ModifierType>());
            comboBox_Mod7.SelectedIndex = 0;
            comboBox_Mod7.SelectedIndexChanged += (s, e) =>
            {
                ModifierType modifierType = Enum.Parse<ModifierType>(comboBox_Mod7.SelectedItem?.ToString() ?? string.Empty);
                _settings.Modifiers.Mode.modifierType7 = modifierType;
            };

            comboBox_Mod8.Items.AddRange(Enum.GetNames<ModifierType>());
            comboBox_Mod8.SelectedIndex = 0;
            comboBox_Mod8.SelectedIndexChanged += (s, e) =>
            {
                ModifierType modifierType = Enum.Parse<ModifierType>(comboBox_Mod8.SelectedItem?.ToString() ?? string.Empty);
                _settings.Modifiers.Mode.modifierType8 = modifierType;
            };

            comboBox_GambleType.SelectedItem = _settings.Modifiers.GambleType.ToString();
            textBox_GamblerGetCoordinatesKey._textBox.Text = _settings.Modifiers.GetCoorinatesKey;
            textBox_GamblerStartKey._textBox.Text = _settings.Modifiers.GamblerStart;
            textBox_GamblerStopKey._textBox.Text = _settings.Modifiers.GamblerStop;
            textBox_ItemXY._textBox.Text = $"{_settings.Modifiers.Mode.Item.X}, {_settings.Modifiers.Mode.Item.Y}";
            textBox_BaseXY._textBox.Text = $"{_settings.Modifiers.Mode.Base.X}, {_settings.Modifiers.Mode.Base.Y}";
            textBox_SecondXY._textBox.Text = $"{_settings.Modifiers.Mode.Second.X}, {_settings.Modifiers.Mode.Second.Y}";
            textBox_GamblerDelay._textBox.Text = _settings.Modifiers.Delay.ToString();

            comboBox_Mod1.SelectedItem = _settings.Modifiers.Mode.modifierType1.ToString();
            comboBox_Mod2.SelectedItem = _settings.Modifiers.Mode.modifierType2.ToString();
            comboBox_Mod3.SelectedItem = _settings.Modifiers.Mode.modifierType3.ToString();
            comboBox_Mod4.SelectedItem = _settings.Modifiers.Mode.modifierType4.ToString();
            comboBox_Mod5.SelectedItem = _settings.Modifiers.Mode.modifierType5.ToString();
            comboBox_Mod6.SelectedItem = _settings.Modifiers.Mode.modifierType6.ToString();
            comboBox_Mod7.SelectedItem = _settings.Modifiers.Mode.modifierType7.ToString();
            comboBox_Mod8.SelectedItem = _settings.Modifiers.Mode.modifierType8.ToString();

            textBox_Priority1._textBox.Text = _settings.Modifiers.Mode.Priority1.ToString();
            textBox_Priority2._textBox.Text = _settings.Modifiers.Mode.Priority2.ToString();
            textBox_Priority3._textBox.Text = _settings.Modifiers.Mode.Priority3.ToString();
            textBox_Priority4._textBox.Text = _settings.Modifiers.Mode.Priority4.ToString();
            textBox_Priority5._textBox.Text = _settings.Modifiers.Mode.Priority5.ToString();
            textBox_Priority6._textBox.Text = _settings.Modifiers.Mode.Priority6.ToString();
            textBox_Priority7._textBox.Text = _settings.Modifiers.Mode.Priority7.ToString();
            textBox_Priority8._textBox.Text = _settings.Modifiers.Mode.Priority8.ToString();

            textBox_Tier1._textBox.Text = _settings.Modifiers.Mode.Tier1.ToString();
            textBox_Tier2._textBox.Text = _settings.Modifiers.Mode.Tier2.ToString();
            textBox_Tier3._textBox.Text = _settings.Modifiers.Mode.Tier3.ToString();
            textBox_Tier4._textBox.Text = _settings.Modifiers.Mode.Tier4.ToString();
            textBox_Tier5._textBox.Text = _settings.Modifiers.Mode.Tier5.ToString();
            textBox_Tier6._textBox.Text = _settings.Modifiers.Mode.Tier6.ToString();
            textBox_Tier7._textBox.Text = _settings.Modifiers.Mode.Tier7.ToString();
            textBox_Tier8._textBox.Text = _settings.Modifiers.Mode.Tier8.ToString();

            textBox_Mod1._textBox.Text = _settings.Modifiers.Mode.Content1;
            textBox_Mod2._textBox.Text = _settings.Modifiers.Mode.Content2;
            textBox_Mod3._textBox.Text = _settings.Modifiers.Mode.Content3;
            textBox_Mod4._textBox.Text = _settings.Modifiers.Mode.Content4;
            textBox_Mod5._textBox.Text = _settings.Modifiers.Mode.Content5;
            textBox_Mod6._textBox.Text = _settings.Modifiers.Mode.Content6;
            textBox_Mod7._textBox.Text = _settings.Modifiers.Mode.Content7;
            textBox_Mod8._textBox.Text = _settings.Modifiers.Mode.Content8;

            textBox_ItemXY._textBox.KeyUp += (s, e) =>
            {
                if (textBox_ItemXY._textBox.Text.Contains(','))
                {
                    string[] coords = textBox_ItemXY._textBox.Text.Split(',');
                    if (coords.Length == 2 && int.TryParse(coords[0], out int x) && int.TryParse(coords[1], out int y))
                    {
                        Coordinates coordinates = new Coordinates(x, y);
                        _settings.Modifiers.Mode.Item = coordinates;

                        textBox_ItemXY._textBox.ForeColor = StaticColors.ForeGround;
                    }
                    else
                    {
                        textBox_ItemXY._textBox.ForeColor = Color.Red;
                    }
                }
                else
                {
                    textBox_ItemXY._textBox.ForeColor = Color.Red;
                }
            };
            textBox_BaseXY._textBox.KeyUp += (s, e) =>
            {
                if (textBox_BaseXY._textBox.Text.Contains(','))
                {
                    string[] coords = textBox_BaseXY._textBox.Text.Split(',');
                    if (coords.Length == 2 && int.TryParse(coords[0], out int x) && int.TryParse(coords[1], out int y))
                    {
                        Coordinates coordinates = new Coordinates(x, y);
                        _settings.Modifiers.Mode.Base = coordinates;

                        textBox_BaseXY._textBox.ForeColor = StaticColors.ForeGround;
                    }
                    else
                    {
                        textBox_BaseXY._textBox.ForeColor = Color.Red;
                    }
                }
                else
                {
                    textBox_BaseXY._textBox.ForeColor = Color.Red;
                }
            };
            textBox_SecondXY._textBox.KeyUp += (s, e) =>
            {
                if (textBox_SecondXY._textBox.Text.Contains(','))
                {
                    string[] coords = textBox_SecondXY._textBox.Text.Split(',');
                    if (coords.Length == 2 && int.TryParse(coords[0], out int x) && int.TryParse(coords[1], out int y))
                    {
                        Coordinates coordinates = new Coordinates(x, y);
                        _settings.Modifiers.Mode.Second = coordinates;

                        textBox_SecondXY._textBox.ForeColor = StaticColors.ForeGround;
                    }
                    else
                    {
                        textBox_SecondXY._textBox.ForeColor = Color.Red;
                    }
                }
                else
                {
                    textBox_SecondXY._textBox.ForeColor = Color.Red;
                }
            };

            textBox_GamblerGetCoordinatesKey._textBox.KeyDown += (s, e) =>
            {
                e.SuppressKeyPress = true;
            };
            textBox_GamblerGetCoordinatesKey._textBox.KeyUp += (s, e) =>
            {
                var names = Enum.GetNames<KeyCode>();
                string key = names.FirstOrDefault(x => x.Equals(e.KeyCode.ToString(), StringComparison.OrdinalIgnoreCase)) ?? e.KeyCode.ToString();

                if (string.IsNullOrEmpty(key) || key == "None")
                {
                    textBox_GamblerGetCoordinatesKey._textBox.ForeColor = Color.Red;
                    return;
                }

                gamblerGetCoordinates = new Bind(gamblerStart.Id, gamblerStart.Action, Enum.Parse<KeyCode>(key));
                _hotkeys.Change(gamblerGetCoordinates);

                textBox_GamblerGetCoordinatesKey._textBox.Text = key;
                _settings.Modifiers.GetCoorinatesKey = key;
            };
            textBox_GamblerStartKey._textBox.KeyDown += (s, e) =>
            {
                e.SuppressKeyPress = true;
            };
            textBox_GamblerStartKey._textBox.KeyUp += (s, e) =>
            {
                var names = Enum.GetNames<KeyCode>();
                string key = names.FirstOrDefault(x => x.Equals(e.KeyCode.ToString(), StringComparison.OrdinalIgnoreCase)) ?? e.KeyCode.ToString();
                if (string.IsNullOrEmpty(key) || key == "None")
                {
                    textBox_GamblerStartKey._textBox.ForeColor = Color.Red;
                    return;
                }

                gamblerStart = new Bind(gamblerStart.Id, gamblerStart.Action, Enum.Parse<KeyCode>(key));
                _hotkeys.Change(gamblerStart);

                textBox_GamblerStartKey._textBox.Text = key;
                _settings.Modifiers.GamblerStart = key;
            };
            textBox_GamblerStopKey._textBox.KeyDown += (s, e) =>
            {
                e.SuppressKeyPress = true;
            };
            textBox_GamblerStopKey._textBox.KeyUp += (s, e) =>
            {
                var names = Enum.GetNames<KeyCode>();
                string key = names.FirstOrDefault(x => x.Equals(e.KeyCode.ToString(), StringComparison.OrdinalIgnoreCase)) ?? e.KeyCode.ToString();
                
                if (string.IsNullOrEmpty(key) || key == "None")
                {
                    textBox_GamblerStopKey._textBox.ForeColor = Color.Red;
                    return;
                }

                gamblerStop = new Bind(gamblerStop.Id, gamblerStop.Action, Enum.Parse<KeyCode>(key));
                _hotkeys.Change(gamblerStop);

                textBox_GamblerStopKey._textBox.Text = key;
                _settings.Modifiers.GamblerStop = key;
            };
            textBox_GamblerDelay._textBox.KeyUp += (s, e) =>
            {
                if (Delay_NumberOnly(textBox_GamblerDelay._textBox))
                    _settings.Modifiers.Delay = int.Parse(textBox_GamblerDelay._textBox.Text);
            };

            textBox_Priority1._textBox.KeyUp += (s, e) =>
            {
                if (Priority_NumberOnly(textBox_Priority1._textBox))
                    _settings.Modifiers.Mode.Priority1 = decimal.Parse(textBox_Priority1._textBox.Text);
            };
            textBox_Priority2._textBox.KeyUp += (s, e) =>
            {
                if (Priority_NumberOnly(textBox_Priority2._textBox))
                    _settings.Modifiers.Mode.Priority2 = decimal.Parse(textBox_Priority2._textBox.Text);
            };
            textBox_Priority3._textBox.KeyUp += (s, e) =>
            {
                if (Priority_NumberOnly(textBox_Priority3._textBox))
                    _settings.Modifiers.Mode.Priority3 = decimal.Parse(textBox_Priority3._textBox.Text);
            };
            textBox_Priority4._textBox.KeyUp += (s, e) =>
            {
                if (Priority_NumberOnly(textBox_Priority4._textBox))
                    _settings.Modifiers.Mode.Priority4 = decimal.Parse(textBox_Priority4._textBox.Text);
            };
            textBox_Priority5._textBox.KeyUp += (s, e) =>
            {
                if (Priority_NumberOnly(textBox_Priority5._textBox))
                    _settings.Modifiers.Mode.Priority5 = decimal.Parse(textBox_Priority5._textBox.Text);
            };
            textBox_Priority6._textBox.KeyUp += (s, e) =>
            {
                if (Priority_NumberOnly(textBox_Priority6._textBox))
                    _settings.Modifiers.Mode.Priority6 = decimal.Parse(textBox_Priority6._textBox.Text);
            };
            textBox_Priority7._textBox.KeyUp += (s, e) =>
            {
                if (Priority_NumberOnly(textBox_Priority7._textBox))
                    _settings.Modifiers.Mode.Priority7 = decimal.Parse(textBox_Priority7._textBox.Text);
            };
            textBox_Priority8._textBox.KeyUp += (s, e) =>
            {
                if (Priority_NumberOnly(textBox_Priority8._textBox))
                    _settings.Modifiers.Mode.Priority8 = decimal.Parse(textBox_Priority8._textBox.Text);
            };

            textBox_Tier1._textBox.KeyUp += (s, e) =>
            {
               if (Tier_NumberOnly(textBox_Tier1._textBox))
                    _settings.Modifiers.Mode.Tier1 = int.Parse(textBox_Tier1._textBox.Text);
            };
            textBox_Tier2._textBox.KeyUp += (s, e) =>
            {
                if (Tier_NumberOnly(textBox_Tier2._textBox))
                    _settings.Modifiers.Mode.Tier2 = int.Parse(textBox_Tier2._textBox.Text);
            };
            textBox_Tier3._textBox.KeyUp += (s, e) =>
            {
                if (Tier_NumberOnly(textBox_Tier3._textBox))
                    _settings.Modifiers.Mode.Tier3 = int.Parse(textBox_Tier3._textBox.Text);
            };
            textBox_Tier4._textBox.KeyUp += (s, e) =>
            {
                if (Tier_NumberOnly(textBox_Tier4._textBox))
                    _settings.Modifiers.Mode.Tier4 = int.Parse(textBox_Tier4._textBox.Text);
            };
            textBox_Tier5._textBox.KeyUp += (s, e) =>
            {
                if (Tier_NumberOnly(textBox_Tier5._textBox))
                    _settings.Modifiers.Mode.Tier5 = int.Parse(textBox_Tier5._textBox.Text);
            };
            textBox_Tier6._textBox.KeyUp += (s, e) =>
            {
                if (Tier_NumberOnly(textBox_Tier6._textBox))
                    _settings.Modifiers.Mode.Tier6 = int.Parse(textBox_Tier6._textBox.Text);
            };
            textBox_Tier7._textBox.KeyUp += (s, e) =>
            {
                if (Tier_NumberOnly(textBox_Tier7._textBox))
                    _settings.Modifiers.Mode.Tier7 = int.Parse(textBox_Tier7._textBox.Text);
            };
            textBox_Tier8._textBox.KeyUp += (s, e) =>
            {
                if (Tier_NumberOnly(textBox_Tier8._textBox))
                    _settings.Modifiers.Mode.Tier8 = int.Parse(textBox_Tier8._textBox.Text);
            };

            textBox_Mod1._textBox.KeyUp += (s, e) =>
            {
                _settings.Modifiers.Mode.Content1 = textBox_Mod1._textBox.Text;
            };
            textBox_Mod2._textBox.KeyUp += (s, e) =>
            {
                _settings.Modifiers.Mode.Content2 = textBox_Mod2._textBox.Text;
            };
            textBox_Mod3._textBox.KeyUp += (s, e) =>
            {
                _settings.Modifiers.Mode.Content3 = textBox_Mod3._textBox.Text;
            };
            textBox_Mod4._textBox.KeyUp += (s, e) =>
            {
                _settings.Modifiers.Mode.Content4 = textBox_Mod4._textBox.Text;
            };
            textBox_Mod5._textBox.KeyUp += (s, e) =>
            {
                _settings.Modifiers.Mode.Content5 = textBox_Mod5._textBox.Text;
            };
            textBox_Mod6._textBox.KeyUp += (s, e) =>
            {
                _settings.Modifiers.Mode.Content6 = textBox_Mod6._textBox.Text;
            };
            textBox_Mod7._textBox.KeyUp += (s, e) =>
            {
                _settings.Modifiers.Mode.Content7 = textBox_Mod7._textBox.Text;
            };
            textBox_Mod8._textBox.KeyUp += (s, e) =>
            {
                _settings.Modifiers.Mode.Content8 = textBox_Mod8._textBox.Text;
            };

            button_Record1.Click += (s, e) =>
            {
                if (_getCoordinatesBase)
                    button_Record2.PerformClick();
                if (_getCoordinatesSecond)
                    button_Record3.PerformClick();

                if (_getCoordinatesItem)
                {
                    _getCoordinatesItem = false;

                    button_Record1.ForeColor = Color.Black;
                    button_Record1.Text = "Rec";
                }
                else
                {
                    _getCoordinatesItem = true;

                    button_Record1.ForeColor = Color.Red;
                    button_Record1.Text = "...";
                }
            };
            button_Record2.Click += (s, e) =>
            {
                if (_getCoordinatesItem)
                    button_Record1.PerformClick();
                if (_getCoordinatesSecond)
                    button_Record3.PerformClick();

                if (_getCoordinatesBase)
                {
                    _getCoordinatesBase = false;

                    button_Record2.ForeColor = Color.Black;
                    button_Record2.Text = "Rec";
                }
                else
                {
                    _getCoordinatesBase = true;

                    button_Record2.ForeColor = Color.Red;
                    button_Record2.Text = "...";
                }
            };
            button_Record3.Click += (s, e) =>
            {
                if (_getCoordinatesItem)
                    button_Record1.PerformClick();
                if (_getCoordinatesBase)
                    button_Record2.PerformClick();

                if (_getCoordinatesSecond)
                {
                    _getCoordinatesSecond = false;

                    button_Record3.ForeColor = Color.Black;
                    button_Record3.Text = "Rec";
                }
                else
                {
                    _getCoordinatesSecond = true;

                    button_Record3.ForeColor = Color.Red;
                    button_Record3.Text = "...";
                }
            };

            Init();
        }

        private bool Priority_NumberOnly(TextBox textBox)
        {
            if (decimal.TryParse(textBox.Text, out decimal value))
            {
                textBox.ForeColor = StaticColors.ForeGround;
                return true;
            }
            else
            {
                textBox.ForeColor = Color.Red;
                return false;
            }
        }
        private bool Tier_NumberOnly(TextBox textBox)
        {
            if (int.TryParse(textBox.Text, out int value) && value > 0 && value < 10)
            {
                textBox.ForeColor = StaticColors.ForeGround;
                return true;
            }
            else
            {
                textBox.ForeColor = Color.Red;
                return false;
            }
        }
        private bool Delay_NumberOnly(TextBox textBox)
        {
            if (int.TryParse(textBox.Text, out int value))
            {
                textBox.ForeColor = StaticColors.ForeGround;
                return true;
            }
            else
            {
                textBox.ForeColor = Color.Red;
                return false;
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            _hotkeys.Stop();

            _userSettings.SaveSettings();
        }

        private void LabelPercent()
        {
            if (!groupBox_Flask1.Visible && !groupBox_Flask2.Visible && !groupBox_Flask3.Visible && !groupBox_Flask4.Visible && !groupBox_Flask5.Visible)
                label_Percent.Visible = false;
            else
                label_Percent.Visible = true;
        }
    }
}
