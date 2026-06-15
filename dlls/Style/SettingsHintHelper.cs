namespace PoE.dlls.Style
{
    internal static class SettingsHintHelper
    {
        public static void Attach(ToolTip toolTip, Control container, Label label, FlatTextBox textBox, string hint)
        {
            toolTip.SetToolTip(label, hint);

            var icon = new Label
            {
                AutoSize = true,
                Text = "ⓘ",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = StaticColors.ForeGround,
                BackColor = Color.Transparent,
                Cursor = Cursors.Help,
                TabStop = false,
            };

            toolTip.SetToolTip(icon, hint);
            container.Controls.Add(icon);

            var iconSize = TextRenderer.MeasureText(icon.Text, icon.Font);
            icon.Location = new Point(
                textBox.Location.X + textBox.Width - iconSize.Width,
                label.Location.Y + (label.Height - iconSize.Height) / 2);

            icon.BringToFront();
        }

        public static void Configure(ToolTip toolTip)
        {
            toolTip.AutoPopDelay = 12000;
            toolTip.InitialDelay = 400;
            toolTip.ReshowDelay = 200;
            toolTip.ShowAlways = true;
            toolTip.BackColor = StaticColors.BackGround;
            toolTip.ForeColor = StaticColors.ForeGround;
        }
    }
}
