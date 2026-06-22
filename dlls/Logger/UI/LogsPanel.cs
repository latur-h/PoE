using PoE.dlls.Style;

namespace PoE.dlls.Logger.UI
{
    public sealed class LogsPanel : UserControl
    {
        private static readonly Font UiFont = new("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly Font MonoFont = new("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);

        private static readonly Color ColorOk = StaticColors.ForeGround;
        private static readonly Color ColorInfo = StaticColors.ForeGround;
        private static readonly Color ColorWarn = Color.FromArgb(230, 180, 80);
        private static readonly Color ColorError = Color.FromArgb(230, 100, 100);
        private static readonly Color ColorDebug = Color.FromArgb(124, 127, 151);

        private enum LevelFilter
        {
            InfoPlus,
            All,
            WarnPlus,
            ErrorsOnly,
        }

        private readonly LogBuffer _buffer;
        private readonly TextBox _searchBox;
        private readonly ComboBox _levelFilter;
        private readonly CheckBox _chkGambler;
        private readonly CheckBox _chkFlask;
        private readonly CheckBox _chkSystem;
        private readonly Label _countLabel;
        private readonly ListView _listView;

        private List<LogEntry> _filtered = [];
        private LevelFilter _levelFilterValue = LevelFilter.InfoPlus;
        private int _lastRenderedCount;

        public LogsPanel(LogBuffer buffer)
        {
            _buffer = buffer;
            BackColor = StaticColors.BackGround;
            Font = UiFont;

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = StaticColors.BackGround,
                Padding = new Padding(6, 4, 6, 4),
            };

            var searchLabel = CreateLabel("Search", new Point(0, 6));
            _searchBox = new TextBox
            {
                Location = new Point(52, 3),
                Size = new Size(160, 23),
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
                BorderStyle = BorderStyle.FixedSingle,
                Font = UiFont,
            };
            _searchBox.TextChanged += (_, _) => RebuildFilter();

            var levelLabel = CreateLabel("Level", new Point(222, 6));
            _levelFilter = new ComboBox
            {
                Location = new Point(264, 3),
                Size = new Size(90, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
                Font = UiFont,
            };
            _levelFilter.Items.AddRange(["Info+", "All", "Warn+", "Errors"]);
            _levelFilter.SelectedIndex = 0;
            _levelFilter.SelectedIndexChanged += (_, _) =>
            {
                _levelFilterValue = _levelFilter.SelectedIndex switch
                {
                    1 => LevelFilter.All,
                    2 => LevelFilter.WarnPlus,
                    3 => LevelFilter.ErrorsOnly,
                    _ => LevelFilter.InfoPlus,
                };
                RebuildFilter();
            };

            var clearButton = CreateToolbarButton("Clear", new Point(364, 2));
            clearButton.Click += (_, _) =>
            {
                _lastRenderedCount = 0;
                _buffer.Clear();
            };

            var copyButton = CreateToolbarButton("Copy", new Point(424, 2));
            copyButton.Click += (_, _) => CopyVisible();

            _countLabel = CreateLabel("0 / 0", new Point(484, 6));
            _countLabel.AutoSize = true;

            _chkGambler = CreateCategoryCheck("Gambler", new Point(0, 34), true);
            _chkFlask = CreateCategoryCheck("Flask", new Point(90, 34), true);
            _chkSystem = CreateCategoryCheck("System", new Point(160, 34), true);
            _chkGambler.CheckedChanged += (_, _) => RebuildFilter();
            _chkFlask.CheckedChanged += (_, _) => RebuildFilter();
            _chkSystem.CheckedChanged += (_, _) => RebuildFilter();

            toolbar.Controls.Add(searchLabel);
            toolbar.Controls.Add(_searchBox);
            toolbar.Controls.Add(levelLabel);
            toolbar.Controls.Add(_levelFilter);
            toolbar.Controls.Add(clearButton);
            toolbar.Controls.Add(copyButton);
            toolbar.Controls.Add(_countLabel);
            toolbar.Controls.Add(_chkGambler);
            toolbar.Controls.Add(_chkFlask);
            toolbar.Controls.Add(_chkSystem);

            _listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                VirtualMode = true,
                VirtualListSize = 0,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                BorderStyle = BorderStyle.None,
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
                Font = MonoFont,
                MultiSelect = true,
            };

            _listView.Columns.Add("Time", 64);
            _listView.Columns.Add("Cat", 36);
            _listView.Columns.Add("Lvl", 36);
            _listView.Columns.Add("Message", 460);

            _listView.RetrieveVirtualItem += OnRetrieveVirtualItem;
            _listView.CacheVirtualItems += (_, e) => { };
            _listView.KeyDown += (_, e) =>
            {
                if (e.Control && e.KeyCode == Keys.C)
                    CopySelection();
            };

            var scrollWatcher = new Panel { Dock = DockStyle.Fill };
            scrollWatcher.Controls.Add(_listView);
            _listView.Resize += (_, _) => UpdateMessageColumnWidth();

            Controls.Add(scrollWatcher);
            Controls.Add(toolbar);

            _buffer.Changed += OnBufferChanged;
            RebuildFilter();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _buffer.Changed -= OnBufferChanged;

            base.Dispose(disposing);
        }

