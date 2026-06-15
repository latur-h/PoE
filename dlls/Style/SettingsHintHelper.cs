namespace PoE.dlls.Style
{
    internal static class SettingsHintHelper
    {
        public static void Attach(ToolTip toolTip, Control container, Label label, FlatTextBox textBox, string hint) =>
            AttachField(toolTip, container, label, textBox, hint, null);

        public static Label Attach(
            ToolTip toolTip,
            Control container,
            Label label,
            FlatComboBox comboBox,
            string hint,
            Action? onClick = null) =>
            AttachField(toolTip, container, label, comboBox, hint, onClick);

        private static Label AttachField(
            ToolTip toolTip,
            Control container,
            Label label,
            Control field,
            string hint,
            Action? onClick)
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
            if (onClick is not null)
                icon.Click += (_, _) => onClick();

            container.Controls.Add(icon);

            var iconSize = TextRenderer.MeasureText(icon.Text, icon.Font);
            icon.Location = new Point(
                field.Location.X + field.Width - iconSize.Width,
                label.Location.Y + (label.Height - iconSize.Height) / 2);

            icon.BringToFront();
            return icon;
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
