using PoE.dlls.InteropServices;
using PoE.dlls.Settings.Mods;using PoE.dlls.Style;

namespace PoE
{
    public partial class Main
    {
        private sealed class CoordinateSlot
        {
            public required string Key { get; init; }
            public required Label Label { get; init; }
            public required FlatTextBox TextBox { get; init; }
            public required Button Record { get; init; }
            public required Action<Coordinates> Setter { get; init; }
            public required Func<Coordinates> Getter { get; init; }
        }

        private readonly List<CoordinateSlot> _coordinateSlots = [];
        private CoordinateSlot? _activeCoordinateSlot;
        private GroupBox groupBox_OrbsItems = null!;
        private GroupBox groupBox_OrbsOrbs = null!;
        private FlowLayoutPanel flowLayout_OrbsItems = null!;
        private FlowLayoutPanel flowLayout_OrbsOrbs = null!;

        private void InitializeOrbsTab()
        {
            tabPage_Orbs.BackColor = StaticColors.BackGround;

            groupBox_OrbsItems = new GroupBox
            {
                Text = "Items",
                Font = new Font("Segoe UI", 12F),
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Location = new Point(7, 7),
                Size = new Size(600, 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };

            groupBox_OrbsOrbs = new GroupBox
            {
                Text = "Orbs",
                Font = new Font("Segoe UI", 12F),
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Location = new Point(7, 134),
                Size = new Size(600, 250),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            };

            flowLayout_OrbsItems = CreateCoordinateFlowPanel();
            flowLayout_OrbsOrbs = CreateCoordinateFlowPanel();

            groupBox_OrbsItems.Controls.Add(flowLayout_OrbsItems);
            groupBox_OrbsOrbs.Controls.Add(flowLayout_OrbsOrbs);
            tabPage_Orbs.Controls.Add(groupBox_OrbsItems);
            tabPage_Orbs.Controls.Add(groupBox_OrbsOrbs);

            AddItemSlot("default-item", "Default item", () => _settings.Modifiers.Items.Default, v => _settings.Modifiers.Items.Default = v);
            AddItemSlot("harvest-item", "Harvest item", () => _settings.Modifiers.Items.Harvest, v => _settings.Modifiers.Items.Harvest = v);
            AddItemSlot("essence-item", "Essence item", () => _settings.Modifiers.Items.Essence, v => _settings.Modifiers.Items.Essence = v);

            foreach (var orb in GambleCoordinateResolver.AllOrbTypes())
            {
                var orbType = orb;
                AddOrbSlot(orbType, () => _settings.Modifiers.Orbs.Get(orbType), v => _settings.Modifiers.Orbs.Set(orbType, v));
            }

            LoadOrbsTabIntoUi();
        }

        private static FlowLayoutPanel CreateCoordinateFlowPanel() => new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            Padding = new Padding(4),
        };

        private void AddItemSlot(string key, string label, Func<Coordinates> getter, Action<Coordinates> setter) =>
            AddCoordinateSlot(key, label, getter, setter, flowLayout_OrbsItems);

        private void AddOrbSlot(GambleOrbType orb, Func<Coordinates> getter, Action<Coordinates> setter) =>
            AddCoordinateSlot($"orb-{orb}", GambleCoordinateResolver.OrbLabel(orb), getter, setter, flowLayout_OrbsOrbs);

        private void AddCoordinateSlot(string key, string labelText, Func<Coordinates> getter, Action<Coordinates> setter, FlowLayoutPanel parent)
        {
            var label = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F),
                ForeColor = StaticColors.ForeGround,
                Text = labelText,
                Width = 110,
            };

            var textBox = new FlatTextBox
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12F),
                Size = new Size(110, 30),
                TextAlign = HorizontalAlignment.Center,
            };

            var record = new Button
            {
                Size = new Size(48, 23),
                Text = "Rec",
                UseVisualStyleBackColor = true,
            };

            var slot = new CoordinateSlot
            {
                Key = key,
                Label = label,
                TextBox = textBox,
                Record = record,
                Getter = getter,
                Setter = setter,
            };

            textBox._textBox.KeyUp += (_, _) =>
            {
                if (TryParseCoordinateText(textBox._textBox.Text, out var coordinates))
                {
                    setter(coordinates);
                    textBox._textBox.ForeColor = StaticColors.ForeGround;
                }
                else
                {
                    textBox._textBox.ForeColor = Color.Red;
                }
            };

            record.Click += (_, _) => ToggleCoordinateRecording(slot);

            var panel = new Panel { Size = new Size(280, 62), Margin = new Padding(4) };
            label.Location = new Point(0, 0);
            record.Location = new Point(0, 28);
            textBox.Location = new Point(54, 26);
            panel.Controls.Add(label);
            panel.Controls.Add(record);
            panel.Controls.Add(textBox);
            parent.Controls.Add(panel);

            _coordinateSlots.Add(slot);
        }

        private void ToggleCoordinateRecording(CoordinateSlot slot)
        {
            if (_activeCoordinateSlot == slot)
            {
                StopCoordinateRecording(slot);
                _activeCoordinateSlot = null;
                return;
            }

            if (_activeCoordinateSlot is not null)
                StopCoordinateRecording(_activeCoordinateSlot);

            _activeCoordinateSlot = slot;
            slot.Record.ForeColor = Color.Red;
            slot.Record.Text = "...";
        }
        private void StopCoordinateRecording(CoordinateSlot slot)
        {
            slot.Record.ForeColor = Color.Black;
            slot.Record.Text = "Rec";
        }

        private void ClearCoordinateRecording()
        {
            if (_activeCoordinateSlot is not null)
            {
                StopCoordinateRecording(_activeCoordinateSlot);
                _activeCoordinateSlot = null;
            }
        }

        private void LoadOrbsTabIntoUi()
        {
            foreach (var slot in _coordinateSlots)
            {
                var c = slot.Getter();
                slot.TextBox._textBox.Text = $"{c.X}, {c.Y}";
                slot.TextBox._textBox.ForeColor = StaticColors.ForeGround;
            }
        }

        private bool TryApplyRecordedCoordinate()
        {
            if (_activeCoordinateSlot is null)
                return false;

            var coordinates = InteropHelper.GetMousePos();
            _activeCoordinateSlot.Setter(coordinates);
            _activeCoordinateSlot.TextBox._textBox.Text = $"{coordinates.X}, {coordinates.Y}";
            _activeCoordinateSlot.TextBox._textBox.ForeColor = StaticColors.ForeGround;
            StopCoordinateRecording(_activeCoordinateSlot);
            _activeCoordinateSlot = null;
            return true;
        }

        private void LayoutOrbsTab()
        {
            const int margin = 7;
            int width = tabPage_Orbs.ClientSize.Width;
            int height = tabPage_Orbs.ClientSize.Height;
            if (width <= 0 || height <= 0)
                return;

            int innerWidth = width - margin * 2;
            groupBox_OrbsItems.Location = new Point(margin, margin);
            groupBox_OrbsItems.Size = new Size(innerWidth, 118);

            groupBox_OrbsOrbs.Location = new Point(margin, groupBox_OrbsItems.Bottom + margin);
            groupBox_OrbsOrbs.Size = new Size(innerWidth, Math.Max(160, height - groupBox_OrbsOrbs.Top - margin));
        }
    }
}
