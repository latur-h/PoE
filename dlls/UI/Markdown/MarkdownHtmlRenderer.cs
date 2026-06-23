using Markdig;
using PoE.dlls.Style;

namespace PoE.dlls.UI.Markdown
{
    public static class MarkdownHtmlRenderer
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        public static string ToPreviewDocument(string? markdown)
        {
            string source = markdown ?? string.Empty;
            if (string.IsNullOrWhiteSpace(source))
                return BuildDocument("""<p class="empty-hint">Double-click to edit this note.</p>""");

            string withCopyChips = MarkdownCopyChipPreprocessor.ExpandCopyChips(source);
            string body = Markdig.Markdown.ToHtml(withCopyChips, Pipeline);
            return BuildDocument(body);
        }

        private static string BuildDocument(string bodyHtml)
        {
            Color background = StaticColors.BackGround;
            Color foreground = StaticColors.ForeGround;
            Color heading = StaticColors.TabControlSelectedForeGround;
            Color link = StaticColors.Underline;
            string bg = ColorToHex(background);
            string fg = ColorToHex(foreground);
            string hd = ColorToHex(heading);
            string ln = ColorToHex(link);

            return $$"""
                <!DOCTYPE html>
                <html>
                <head>
                <meta charset="utf-8" />
                <style>
                html, body {
                  margin: 0;
                  padding: 0;
                  background: {{bg}};
                  color: {{fg}};
                  font-family: 'Segoe UI', sans-serif;
                  font-size: 14px;
                  line-height: 1.5;
                }
                body { padding: 12px 14px; }
                h1, h2, h3, h4 { color: {{hd}}; margin: 0.8em 0 0.35em; }
                h1 { font-size: 1.45em; }
                h2 { font-size: 1.25em; }
                h3 { font-size: 1.1em; }
                p { margin: 0.45em 0; }
                ul, ol { margin: 0.45em 0; padding-left: 1.4em; }
                li { margin: 0.15em 0; }
                code {
                  background: #2a2a2a;
                  padding: 1px 5px;
                  border-radius: 3px;
                  font-family: Consolas, 'Courier New', monospace;
                  font-size: 0.95em;
                }
                pre {
                  background: #2a2a2a;
                  padding: 10px 12px;
                  border-radius: 4px;
                  overflow-x: auto;
                }
                pre code { background: transparent; padding: 0; }
                a { color: {{ln}}; }
                blockquote {
                  border-left: 3px solid {{fg}};
                  margin: 0.5em 0;
                  padding: 0.1em 0 0.1em 12px;
                  opacity: 0.92;
                }
                hr { border: 0; border-top: 1px solid #444; margin: 0.8em 0; }
                table { border-collapse: collapse; margin: 0.5em 0; }
                th, td { border: 1px solid #444; padding: 4px 8px; }
                .empty-hint { opacity: 0.65; font-style: italic; }
                .copy-chip {
                  display: inline-block;
                  background: #2a2a2a;
                  border: 1px solid #555;
                  border-radius: 4px;
                  padding: 0 7px;
                  margin: 0 1px;
                  font-family: Consolas, 'Courier New', monospace;
                  font-size: 0.95em;
                  color: {{fg}};
                  cursor: pointer;
                  user-select: none;
                  vertical-align: baseline;
                  line-height: 1.45;
                }
                .copy-chip:hover {
                  border-color: {{ln}};
                  background: #333;
                }
                .copy-chip.copied {
                  border-color: {{fg}};
                }
                .copy-chip .copy-icon {
                  opacity: 0.55;
                  font-size: 0.85em;
                  margin-left: 4px;
                }
                </style>
                </head>
                <body>
                {{bodyHtml}}
                <script>
                document.addEventListener('copy', function(e) {
                  const sel = window.getSelection();
                  if (!sel || sel.isCollapsed) return;
                  if (sel.anchorNode && sel.anchorNode.parentElement && sel.anchorNode.parentElement.closest('.copy-chip'))
                    return;
                  e.preventDefault();
                  e.clipboardData.setData('text/plain', sel.toString());
                });
                document.body.addEventListener('click', function(e) {
                  const chip = e.target.closest('.copy-chip');
                  if (!chip) return;
                  e.preventDefault();
                  e.stopPropagation();
                  const text = chip.getAttribute('data-copy') || chip.textContent || '';
                  if (window.chrome && window.chrome.webview)
                    window.chrome.webview.postMessage(JSON.stringify({ type: 'copy', text: text }));
                  chip.classList.add('copied');
                  const icon = chip.querySelector('.copy-icon');
                  if (icon) icon.textContent = '✓';
                  window.setTimeout(function() {
                    chip.classList.remove('copied');
                    if (icon) icon.textContent = '⧉';
                  }, 900);
                });
                document.body.addEventListener('dblclick', function(e) {
                  if (e.target.closest('.copy-chip')) {
                    e.preventDefault();
                    e.stopPropagation();
                    return;
                  }
                  if (window.chrome && window.chrome.webview)
                    window.chrome.webview.postMessage('edit');
                });
                </script>
                </body>
                </html>
                """;
        }

        private static string ColorToHex(Color color) =>
            $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