        private void OnBufferChanged()
        {
            if (InvokeRequired)
            {
                BeginInvoke(RebuildFilter);
                return;
            }

            RebuildFilter();
        }

        private void RebuildFilter()
        {
            string search = _searchBox.Text.Trim();
            var snapshot = _buffer.Snapshot();
            bool wasEmpty = _lastRenderedCount == 0;

            _filtered = snapshot
                .Where(PassesCategoryFilter)
                .Where(PassesLevelFilter)
                .Where(e => e.MatchesSearch(search))
                .ToList();

            _listView.BeginUpdate();
            try
            {
                _listView.VirtualListSize = _filtered.Count;
                if (_filtered.Count > 0)
                    _listView.RedrawItems(0, _filtered.Count - 1, true);
                else
                    _listView.Invalidate();
            }
            finally
            {
                _listView.EndUpdate();
            }

            _lastRenderedCount = _filtered.Count;
            UpdateMessageColumnWidth();

            _countLabel.Text = $"{_filtered.Count} / {_buffer.Count}";

            if (_filtered.Count == 0)
                return;

            if (wasEmpty)
                ScrollToHead();
            else
                ScrollToTail();
        }

        private void OnRetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e)
        {
            var entry = _filtered[e.ItemIndex];
            Color color = SeverityColor(entry.Severity);
            var item = new ListViewItem(entry.TimeText)
            {
                ForeColor = color,
                BackColor = StaticColors.BackGround,
                UseItemStyleForSubItems = true,
            };
            item.SubItems.Add(entry.CategoryCode);
            item.SubItems.Add(entry.SeverityCode.Trim());
            item.SubItems.Add(entry.Message);
            foreach (ListViewItem.ListViewSubItem subItem in item.SubItems)
            {
                subItem.ForeColor = color;
                subItem.BackColor = StaticColors.BackGround;
            }

            e.Item = item;
        }

        private bool PassesCategoryFilter(LogEntry entry) => entry.Category switch
        {
            LogCategory.Gambler => _chkGambler.Checked,
            LogCategory.Flask => _chkFlask.Checked,
            LogCategory.System => _chkSystem.Checked,
            _ => true,
        };

        private bool PassesLevelFilter(LogEntry entry) => _levelFilterValue switch
        {
            LevelFilter.All => true,
            LevelFilter.InfoPlus => entry.Severity != LogSeverity.Debug,
            LevelFilter.WarnPlus => entry.Severity is LogSeverity.Warn or LogSeverity.Error,
            LevelFilter.ErrorsOnly => entry.Severity == LogSeverity.Error,
            _ => true,
        };

        private static Color SeverityColor(LogSeverity severity) => severity switch
        {
            LogSeverity.Ok => ColorOk,
            LogSeverity.Info => ColorInfo,
            LogSeverity.Warn => ColorWarn,
            LogSeverity.Error => ColorError,
            LogSeverity.Debug => ColorDebug,
            _ => ColorInfo,
        };

        private void CopyVisible()
        {
            if (_filtered.Count == 0)
                return;

            Clipboard.SetText(string.Join(Environment.NewLine, _filtered.Select(e => e.FormatLine())));
        }

        private void CopySelection()
        {
            if (_listView.SelectedIndices.Count == 0)
            {
                CopyVisible();
                return;
            }

            var lines = _listView.SelectedIndices
                .Cast<int>()
                .OrderBy(i => i)
                .Select(i => _filtered[i].FormatLine());

            Clipboard.SetText(string.Join(Environment.NewLine, lines));
        }

        private void ScrollToHead()
        {
            if (_filtered.Count == 0)
                return;

            void Scroll()
            {
                try
                {
                    _listView.EnsureVisible(0);
                }
                catch
                {
                    // Virtual list may not have materialized the row yet.
                }
            }

            if (InvokeRequired)
                BeginInvoke(Scroll);
            else
                Scroll();
        }

        private void ScrollToTail()
        {
            if (_filtered.Count == 0)
                return;

            int lastIndex = _filtered.Count - 1;

            void Scroll()
            {
                if (lastIndex >= _listView.VirtualListSize)
                    return;

                try
                {
                    _listView.EnsureVisible(lastIndex);
                }
                catch
                {
                    // Virtual list may not have materialized the row yet.
                }
            }

            if (InvokeRequired)
                BeginInvoke(Scroll);
            else
                Scroll();
        }

        private void UpdateMessageColumnWidth()
        {
            if (_listView.Columns.Count < 4)
                return;

            int fixedWidth = _listView.Columns[0].Width + _listView.Columns[1].Width + _listView.Columns[2].Width + 8;
            _listView.Columns[3].Width = Math.Max(120, _listView.ClientSize.Width - fixedWidth);
        }

        private static Label CreateLabel(string text, Point location) =>
            new()
            {
                AutoSize = true,
                Text = text,
                Location = location,
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = UiFont,
            };

        private static Button CreateToolbarButton(string text, Point location) =>
            new()
            {
                Text = text,
                Location = location,
                Size = new Size(54, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
                Font = UiFont,
                Cursor = Cursors.Hand,
                TabStop = false,
            };

        private static CheckBox CreateCategoryCheck(string text, Point location, bool enabled) =>
            new()
            {
                AutoSize = true,
                Text = text,
                Location = location,
                Checked = enabled,
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = UiFont,
            };
    }
}
