using PoE.dlls.Automation;
using PoE.dlls.Settings;
using PoE.dlls.Style;

namespace PoE
{
    public partial class Main
    {
        private FlatGroupBox groupBox_Input = null!;
        private Label label_ProcessName = null!;
        private FlatTextBox textBox_ProcessName = null!;

        private void InitializeInputSettingsUi()
        {
            groupBox_Input = new FlatGroupBox
            {
                Text = "Input",
                Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point),
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
            };

            label_ProcessName = new Label
            {
                AutoSize = true,
                Text = "Process name:",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
            };

            textBox_ProcessName = new FlatTextBox
            {
                Size = new Size(280, 30),
            };
            textBox_ProcessName._textBox.Text = InputSimulatorHost.ResolveProcessName(_settings.InputProcessName);
            textBox_ProcessName._textBox.Leave += (_, _) => ApplyInputProcessNameSetting();

            groupBox_Input.Controls.Add(label_ProcessName);
            groupBox_Input.Controls.Add(textBox_ProcessName);

            tabPage_Settings.Controls.Add(groupBox_Input);
            ApplyInputProcessNameFromSettings();
        }

        private void LayoutInputSettingsGroup()
        {
            if (groupBox_Input is null || textBox_ProcessName is null || label_ProcessName is null)
                return;

            const int innerPad = 12;

            label_ProcessName.Location = new Point(innerPad, innerPad + 8);
            textBox_ProcessName.Location = new Point(innerPad + 110, innerPad + 4);
            textBox_ProcessName.Width = Math.Max(160, groupBox_Input.Width - textBox_ProcessName.Left - innerPad);
        }

        private void ApplyInputProcessNameFromSettings()
        {
            _inputHost.Configure(_settings.InputProcessName);
            textBox_ProcessName._textBox.Text = _inputHost.EffectiveProcessName;
        }

        private void ApplyInputProcessNameSetting()
        {
            string entered = textBox_ProcessName._textBox.Text.Trim();
            if (string.IsNullOrEmpty(entered))
            {
                _settings.InputProcessName = string.Empty;
                textBox_ProcessName._textBox.Text = InputSimulatorHost.DefaultProcessName;
            }
            else
            {
                _settings.InputProcessName = InputSimulatorHost.NormalizeForStorage(entered);
                textBox_ProcessName._textBox.Text = InputSimulatorHost.ResolveProcessName(_settings.InputProcessName);
            }

            _inputHost.Configure(_settings.InputProcessName);
            _userSettings.SaveSettings();
        }
    }
}
