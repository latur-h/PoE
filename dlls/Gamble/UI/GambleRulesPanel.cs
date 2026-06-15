using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.Settings.Mods;
using PoE.dlls.Style;

namespace PoE.dlls.Gamble.UI
{
    public sealed class GambleRulesPanel : UserControl
    {
        private const int RowHeight = 36;
        private const int HeaderHeight = 28;
        private const int AddBarHeight = 36;

        private static readonly Font UiFont = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly string[] ModifierTypeNames = Enum.GetNames<ModifierType>();

        private readonly Panel _scrollPanel;
        private readonly Panel _rowsHost;
        private readonly Button _addButton;
        private readonly Dictionary<GamblePreset, PresetView> _views = [];

        private GamblePreset? _visiblePreset;
        private PresetView? _activeView;
        private bool _suppressEvents;

        public GambleRulesPanel()
        {
            BackColor = StaticColors.BackGround;
            Font = UiFont;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = HeaderHeight,
                BackColor = StaticColors.BackGround,
            };

            header.Controls.Add(CreateHeaderLabel("Priority", 0, 59));
            header.Controls.Add(CreateHeaderLabel("Type", 66, 121));
            header.Controls.Add(CreateHeaderLabel("Tier", 200, 59));
            header.Controls.Add(CreateHeaderLabel("Modifier content", 272, 287));

            _addButton = new Button
            {
                Dock = DockStyle.Bottom,
                Height = AddBarHeight,
                Text = "+ Add rule",
                FlatStyle = FlatStyle.Flat,
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
                Cursor = Cursors.Hand,
                TabStop = false,
            };
            _addButton.FlatAppearance.BorderColor = StaticColors.ForeGround;
            _addButton.Click += (_, _) => AddRow(new GambleRuleRow());

            _rowsHost = new Panel
            {
                AutoSize = false,
                Location = new Point(0, 0),
                Width = 0,
                Height = 0,
                BackColor = StaticColors.BackGround,
            };

            _scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = StaticColors.BackGround,
            };
            _scrollPanel.Controls.Add(_rowsHost);
            _scrollPanel.Resize += (_, _) => LayoutActiveView();
            _scrollPanel.HandleCreated += (_, _) => DisableHorizontalScroll();

