using PoE.dlls.Style;

namespace PoE.dlls.GameData
{
    public static class SpawnTagAutocomplete
    {
        private const int DebounceMs = 250;
        private const int MinSearchLength = 1;
        private const int PageSize = 50;
        private const int MaxVisibleItems = 12;

        public static void Attach(
            TextBox textBox,
            ModSuggestionService service,
            Action? onFilterChanged = null,
            Func<bool>? useEldritchArmourScope = null)
        {
            if (textBox.Tag is AttachState existing)
            {
                existing.UseEldritchArmourScope = useEldritchArmourScope;
                return;
            }

            SuggestionPopup? popup = null;
            var validateTimer = new System.Windows.Forms.Timer { Interval = DebounceMs };

            void EnsurePopup()
            {
                if (popup is not null || textBox.IsDisposed)
                    return;

                if (textBox.FindForm() is null)
                    return;

                popup = new SuggestionPopup(textBox, service);
                textBox.Tag = new AttachState
                {
                    Popup = popup,
                    ValidateTimer = validateTimer,
                    UseEldritchArmourScope = useEldritchArmourScope,
                };
                textBox.Disposed += (_, _) =>
                {
                    validateTimer.Stop();
                    validateTimer.Dispose();
                    popup.Dispose();
                    textBox.Tag = null;
                };
            }

            validateTimer.Tick += (_, _) =>
            {
                validateTimer.Stop();
                ValidateTag(textBox, service);
            };

            textBox.TextChanged += (_, _) =>
            {
                ScheduleValidation(validateTimer);
                onFilterChanged?.Invoke();

                if (!textBox.Focused)
                    return;

                EnsurePopup();
                popup?.RequestRefresh();
            };

            textBox.GotFocus += (_, _) =>
            {
                EnsurePopup();
                popup?.NotifyFocused();
                ScheduleValidation(validateTimer);
            };

            textBox.HandleCreated += (_, _) => EnsurePopup();
            textBox.LostFocus += (_, _) => ValidateTag(textBox, service);

            for (Control? ancestor = textBox.Parent; ancestor is not null; ancestor = ancestor.Parent)
            {
                ancestor.ParentChanged += (_, _) => EnsurePopup();
                ancestor.HandleCreated += (_, _) => EnsurePopup();
            }

            EnsurePopup();
            ValidateTag(textBox, service);
        }

        public static void SetEldritchArmourScope(TextBox textBox, Func<bool>? useEldritchArmourScope)
        {
            if (textBox.Tag is AttachState state)
                state.UseEldritchArmourScope = useEldritchArmourScope;
        }

        public static void Refresh(TextBox textBox, ModSuggestionService service)
        {
            ValidateTag(textBox, service);
            if (textBox.Tag is AttachState state)
                state.Popup.RequestRefresh();
        }

        private static bool UseEldritchArmourScope(TextBox textBox) =>
            textBox.Tag is AttachState state && state.UseEldritchArmourScope?.Invoke() == true;

        private static void ValidateTag(TextBox textBox, ModSuggestionService service)
        {
            if (textBox.IsDisposed)
                return;

            string trimmed = textBox.Text.Trim();
            if (trimmed.Length == 0)
            {
                textBox.ForeColor = StaticColors.ForeGround;
                return;
            }

            if (!service.IsReady)
            {
                textBox.ForeColor = StaticColors.ForeGround;
                return;
            }

            bool exists = UseEldritchArmourScope(textBox)
                ? service.SpawnTagExists(trimmed, eldritchArmourOnly: true)
                : service.SpawnTagExists(trimmed);

            textBox.ForeColor = exists
                ? StaticColors.ForeGround
                : Color.Red;
        }

        private static void ScheduleValidation(System.Windows.Forms.Timer timer)
        {
            timer.Stop();
            timer.Start();
        }

        private sealed class AttachState
        {
            public required SuggestionPopup Popup { get; init; }
            public required System.Windows.Forms.Timer ValidateTimer { get; init; }
            public Func<bool>? UseEldritchArmourScope { get; set; }
        }

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

        private sealed class SuggestionPopup : IDisposable
        {
            private readonly TextBox _textBox;
            private readonly ModSuggestionService _service;
            private readonly Form _owner;
            private readonly SuggestionDropDownForm _popup;
            private readonly ListBox _list;
            private readonly System.Windows.Forms.Timer _debounce;
            private readonly System.Windows.Forms.Timer _hideTimer;
            private readonly List<string> _tags = [];
            private string _lastTerm = string.Empty;
            private bool _selecting;
            private IMessageFilter? _clickOutsideFilter;

