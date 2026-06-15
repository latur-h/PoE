using PoE.dlls.Settings.Mods;
using PoE.dlls.Style;

namespace PoE.dlls.Gamble.UI
{
    internal sealed class GamblePresetNameDialog : Form
    {
        private static readonly Font UiFont = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);

        private readonly FlatTextBox _nameBox;
        private readonly Label _errorLabel;
        private readonly IReadOnlyCollection<GamblePreset> _presets;

        public GamblePresetNameDialog(IReadOnlyCollection<GamblePreset> presets, string suggestedName)
        {
            _presets = presets;

            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(340, 140);
            BackColor = StaticColors.BackGround;
            ForeColor = StaticColors.ForeGround;
            Text = "New preset";

            var label = new Label
            {
                AutoSize = true,
                Location = new Point(12, 14),
                Text = "Preset name",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = UiFont,
            };

            _nameBox = new FlatTextBox
            {
                Location = new Point(12, 40),
                Size = new Size(316, 30),
                Font = UiFont,
            };
            _nameBox._textBox.Text = suggestedName;

            _errorLabel = new Label
            {
                AutoSize = true,
                Location = new Point(12, 76),
                ForeColor = Color.Red,
                BackColor = StaticColors.BackGround,
                Font = UiFont,
                Visible = false,
            };

            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.None,
                Location = new Point(172, 96),
                Size = new Size(75, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
            };
            okButton.FlatAppearance.BorderColor = StaticColors.ForeGround;
            okButton.Click += (_, _) => TryAccept();

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(253, 96),
                Size = new Size(75, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
            };
            cancelButton.FlatAppearance.BorderColor = StaticColors.ForeGround;

            AcceptButton = okButton;
            CancelButton = cancelButton;

            Controls.Add(label);
            Controls.Add(_nameBox);
            Controls.Add(_errorLabel);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
        }

        public string? PresetName { get; private set; }

        private void TryAccept()
        {
            string name = _nameBox._textBox.Text.Trim();
            if (!GamblePresetHelper.IsNameAvailable(name, _presets))
            {
                _errorLabel.Text = string.IsNullOrWhiteSpace(name)
                    ? "Name is required."
                    : "That name is already in use.";
                _errorLabel.Visible = true;
                return;
            }

            PresetName = name;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
