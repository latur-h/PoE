using System.ComponentModel;
using Microsoft.Web.WebView2.WinForms;
using PoE.dlls.Style;
using PoE.dlls.UI.Markdown;

namespace PoE.dlls.UI.Markdown
{
    public sealed class MarkdownDocumentEditor : UserControl
    {
        private static readonly Font EditorFont = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);

        private readonly Panel _surface;
        private readonly WebView2 _preview;
        private readonly FlatTextBox _editor;
        private readonly Label _statusLabel;

        private string _markdown = string.Empty;
        private bool _isEditing;
        private bool _webViewReady;
        private bool _exitEditScheduled;

        public MarkdownDocumentEditor()
        {
            BackColor = StaticColors.BackGround;

            _surface = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = StaticColors.BackGround,
            };

            _preview = new WebView2
            {
                Dock = DockStyle.Fill,
                DefaultBackgroundColor = StaticColors.BackGround,
            };
            _preview.DoubleClick += (_, _) => EnterEditMode();

            _editor = new FlatTextBox
            {
                Dock = DockStyle.Fill,
                Visible = false,
                Font = EditorFont,
            };
            _editor._textBox.Multiline = true;
            _editor._textBox.AcceptsTab = true;
            _editor._textBox.WordWrap = true;
            _editor._textBox.ScrollBars = ScrollBars.Vertical;
            _editor._textBox.BorderStyle = BorderStyle.None;
            _editor._textBox.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    e.SuppressKeyPress = true;
                    ExitEditMode();
                }
            };
            _editor._textBox.Leave += (_, _) => ScheduleExitEditIfFocusLeft();

            _statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                Text = "Preview — double-click to edit. Copy uses plain text.",
                ForeColor = StaticColors.TabControlForeGround,
                BackColor = StaticColors.BackGround,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0),
            };

            _surface.Controls.Add(_preview);
            _surface.Controls.Add(_editor);

            Controls.Add(_surface);
            Controls.Add(_statusLabel);

            InitializeWebView();
        }

        public event EventHandler? MarkdownChanged;

        public bool IsEditing => _isEditing;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Markdown
        {
            get => _isEditing ? _editor._textBox.Text : _markdown;
            set
            {
                if (_isEditing)
                    ExitEditMode(commit: true);

                _markdown = value ?? string.Empty;
                if (_webViewReady && !_isEditing)
                    RefreshPreview();
            }
        }

        public void CommitEdit()
        {
            if (_isEditing)
                ExitEditMode(commit: true);
        }

        public void EnterEditMode()
        {
            if (_isEditing || ReadOnly)
                return;

            _isEditing = true;
            _exitEditScheduled = false;
            _editor._textBox.Text = _markdown;

            ShowEditorSurface();
            _statusLabel.Text = "Edit — Esc or click outside to save.";
            _editor._textBox.Focus();
            _editor._textBox.Select(_editor._textBox.TextLength, 0);
        }

        public void RefreshPreview()
        {
            if (!_webViewReady || _isEditing)
                return;

            _preview.CoreWebView2.NavigateToString(MarkdownHtmlRenderer.ToPreviewDocument(_markdown));
        }

        public void AttachHints(ToolTip toolTip, string editorHint, string copyChipHint)
        {
            toolTip.SetToolTip(_surface, editorHint);
            toolTip.SetToolTip(_preview, editorHint);
            toolTip.SetToolTip(_editor, editorHint);
            toolTip.SetToolTip(_statusLabel, $"{editorHint} {copyChipHint}");
        }

        [DefaultValue(false)]
        public bool ReadOnly { get; set; }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_isEditing && keyData == Keys.Escape)
            {
                ExitEditMode();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void InitializeWebView()
        {
            try
            {
                await _preview.EnsureCoreWebView2Async();
                _preview.CoreWebView2.Settings.IsStatusBarEnabled = false;
                _preview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                _preview.CoreWebView2.Settings.IsZoomControlEnabled = false;
                _preview.CoreWebView2.WebMessageReceived += (_, args) =>
                {
                    string raw = args.TryGetWebMessageAsString();
                    if (string.Equals(raw, "edit", StringComparison.Ordinal))
                    {
                        BeginInvoke(EnterEditMode);
                        return;
                    }

                    if (raw.StartsWith('{'))
                        HandlePreviewWebMessage(raw);
                };

                _webViewReady = true;
                RefreshPreview();
            }
            catch (Exception)
            {
                _statusLabel.Text = "Preview unavailable — install WebView2 Runtime to render notes.";
            }
        }

        private void HandlePreviewWebMessage(string raw)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(raw);
                if (!string.Equals(doc.RootElement.GetProperty("type").GetString(), "copy", StringComparison.Ordinal))
                    return;

                string? text = doc.RootElement.GetProperty("text").GetString();
                if (string.IsNullOrEmpty(text))
                    return;

                if (InvokeRequired)
                    BeginInvoke(() => CopyPreviewText(text));
                else
                    CopyPreviewText(text);
            }
            catch (System.Text.Json.JsonException)
            {
            }
        }

        private static void CopyPreviewText(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception)
            {
            }
        }

        private void ShowEditorSurface()
        {
            if (_surface.Controls.Contains(_preview))
                _surface.Controls.Remove(_preview);

            _editor.Visible = true;
            _editor.Dock = DockStyle.Fill;
            _editor.BringToFront();
        }

        private void ShowPreviewSurface()
        {
            _editor.Visible = false;

            if (!_surface.Controls.Contains(_preview))
            {
                _surface.Controls.Add(_preview);
                _preview.Dock = DockStyle.Fill;
            }

            _preview.Visible = true;
            _preview.Enabled = true;
            _preview.BringToFront();
        }

        private void ScheduleExitEditIfFocusLeft()
        {
            if (!_isEditing || _exitEditScheduled)
                return;

            _exitEditScheduled = true;
            BeginInvoke(ExitEditIfFocusLeft);
        }

        private void ExitEditIfFocusLeft()
        {
            _exitEditScheduled = false;

            if (!_isEditing)
                return;

            if (_editor._textBox.Focused)
                return;

            ExitEditMode();
        }

        private void ExitEditMode(bool commit = true)
        {
            if (!_isEditing)
                return;

            _exitEditScheduled = false;
            string next = commit ? _editor._textBox.Text : _markdown;
            bool changed = commit && !string.Equals(next, _markdown, StringComparison.Ordinal);

            _isEditing = false;
            ShowPreviewSurface();
            _statusLabel.Text = "Preview — double-click to edit. Copy uses plain text.";

            if (!commit)
                return;

            _markdown = next;
            RefreshPreview();

            if (changed)
                MarkdownChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