            public SuggestionPopup(TextBox textBox, ModSuggestionService service)
            {
                _textBox = textBox;
                _service = service;
                _owner = textBox.FindForm()
                    ?? throw new InvalidOperationException("Autocomplete requires a parent form.");

                _list = new ListBox
                {
                    Dock = DockStyle.Fill,
                    BorderStyle = BorderStyle.FixedSingle,
                    IntegralHeight = false,
                    Font = textBox.Font,
                    BackColor = StaticColors.BackGround,
                    ForeColor = StaticColors.ForeGround,
                    TabStop = false,
                };
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
                    Size = new Size(220, 160),
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

                _textBox.LostFocus += (_, _) =>
                {
                    if (_selecting)
                        return;

                    _hideTimer.Stop();
                    _hideTimer.Start();
                };

                _owner.LocationChanged += (_, _) =>
                {
                    if (_popup.Visible)
                        RepositionPopup();
                };
                _owner.SizeChanged += (_, _) =>
                {
                    if (_popup.Visible)
                        RepositionPopup();
                };
                _owner.VisibleChanged += (_, _) =>
                {
                    if (!_owner.Visible)
                        HidePopup();
                };
            }

            internal void NotifyFocused() => ScheduleRefresh();

            public void Dispose()
            {
                RemoveClickOutsideFilter();
                _debounce.Dispose();
                _hideTimer.Dispose();
                _popup.Dispose();
            }

            internal void RequestRefresh() => ScheduleRefresh();

            private void ScheduleRefresh()
            {
                if (!_textBox.Focused)
                    return;

                _debounce.Stop();
                _debounce.Start();
            }

            private void RefreshSuggestions()
            {
                if (_textBox.IsDisposed || !_textBox.Focused)
                {
                    HidePopup();
                    return;
                }

                if (!_service.IsReady)
                {
                    HidePopup();
                    return;
                }

                string term = _textBox.Text.Trim();
                if (term.Length < MinSearchLength)
                {
                    HidePopup();
                    return;
                }

                IReadOnlyList<string> tags = UseEldritchArmourScope(_textBox)
                    ? _service.SearchSpawnTags(term, PageSize, eldritchArmourOnly: true)
                    : _service.SearchSpawnTags(term, PageSize);
                if (tags.Count == 0)
                {
                    HidePopup();
                    return;
                }

                _lastTerm = term;
                _tags.Clear();
                _list.BeginUpdate();
                _list.Items.Clear();
                foreach (string tag in tags)
                {
                    _tags.Add(tag);
                    _list.Items.Add(ModSpawnTagDisplay.FormatListItem(tag));
                }
                _list.EndUpdate();
                ShowPopup();
            }

            private void ShowPopup()
            {
                RepositionPopup();
                if (!_popup.Visible)
                {
                    _popup.Show(_owner);
                    InstallClickOutsideFilter();
                }
            }

            private void RepositionPopup()
            {
                if (_list.Items.Count == 0)
                    return;

                Control anchor = _textBox.Parent ?? _textBox;
                Point screen = anchor.PointToScreen(new Point(0, anchor.Height + 2));
                int width = Math.Max(anchor.Width, 180);
                int visibleRows = Math.Min(MaxVisibleItems, Math.Max(_list.Items.Count, 1));
                int height = Math.Min(280, Math.Max(72, _list.ItemHeight * visibleRows + 8));
                _popup.Size = new Size(width, height);
                _popup.Location = screen;
            }

            private void HidePopup()
            {
                _debounce.Stop();
                RemoveClickOutsideFilter();
                if (_popup.Visible)
                    _popup.Hide();
                _list.Items.Clear();
                _tags.Clear();
                _lastTerm = string.Empty;
            }

            private void AcceptSelection()
            {
                int index = _list.SelectedIndex;
                if (index < 0 || index >= _list.Items.Count)
                {
                    _selecting = false;
                    return;
                }

                _selecting = true;
                _debounce.Stop();
                try
                {
                    _textBox.Text = ModSpawnTagDisplay.GetDisplayName(_tags[index]);
                    _textBox.SelectionStart = _textBox.Text.Length;
                    HidePopup();
                    _textBox.Focus();
                }
                finally
                {
                    _selecting = false;
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

                Control anchor = _textBox.Parent ?? _textBox;
                Rectangle anchorBounds = new(anchor.PointToScreen(Point.Empty), anchor.Size);
                return anchorBounds.Contains(screen);
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

                    popup.HidePopup();
                    return false;
                }
            }
        }
    }
}
