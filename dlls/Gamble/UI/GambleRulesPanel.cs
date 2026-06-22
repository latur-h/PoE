using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.GameData;
using PoE.dlls.Settings.Mods;
using PoE.dlls.Style;

namespace PoE.dlls.Gamble.UI
{
    public sealed class GambleRulesPanel : UserControl
    {
        private const int RowHeight = 36;
        private const int HeaderHeight = 28;
        private const int AddBarHeight = 36;
        private const int ColumnGap = 7;
        private const int RoleColumnWidth = 88;
        private const int TypeColumnX = RoleColumnWidth + ColumnGap;
        private const int TypeColumnWidth = 121;
        private const int InfluenceColumnX = TypeColumnX + TypeColumnWidth + ColumnGap;
        private const int InfluenceColumnWidth = 158;
        private const int TierColumnX = InfluenceColumnX;
        private const int TierColumnWidth = 59;
        private const int EldritchContentColumnX = InfluenceColumnX + InfluenceColumnWidth + ColumnGap;

        private static int ContentColumnX(bool eldritch) =>
            eldritch ? EldritchContentColumnX : TierColumnX + TierColumnWidth + ColumnGap;

        private readonly Panel _header;
        private readonly Label _headerRole;
        private readonly Label _headerType;
        private readonly Label _headerInfluence;
        private readonly Label _headerTier;
        private readonly Label _headerContent;

        private static readonly Font UiFont = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);

        private readonly Panel _scrollPanel;
        private readonly Panel _rowsHost;
        private readonly Button _addButton;
        private readonly Dictionary<GamblePreset, PresetView> _views = [];

        private GamblePreset? _visiblePreset;
        private PresetView? _activeView;
        private bool _suppressEvents;
        private bool _inLayout;
        private bool _relayoutScheduled;
        private readonly ModSuggestionService? _modSuggestions;
        private readonly Func<GambleType>? _getGambleType;

        public GambleRulesPanel(ModSuggestionService? modSuggestions = null, Func<GambleType>? getGambleType = null)
        {
            _modSuggestions = modSuggestions;
            _getGambleType = getGambleType;
            BackColor = StaticColors.BackGround;
            Font = UiFont;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = HeaderHeight,
                BackColor = StaticColors.BackGround,
            };

            _header = header;
            _headerRole = CreateHeaderLabel("Role", 0, RoleColumnWidth);
            _headerType = CreateHeaderLabel("Item type", TypeColumnX, TypeColumnWidth);
            _headerInfluence = CreateHeaderLabel("Influence", InfluenceColumnX, InfluenceColumnWidth);
            _headerTier = CreateHeaderLabel("Tier", TierColumnX, TierColumnWidth);
            _headerContent = CreateHeaderLabel("Modifier content", ContentColumnX(false), 287);

            header.Controls.Add(_headerRole);
            header.Controls.Add(_headerType);
            header.Controls.Add(_headerInfluence);
            header.Controls.Add(_headerTier);
            header.Controls.Add(_headerContent);

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
            _addButton.Click += (_, _) => AddRow(CreateDefaultRow());

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
            _scrollPanel.Layout += (_, _) => OnScrollPanelLayout();
            _scrollPanel.HandleCreated += (_, _) => DisableHorizontalScroll();

