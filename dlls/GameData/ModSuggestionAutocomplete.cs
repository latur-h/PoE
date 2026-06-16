using PoE.dlls.Style;

namespace PoE.dlls.GameData
{
    public static class ModSuggestionAutocomplete
    {
        private const int DebounceMs = 200;
        private const int MinSearchLength = 2;
        private const int PageSize = 50;
        private const int MaxVisibleItems = 10;

        public static void Attach(
            TextBox textBox,
            ModSuggestionService service,
            Func<IModSuggestionStrategy>? getStrategy = null)
        {
            if (ReferenceEquals(textBox.Tag, AttachedMarker))
                return;

            textBox.Tag = AttachedMarker;
            IModSuggestionStrategy Strategy() => getStrategy?.Invoke() ?? ItemModSuggestionStrategy.Instance;

            SuggestionPopup? popup = null;

            void EnsurePopup()
            {
                if (popup is not null || textBox.IsDisposed)
                    return;

                if (textBox.FindForm() is null)
                    return;

                popup = new SuggestionPopup(textBox, service, Strategy);
                textBox.Disposed += (_, _) =>
                {
                    popup.Dispose();
                    if (ReferenceEquals(textBox.Tag, AttachedMarker))
                        textBox.Tag = null;
                };
            }

            textBox.GotFocus += (_, _) =>
            {
                EnsurePopup();
                popup?.NotifyFocused();
            };
            textBox.TextChanged += (_, _) =>
            {
                if (!textBox.Focused)
                    return;

                EnsurePopup();
                popup?.RequestRefresh();
            };
            textBox.HandleCreated += (_, _) => EnsurePopup();

            for (Control? ancestor = textBox.Parent; ancestor is not null; ancestor = ancestor.Parent)
            {
                ancestor.ParentChanged += (_, _) => EnsurePopup();
                ancestor.HandleCreated += (_, _) => EnsurePopup();
            }

            EnsurePopup();
        }

        private static readonly object AttachedMarker = new();