            Controls.Add(_scrollPanel);
            Controls.Add(_addButton);
            Controls.Add(header);
        }

        public void Bind(GamblePreset preset)
        {
            if (_visiblePreset is not null && !ReferenceEquals(_visiblePreset, preset))
                Commit();

            ShowPreset(preset);
        }

        public void PurgeViewsExcept(IEnumerable<GamblePreset> keepPresets)
        {
            var keep = keepPresets.ToHashSet();
            foreach (var preset in _views.Keys.Where(p => !keep.Contains(p)).ToList())
                DropPresetView(preset);
        }

        public void DropPresetView(GamblePreset preset)
        {
            if (!_views.Remove(preset, out PresetView? view))
                return;

            if (ReferenceEquals(_visiblePreset, preset))
            {
                _visiblePreset = null;
                _activeView = null;
            }

            _rowsHost.Controls.Remove(view.Host);
            foreach (var row in view.Rows)
                row.Dispose();
            view.Host.Dispose();
        }

        public void Commit()
        {
            if (_visiblePreset is null || _activeView is null)
                return;

            _visiblePreset.Rules = _activeView.Rows.Select(r => r.ToRule()).ToList();
        }

        private void ShowPreset(GamblePreset preset)
        {
            foreach (var view in _views.Values)
                view.Host.Visible = false;

            _activeView = GetOrCreateView(preset);
            _visiblePreset = preset;
            _activeView.Host.Visible = true;
            _activeView.Host.BringToFront();
            LayoutActiveView();
        }

        private PresetView GetOrCreateView(GamblePreset preset)
        {
            if (_views.TryGetValue(preset, out PresetView? existing))
                return existing;

            var host = new Panel
            {
                Location = new Point(0, 0),
                Width = Math.Max(0, _scrollPanel.ClientSize.Width),
                Visible = false,
                BackColor = StaticColors.BackGround,
            };

            var rows = new List<GambleRuleRowControl>();
            _suppressEvents = true;
            try
            {
                for (int i = 0; i < preset.Rules.Count; i++)
                    CreateRowControl(host, preset.Rules[i], rows);
            }
            finally
            {
                _suppressEvents = false;
            }

            _rowsHost.Controls.Add(host);

            var view = new PresetView(host, rows);
            _views[preset] = view;
            LayoutView(view);
            return view;
        }

        private void AddRow(GambleRuleRow rule)
        {
            if (_visiblePreset is null || _activeView is null || _activeView.Rows.Count >= GambleModeLayout.MaxRules)
                return;

            CreateRowControl(_activeView.Host, rule, _activeView.Rows);
            LayoutActiveView();

            if (!_suppressEvents)
                _visiblePreset.Rules = _activeView.Rows.Select(r => r.ToRule()).ToList();

            ScrollToBottom();
        }

        private GambleRuleRowControl CreateRowControl(Panel host, GambleRuleRow rule, List<GambleRuleRowControl> rows)
        {
            var row = new GambleRuleRowControl(rule);
            row.RemoveRequested += (_, _) => RemoveRow(row);
            row.Changed += (_, _) =>
            {
                if (!_suppressEvents && _visiblePreset is not null && _activeView is not null)
                    _visiblePreset.Rules = _activeView.Rows.Select(r => r.ToRule()).ToList();
            };

            rows.Add(row);
            host.Controls.Add(row);
            return row;
        }

        private void RemoveRow(GambleRuleRowControl row)
        {
            if (_activeView is null)
                return;

            int index = _activeView.Rows.IndexOf(row);
            if (index < 0)
                return;

            _activeView.Rows.RemoveAt(index);
            _activeView.Host.Controls.Remove(row);
            row.Dispose();
            LayoutActiveView();

            if (_visiblePreset is not null && !_suppressEvents)
                _visiblePreset.Rules = _activeView.Rows.Select(r => r.ToRule()).ToList();
        }

        private void LayoutActiveView()
        {
            if (_activeView is not null)
                LayoutView(_activeView);
        }

        private void LayoutView(PresetView view)
        {
            int rowWidth = Math.Max(0, _scrollPanel.ClientSize.Width);
            view.Host.Width = rowWidth;

            for (int i = 0; i < view.Rows.Count; i++)
            {
                view.Rows[i].Location = new Point(0, i * RowHeight);
                view.Rows[i].SetWidth(rowWidth);
            }

            int contentHeight = Math.Max(RowHeight, view.Rows.Count * RowHeight);
            view.Host.Height = contentHeight;
            _rowsHost.Height = contentHeight;
            _rowsHost.Width = rowWidth;
        }

        private void DisableHorizontalScroll()
        {
            _scrollPanel.HorizontalScroll.Enabled = false;
            _scrollPanel.HorizontalScroll.Visible = false;
        }

        private void ScrollToBottom()
        {
            _scrollPanel.AutoScrollPosition = new Point(0, _rowsHost.Height);
        }

        private static Label CreateHeaderLabel(string text, int x, int width) =>
            new()
            {
                AutoSize = false,
                Location = new Point(x, 4),
                Size = new Size(width, 21),
                Text = text,
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = UiFont,
            };

        private sealed class PresetView(Panel host, List<GambleRuleRowControl> rows)
        {
            public Panel Host { get; } = host;
            public List<GambleRuleRowControl> Rows { get; } = rows;
        }

        private sealed class GambleRuleRowControl : Panel
        {
            private GambleRuleRow _rule;
            private readonly FlatTextBox _priority;
            private readonly FlatComboBox _type;
            private readonly FlatTextBox _tier;
            private readonly FlatTextBox _content;
            private readonly Label _remove;

            public event EventHandler? RemoveRequested;
            public event EventHandler? Changed;

            public GambleRuleRowControl(GambleRuleRow rule)
            {
                _rule = rule;
                BackColor = StaticColors.BackGround;
                Height = 34;

                _priority = CreateTextBox(new Point(0, 2), new Size(59, 30), HorizontalAlignment.Center);
                _priority._textBox.Text = rule.Priority.ToString();
                _priority._textBox.KeyUp += (_, _) =>
                {
                    if (TryParsePriority(_priority._textBox))
                        _rule.Priority = decimal.Parse(_priority._textBox.Text);
                    Changed?.Invoke(this, EventArgs.Empty);
                };

                _type = new FlatComboBox
                {
                    Location = new Point(66, 2),
                    Size = new Size(121, 30),
                    Font = UiFont,
                };
                _type.Items.AddRange(ModifierTypeNames);
                _type.SelectedItem = rule.ModifierType.ToString();
                _type.SelectedIndexChanged += (_, _) =>
                {
                    if (_type.SelectedItem is string name)
                        _rule.ModifierType = Enum.Parse<ModifierType>(name);
                    Changed?.Invoke(this, EventArgs.Empty);
                };

                _tier = CreateTextBox(new Point(200, 2), new Size(59, 30), HorizontalAlignment.Center);
                _tier._textBox.Text = rule.Tier.ToString();
                _tier._textBox.KeyUp += (_, _) =>
                {
                    if (TryParseTier(_tier._textBox))
                        _rule.Tier = int.Parse(_tier._textBox.Text);
                    Changed?.Invoke(this, EventArgs.Empty);
                };

                _content = CreateTextBox(new Point(272, 2), new Size(287, 30), HorizontalAlignment.Left);
                _content._textBox.Text = rule.Content;
                _content._textBox.KeyUp += (_, _) =>
                {
                    _rule.Content = _content._textBox.Text;
                    Changed?.Invoke(this, EventArgs.Empty);
                };

                _remove = new Label
                {
                    AutoSize = true,
                    Text = "×",
                    Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point),
                    ForeColor = StaticColors.ForeGround,
                    BackColor = Color.Transparent,
                    Location = new Point(565, 5),
                    Cursor = Cursors.Hand,
                    TabStop = false,
                };
                _remove.Click += (_, _) => RemoveRequested?.Invoke(this, EventArgs.Empty);

                Controls.Add(_priority);
                Controls.Add(_type);
                Controls.Add(_tier);
                Controls.Add(_content);
                Controls.Add(_remove);
            }

            public GambleRuleRow ToRule() => new()
            {
                Priority = _rule.Priority,
                ModifierType = _rule.ModifierType,
                Tier = _rule.Tier,
                Content = _content._textBox.Text,
            };

            public void SetWidth(int width)
            {
                Width = width;
                const int removeWidth = 24;
                _content.Width = Math.Max(59, width - _content.Left - removeWidth);
                _remove.Location = new Point(width - removeWidth + 2, 5);
            }

            private static FlatTextBox CreateTextBox(Point location, Size size, HorizontalAlignment align)
            {
                return new FlatTextBox
                {
                    Location = location,
                    Size = size,
                    Font = UiFont,
                    TextAlign = align,
                };
            }

            private static bool TryParsePriority(TextBox textBox)
            {
                if (decimal.TryParse(textBox.Text, out _))
                {
                    textBox.ForeColor = StaticColors.ForeGround;
                    return true;
                }

                textBox.ForeColor = Color.Red;
                return false;
            }

            private static bool TryParseTier(TextBox textBox)
            {
                if (int.TryParse(textBox.Text, out int value) && value > 0 && value < 10)
                {
                    textBox.ForeColor = StaticColors.ForeGround;
                    return true;
                }

                textBox.ForeColor = Color.Red;
                return false;
            }
        }
    }
}