            Controls.Add(_scrollPanel);
            Controls.Add(_addButton);
            Controls.Add(header);
        }

        public void RefreshGambleTypeLayout()
        {
            GambleType type = GetGambleType();
            bool eldritch = type == GambleType.Eldritch;
            bool roleVisible = RuleRoleMapper.IsRoleColumnVisible(type);
            _headerRole.Visible = roleVisible;
            _headerType.Visible = true;
            _headerInfluence.Visible = eldritch;
            _headerTier.Visible = !eldritch;
            _headerContent.Location = new Point(ContentColumnX(eldritch), 4);

            foreach (var view in _views.Values)
            {
                foreach (var row in view.Rows)
                {
                    row.ApplyGambleType(type);
                    row.ApplyEldritchMode(eldritch);
                    if (_modSuggestions is not null)
                    {
                        SpawnTagAutocomplete.SetEldritchArmourScope(row.TypeFilterTextBox, IsEldritchMode);
                        SpawnTagAutocomplete.Refresh(row.TypeFilterTextBox, _modSuggestions);
                    }
                }
            }

            LayoutActiveView();
        }

        private GambleType GetGambleType() => _getGambleType?.Invoke() ?? GambleType.Alt;

        private GambleRuleRow CreateDefaultRow()
        {
            var row = new GambleRuleRow();
            if (IsEldritchMode())
                row.EldritchInfluence = EldritchInfluence.SearingExarch;

            return row;
        }

        private bool IsEldritchMode() => _getGambleType?.Invoke() == GambleType.Eldritch;

        public void Bind(GamblePreset preset)
        {
            if (_visiblePreset is not null && !ReferenceEquals(_visiblePreset, preset))
                Commit();

            ShowPreset(preset);
            RefreshGambleTypeLayout();
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

            foreach (var row in rows)
                AttachRowSuggestions(row);

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
            AttachRowSuggestions(_activeView.Rows[^1]);
            LayoutActiveView();

            if (!_suppressEvents)
                _visiblePreset.Rules = _activeView.Rows.Select(r => r.ToRule()).ToList();

            ScrollToBottom();
        }

        private GambleRuleRowControl CreateRowControl(Panel host, GambleRuleRow rule, List<GambleRuleRowControl> rows)
        {
            var row = new GambleRuleRowControl(rule, GetGambleType, IsEldritchMode);
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

        private void AttachRowSuggestions(GambleRuleRowControl row)
        {
            if (_modSuggestions is null)
                return;

            SpawnTagAutocomplete.Attach(
                row.TypeFilterTextBox,
                _modSuggestions,
                onFilterChanged: () => ModSuggestionAutocomplete.RequestRefresh(row.ContentTextBox),
                useEldritchArmourScope: IsEldritchMode);

            ModSuggestionAutocomplete.Attach(row.ContentTextBox, _modSuggestions, () => ResolveSuggestionStrategy(row));
        }

        private IModSuggestionStrategy ResolveSuggestionStrategy(GambleRuleRowControl row)
        {
            GambleType type = _getGambleType?.Invoke() ?? GambleType.Alt;
            if (type == GambleType.Eldritch)
                return EldritchModSuggestionStrategy.For(row.GetEldritchInfluence(), row.GetItemTypeFilter());

            if (type is GambleType.Map or GambleType.MapExalt or GambleType.MapT17)
                return MapModSuggestionStrategy.Instance;

            return ItemModSuggestionStrategy.For(row.GetItemTypeFilter());
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
            if (_inLayout || _activeView is null)
                return;

            _inLayout = true;
            try
            {
                LayoutView(_activeView);
            }
            finally
            {
                _inLayout = false;
            }
        }

        private void OnScrollPanelLayout()
        {
            if (_inLayout || _activeView is null)
                return;

            int width = GetViewportWidth();
            if (width > 0 && width != _activeView.Host.Width)
                LayoutActiveView();
        }

        private void ScheduleViewportRelayout()
        {
            if (!IsHandleCreated || _relayoutScheduled || _activeView is null)
                return;

            _relayoutScheduled = true;
            BeginInvoke(() =>
            {
                _relayoutScheduled = false;
                if (_activeView is null)
                    return;

                int width = GetViewportWidth();
                if (width > 0 && width != _activeView.Host.Width)
                    LayoutActiveView();
            });
        }

        private void LayoutView(PresetView view)
        {
            ApplyRowLayout(view, GetViewportWidth());

            int contentHeight = Math.Max(RowHeight, view.Rows.Count * RowHeight);
            view.Host.Height = contentHeight;
            _rowsHost.Height = contentHeight;

            if (contentHeight <= _scrollPanel.ClientSize.Height)
                _scrollPanel.AutoScrollPosition = new Point(0, 0);

            // Scrollbar show/hide changes client width without always raising Resize.
            int widthAfter = GetViewportWidth();
            if (widthAfter != view.Host.Width)
                ApplyRowLayout(view, widthAfter);

            ScheduleViewportRelayout();
        }

        private int GetViewportWidth() => Math.Max(0, _scrollPanel.ClientSize.Width);

        private void ApplyRowLayout(PresetView view, int rowWidth)
        {
            view.Host.Width = rowWidth;
            _rowsHost.Width = rowWidth;

            for (int i = 0; i < view.Rows.Count; i++)
            {
                view.Rows[i].Location = new Point(0, i * RowHeight);
                view.Rows[i].SetWidth(rowWidth);
            }
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
            private static readonly string[] InfluenceLabels =
            [
                "Searing Exarch",
                "Eater of Worlds",
            ];

            private readonly Func<GambleType> _getGambleType;
            private readonly Func<bool> _isEldritchMode;
            private GambleRuleRow _rule;
            private readonly FlatComboBox _role;
            private readonly FlatTextBox _typeFilter;
            private readonly FlatComboBox _influence;
            private readonly FlatTextBox _tier;
            private readonly FlatTextBox _content;
            private readonly Label _remove;

            public event EventHandler? RemoveRequested;
            public event EventHandler? Changed;

            public GambleRuleRowControl(GambleRuleRow rule, Func<GambleType> getGambleType, Func<bool> isEldritchMode)
            {
                _getGambleType = getGambleType;
                _isEldritchMode = isEldritchMode;
                _rule = rule;
                BackColor = StaticColors.BackGround;
                Height = 34;

                _role = new FlatComboBox
                {
                    Location = new Point(0, 2),
                    Size = new Size(RoleColumnWidth, 30),
                    Font = UiFont,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                };
                _role.SelectedIndexChanged += (_, _) =>
                {
                    RuleRole role = GetSelectedRole();
                    _rule.Priority = RuleRoleMapper.ToPriority(_getGambleType(), role);
                    Changed?.Invoke(this, EventArgs.Empty);
                };

                _typeFilter = CreateTextBox(new Point(TypeColumnX, 2), new Size(TypeColumnWidth, 30), HorizontalAlignment.Left);
                _typeFilter._textBox.Text = ModSpawnTagDisplay.GetDisplayName(rule.ItemTypeFilter);
                _typeFilter._textBox.TextChanged += (_, _) =>
                {
                    _rule.ItemTypeFilter = _typeFilter._textBox.Text;
                    Changed?.Invoke(this, EventArgs.Empty);
                };

                _influence = new FlatComboBox
                {
                    Location = new Point(InfluenceColumnX, 2),
                    Size = new Size(InfluenceColumnWidth, 30),
                    Font = UiFont,
                };
                _influence.Items.AddRange(InfluenceLabels);
                _influence.SelectedIndex = rule.EldritchInfluence == EldritchInfluence.EaterOfWorlds ? 1 : 0;

                _tier = CreateTextBox(new Point(TierColumnX, 2), new Size(TierColumnWidth, 30), HorizontalAlignment.Center);
                _tier._textBox.Text = rule.Tier.ToString();
                _tier._textBox.KeyUp += (_, _) =>
                {
                    if (TryParseTier(_tier._textBox))
                        _rule.Tier = int.Parse(_tier._textBox.Text);
                    Changed?.Invoke(this, EventArgs.Empty);
                };

                _content = CreateTextBox(new Point(ContentColumnX(false), 2), new Size(287, 30), HorizontalAlignment.Left);
                _content._textBox.Text = rule.Content;
                _content._textBox.KeyUp += (_, _) =>
                {
                    _rule.Content = _content._textBox.Text;
                    Changed?.Invoke(this, EventArgs.Empty);
                };

                _influence.SelectedIndexChanged += (_, _) =>
                {
                    _rule.EldritchInfluence = _influence.SelectedIndex == 1
                        ? EldritchInfluence.EaterOfWorlds
                        : EldritchInfluence.SearingExarch;
                    Changed?.Invoke(this, EventArgs.Empty);
                    if (_isEldritchMode())
                        ModSuggestionAutocomplete.RequestRefresh(_content._textBox);
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

                Controls.Add(_role);
                Controls.Add(_typeFilter);
                Controls.Add(_influence);
                Controls.Add(_tier);
                Controls.Add(_content);
                Controls.Add(_remove);

                ApplyGambleType(_getGambleType());
                ApplyEldritchMode(_isEldritchMode());
            }

            internal void ApplyGambleType(GambleType type)
            {
                bool roleVisible = RuleRoleMapper.IsRoleColumnVisible(type);
                _role.Visible = roleVisible;
                if (!roleVisible)
                    return;

                ConfigureRoleCombo(type);
            }

            private void ConfigureRoleCombo(GambleType type)
            {
                _role.Items.Clear();
                foreach (RuleRole role in RuleRoleMapper.GetRolesForMode(type))
                    _role.Items.Add(RuleRoleMapper.GetDisplayName(role));

                SelectRole(RuleRoleMapper.FromPriority(type, _rule.Priority, _rule.Content));
            }

            private void SelectRole(RuleRole role)
            {
                string label = RuleRoleMapper.GetDisplayName(role);
                int index = _role.Items.IndexOf(label);
                if (index < 0)
                    index = 0;

                if (_role.SelectedIndex != index)
                    _role.SelectedIndex = index;
            }

            private RuleRole GetSelectedRole() =>
                RuleRoleMapper.FromDisplayName(_getGambleType(), _role.SelectedItem?.ToString());

            internal EldritchInfluence GetEldritchInfluence() =>
                _influence.SelectedIndex == 1
                    ? EldritchInfluence.EaterOfWorlds
                    : EldritchInfluence.SearingExarch;

            internal string GetItemTypeFilter() =>
                ModSpawnTagFilter.Normalize(_typeFilter._textBox.Text) ?? string.Empty;

            internal TextBox TypeFilterTextBox => _typeFilter._textBox;

            internal void ApplyEldritchMode(bool eldritch)
            {
                _influence.Visible = eldritch;
                _influence.Location = new Point(InfluenceColumnX, 2);
                _tier.Visible = !eldritch;
                _content.Location = new Point(ContentColumnX(eldritch), 2);
            }

            internal TextBox ContentTextBox => _content._textBox;

            public GambleRuleRow ToRule()
            {
                GambleType type = _getGambleType();
                decimal priority = RuleRoleMapper.ToPriority(type, GetSelectedRole());

                int tier = _rule.Tier;
                if (int.TryParse(_tier._textBox.Text, out int parsedTier) && parsedTier > 0 && parsedTier < 10)
                    tier = parsedTier;

                return new GambleRuleRow
                {
                    Priority = priority,
                    ModifierType = ModifierType.Any,
                    ItemTypeFilter = ModSpawnTagFilter.Normalize(_typeFilter._textBox.Text) ?? string.Empty,
                    Tier = tier,
                    Content = _content._textBox.Text,
                    EldritchInfluence = _isEldritchMode()
                        ? (_influence.SelectedIndex == 1 ? EldritchInfluence.EaterOfWorlds : EldritchInfluence.SearingExarch)
                        : _rule.EldritchInfluence,
                };
            }

            public void SetWidth(int width)
            {
                Width = width;
                const int removeReserve = 28;
                _content.Width = Math.Max(59, width - _content.Left - removeReserve);
                _remove.Location = new Point(width - _remove.Width - 4, 5);
                _remove.BringToFront();
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