        private sealed class SuggestionDropDownForm : Form
        {
            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    const int wsExNoActivate = 0x08000000;
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= wsExNoActivate;
                    return cp;
                }
            }
        }

        private sealed class ScrollableListBox : ListBox
        {
            public event EventHandler? Scrolled;

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                const int WM_VSCROLL = 0x115;
                const int WM_MOUSEWHEEL = 0x20A;
                if (m.Msg is WM_VSCROLL or WM_MOUSEWHEEL)
                    Scrolled?.Invoke(this, EventArgs.Empty);
            }
        }

        private sealed class SuggestionPopup : IDisposable
        {
            private readonly TextBox _textBox;
            private readonly ModSuggestionService _service;
            private readonly Func<IModSuggestionStrategy> _getStrategy;
            private readonly Form _owner;
            private readonly SuggestionDropDownForm _popup;
            private readonly ScrollableListBox _list;
            private readonly System.Windows.Forms.Timer _debounce;
            private readonly System.Windows.Forms.Timer _hideTimer;
            private readonly List<ModSuggestionItem> _items = [];
            private string _lastTerm = string.Empty;
            private int _keyboardIndex = -1;
            private bool _selecting;
            private bool _hasMore;
            private bool _loadingMore;
            private bool _blockAutoShow;
            private IMessageFilter? _clickOutsideFilter;

            private ModContentSearchSegment _lastSegment;

            public SuggestionPopup(TextBox textBox, ModSuggestionService service, Func<IModSuggestionStrategy> getStrategy)
            {
                _textBox = textBox;
                _service = service;
                _getStrategy = getStrategy;
                _owner = textBox.FindForm()
                    ?? throw new InvalidOperationException("Autocomplete requires a parent form.");

                _list = new ScrollableListBox
                {
                    Dock = DockStyle.Fill,
                    BorderStyle = BorderStyle.FixedSingle,
                    IntegralHeight = false,
                    Font = textBox.Font,
                    BackColor = StaticColors.BackGround,
                    ForeColor = StaticColors.ForeGround,
                    TabStop = false,
                };
                _list.Scrolled += (_, _) => TryLoadMore();
                _list.MouseDown += (_, _) => _selecting = true;
                _list.Click += (_, _) => AcceptSelection();

                _popup = new SuggestionDropDownForm
                {
                    FormBorderStyle = FormBorderStyle.None,
                    ShowInTaskbar = false,
                    StartPosition = FormStartPosition.Manual,
                    AutoSize = false,
                    BackColor = StaticColors.BackGround,
                    Owner = _owner,
                    Size = new Size(420, 160),
                };
                _popup.Controls.Add(_list);

                _debounce = new System.Windows.Forms.Timer { Interval = DebounceMs };
                _debounce.Tick += (_, _) =>
                {
                    _debounce.Stop();
                    RefreshSuggestions();
                };

                _hideTimer = new System.Windows.Forms.Timer { Interval = 150 };
                _hideTimer.Tick += (_, _) =>
                {
                    _hideTimer.Stop();
                    if (_selecting || _textBox.Focused || _popup.Bounds.Contains(Control.MousePosition))
                        return;

                    HidePopup();
                };

                _textBox.TextChanged += (_, _) =>
                {
                    if (_textBox.Focused)
                        ScheduleRefresh();
                };
                _textBox.GotFocus += (_, _) =>
                {
                    if (!_blockAutoShow)
                        ScheduleRefresh();
                };
                _textBox.MouseDown += (_, _) => _blockAutoShow = false;
                _textBox.KeyDown += (_, e) =>
                {
                    if (e.KeyCode is Keys.Back or Keys.Delete
                        or >= Keys.Space and <= Keys.Z
                        or >= Keys.NumPad0 and <= Keys.NumPad9)
                    {
                        _blockAutoShow = false;
                    }

                    OnTextBoxKeyDown(_, e);
                };
                _textBox.LostFocus += (_, _) =>
                {
                    if (_selecting)
                        return;

                    _hideTimer.Stop();
                    _hideTimer.Start();
                };

                _owner.LocationChanged += (_, _) => OnOwnerLayoutChanged();
                _owner.SizeChanged += (_, _) => OnOwnerLayoutChanged();
                _owner.VisibleChanged += (_, _) =>
                {
                    if (!_owner.Visible)
                        HidePopup();
                };

                if (FindParent<TabControl>(_textBox) is TabControl tabControl)
                    tabControl.SelectedIndexChanged += (_, _) => HidePopup();

                for (Control? ancestor = _textBox.Parent; ancestor is not null; ancestor = ancestor.Parent)
                    ancestor.VisibleChanged += (_, _) => OnAnchorVisibilityChanged();
            }

            internal void NotifyFocused()
            {
                if (_textBox.IsDisposed)
                    return;

                _blockAutoShow = false;
                ScheduleRefresh();
            }

            private void OnOwnerLayoutChanged()
            {
                if (_popup.Visible)
                    RepositionPopup();
            }

            private void OnAnchorVisibilityChanged()
            {
                if (!IsAnchorDisplayed())
                    HidePopup();
            }

            private static T? FindParent<T>(Control control) where T : Control
            {
                for (Control? parent = control.Parent; parent is not null; parent = parent.Parent)
                {
                    if (parent is T match)
                        return match;
                }

                return null;
            }

            private bool IsAnchorDisplayed()
            {
                if (!_textBox.IsHandleCreated || !_textBox.Visible)
                    return false;

                for (Control? control = _textBox; control is not null; control = control.Parent)
                {
                    if (!control.Visible)
                        return false;

                    if (control.Parent is TabControl tabControl && control is TabPage page && tabControl.SelectedTab != page)
                        return false;
                }

                return _owner.Visible;
            }

            private Control AnchorControl => _textBox.Parent ?? _textBox;

            public void Dispose()
            {
                RemoveClickOutsideFilter();
                _debounce.Dispose();
                _hideTimer.Dispose();
                _popup.Dispose();
            }

            private void ScheduleRefresh()
            {
                if (_blockAutoShow || !_textBox.Focused)
                    return;

                _debounce.Stop();
                _debounce.Start();
            }

            internal void RequestRefresh()
            {
                if (_textBox.IsDisposed)
                    return;

                ScheduleRefresh();
            }

            private void RefreshSuggestions()
            {
                if (_textBox.IsDisposed)
                    return;

                try
                {
                    if (!_textBox.Focused || !IsAnchorDisplayed())
                    {
                        HidePopup();
                        return;
                    }

                    if (!_service.IsReady)
                    {
                        HidePopup();
                        return;
                    }

                    string term = GetSearchTerm(_textBox, out _lastSegment);
                    if (term.Length < MinSearchLength)
                    {
                        HidePopup();
                        return;
                    }

                    ResetResults();
                    _lastTerm = term;
                    IModSuggestionStrategy strategy = _getStrategy();
                    IReadOnlyList<ModSuggestionItem> suggestions = _service.Search(term, strategy, PageSize, 0);
                    if (suggestions.Count == 0)
                    {
                        HidePopup();
                        return;
                    }

                    AppendSuggestions(suggestions, strategy);
                    _hasMore = suggestions.Count >= PageSize;
                    _keyboardIndex = -1;
                    ShowPopup();
                }
                catch (Exception ex)
                {
                    GameDataLog.Error($"Mod autocomplete failed: {ex.Message}", ex);
                    HidePopup();
                }
            }

            private void ResetResults()
            {
                _items.Clear();
                _list.Items.Clear();
                _hasMore = true;
                _loadingMore = false;
            }

            private void AppendSuggestions(IReadOnlyList<ModSuggestionItem> suggestions, IModSuggestionStrategy strategy)
            {
                if (suggestions.Count == 0)
                    return;

                _list.BeginUpdate();
                foreach (ModSuggestionItem item in suggestions)
                {
                    _items.Add(item);
                    _list.Items.Add(strategy.FormatDisplay(item, _lastTerm));
                }

                _list.EndUpdate();
            }

            private void TryLoadMore()
            {
                if (_loadingMore || !_hasMore || string.IsNullOrEmpty(_lastTerm))
                    return;

                if (_list.Items.Count == 0)
                    return;

                int visible = Math.Max(1, _list.ClientSize.Height / Math.Max(1, _list.ItemHeight));
                if (_list.TopIndex + visible < _list.Items.Count - 2)
                    return;

                LoadMoreSuggestions();
            }

            private void LoadMoreSuggestions()
            {
                if (_loadingMore || !_hasMore || string.IsNullOrEmpty(_lastTerm))
                    return;

                _loadingMore = true;
                try
                {
                    IModSuggestionStrategy strategy = _getStrategy();
                    IReadOnlyList<ModSuggestionItem> suggestions = _service.Search(_lastTerm, strategy, PageSize, _items.Count);
                    _hasMore = suggestions.Count >= PageSize;
                    AppendSuggestions(suggestions, strategy);
                    RepositionPopup();
                }
                catch (Exception ex)
                {
                    GameDataLog.Error($"Mod autocomplete pagination failed: {ex.Message}", ex);
                }
                finally
                {
                    _loadingMore = false;
                }
            }

            private void ShowPopup()
            {
                if (!_textBox.Focused || !IsAnchorDisplayed())
                {
                    HidePopup();
                    return;
                }

                RepositionPopup();
                if (!_popup.Visible)
                {
                    _popup.Show(_owner);
                    InstallClickOutsideFilter();
                }
            }

            private void InstallClickOutsideFilter()
            {
                if (_clickOutsideFilter is not null)
                    return;

                _clickOutsideFilter = new OutsideClickFilter(this);
                Application.AddMessageFilter(_clickOutsideFilter);
            }

            private void RemoveClickOutsideFilter()
            {
                if (_clickOutsideFilter is null)
                    return;

                Application.RemoveMessageFilter(_clickOutsideFilter);
                _clickOutsideFilter = null;
            }

            private bool IsInsideAutocompleteArea(Point screen)
            {
                if (_popup.Visible && _popup.Bounds.Contains(screen))
                    return true;

                Control anchor = AnchorControl;
                Rectangle anchorBounds = new(anchor.PointToScreen(Point.Empty), anchor.Size);
                return anchorBounds.Contains(screen);
            }

            private void DismissForOutsideClick()
            {
                if (_selecting)
                    return;

                HidePopup();
            }

            private void RepositionPopup()
            {
                if (_items.Count == 0 || !IsAnchorDisplayed())
                    return;

                Control anchor = AnchorControl;
                Point screen = anchor.PointToScreen(new Point(0, anchor.Height + 2));
                int width = Math.Max(anchor.Width, _getStrategy().SuggestionPopupMinWidth);
                int visibleRows = Math.Min(MaxVisibleItems, Math.Max(_list.Items.Count, 1));
                int height = Math.Min(320, Math.Max(80, _list.ItemHeight * visibleRows + 8));
                _popup.Size = new Size(width, height);
                _popup.Location = screen;
            }

            private void HidePopup()
            {
                _debounce.Stop();
                RemoveClickOutsideFilter();

                if (_popup.Visible)
                    _popup.Hide();

                _keyboardIndex = -1;
                ResetResults();
                _lastTerm = string.Empty;
            }

            private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
            {
                if (!_popup.Visible || _list.Items.Count == 0)
                    return;

                if (e.KeyCode == Keys.Down)
                {
                    _keyboardIndex = Math.Min(_keyboardIndex + 1, _list.Items.Count - 1);
                    _list.SelectedIndex = _keyboardIndex;
                    if (_keyboardIndex >= _list.Items.Count - 1)
                        LoadMoreSuggestions();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Up)
                {
                    _keyboardIndex = Math.Max(_keyboardIndex - 1, -1);
                    _list.SelectedIndex = _keyboardIndex;
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Enter && _keyboardIndex >= 0)
                {
                    AcceptSelection();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    HidePopup();
                    e.Handled = true;
                }
            }

            private void AcceptSelection()
            {
                int index = _list.SelectedIndex;
                if (index < 0 || index >= _items.Count)
                {
                    _selecting = false;
                    return;
                }

                ModSuggestionItem item = _items[index];
                IModSuggestionStrategy strategy = _getStrategy();
                _selecting = true;
                _blockAutoShow = true;
                _debounce.Stop();
                try
                {
                    ModContentSearchSegment.ReplaceActiveSegment(_textBox, strategy.FormatInsert(item, _lastTerm));
                    HidePopup();
                    _textBox.Focus();
                }
                finally
                {
                    _selecting = false;
                }
            }

            private static string GetSearchTerm(TextBox textBox, out ModContentSearchSegment segment)
            {
                segment = ModContentSearchSegment.Resolve(textBox.Text, textBox.SelectionStart);
                return segment.Phrase;
            }

            private sealed class OutsideClickFilter(SuggestionPopup popup) : IMessageFilter
            {
                public bool PreFilterMessage(ref Message m)
                {
                    const int WM_LBUTTONDOWN = 0x0201;
                    if (m.Msg != WM_LBUTTONDOWN || !popup._popup.Visible)
                        return false;

                    if (popup.IsInsideAutocompleteArea(Control.MousePosition))
                        return false;

                    popup.DismissForOutsideClick();
                    return false;
                }
            }
        }
    }
}
