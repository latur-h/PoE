using PoE.dlls.GameData;
using PoE.dlls.Logger;
using PoE.dlls.Settings;
using PoE.dlls.Style;

namespace PoE
{
    public partial class Main
    {
        private FlatGroupBox groupBox_GameData = null!;
        private Label label_GameFolder = null!;
        private FlatTextBox textBox_GameFolder = null!;
        private Button button_BrowseGameFolder = null!;
        private Button button_RefreshModCache = null!;
        private Label label_ModCacheStatus = null!;

        private void InitializeGameDataSettingsUi()
        {
            groupBox_GameData = new FlatGroupBox
            {
                Text = "Game data",
                Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point),
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
            };

            label_GameFolder = new Label
            {
                AutoSize = true,
                Text = "Game folder:",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
            };

            textBox_GameFolder = new FlatTextBox
            {
                Size = new Size(420, 30),
                ReadOnly = false,
            };
            textBox_GameFolder._textBox.Text = _settings.GameData.GameFolderPath;
            textBox_GameFolder._textBox.Leave += (_, _) =>
            {
                _settings.GameData.GameFolderPath = textBox_GameFolder._textBox.Text.Trim();
                _userSettings.SaveSettings();
                UpdateModCacheStatusLabel();
            };

            button_BrowseGameFolder = new Button
            {
                Text = "Browse…",
                FlatStyle = FlatStyle.Flat,
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
                Size = new Size(90, 30),
                Cursor = Cursors.Hand,
            };
            button_BrowseGameFolder.FlatAppearance.BorderColor = StaticColors.ForeGround;
            button_BrowseGameFolder.Click += (_, _) => BrowseGameFolder();

            button_RefreshModCache = new Button
            {
                Text = "Refresh mod list",
                FlatStyle = FlatStyle.Flat,
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
                Size = new Size(140, 30),
                Cursor = Cursors.Hand,
            };
            button_RefreshModCache.FlatAppearance.BorderColor = StaticColors.ForeGround;
            button_RefreshModCache.Click += (_, _) => RefreshModCache();

            label_ModCacheStatus = new Label
            {
                AutoSize = false,
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Height = 42,
            };

            groupBox_GameData.Controls.Add(label_GameFolder);
            groupBox_GameData.Controls.Add(textBox_GameFolder);
            groupBox_GameData.Controls.Add(button_BrowseGameFolder);
            groupBox_GameData.Controls.Add(button_RefreshModCache);
            groupBox_GameData.Controls.Add(label_ModCacheStatus);

            tabPage_Settings.Controls.Add(groupBox_GameData);
            UpdateModCacheStatusLabel();
        }

        private int LayoutGameDataSettingsGroup(int sectionWidth)
        {
            const int rowGap = 8;
            const int innerPad = 12;

            int innerWidth = Math.Max(120, sectionWidth - innerPad * 2);
            int y = innerPad + 8;

            label_GameFolder.Location = new Point(innerPad, y + 4);
            textBox_GameFolder.Location = new Point(innerPad + 110, y);
            textBox_GameFolder.Width = Math.Max(120, innerWidth - 110 - button_BrowseGameFolder.Width - 8);
            button_BrowseGameFolder.Location = new Point(textBox_GameFolder.Right + 8, y);

            y += textBox_GameFolder.Height + rowGap;
            button_RefreshModCache.Location = new Point(innerPad + 110, y);

            y += button_RefreshModCache.Height + rowGap;
            label_ModCacheStatus.Location = new Point(innerPad, y);
            label_ModCacheStatus.Width = innerWidth;
            label_ModCacheStatus.Height = Math.Max(
                42,
                TextRenderer.MeasureText(
                    label_ModCacheStatus.Text,
                    label_ModCacheStatus.Font,
                    new Size(innerWidth, int.MaxValue),
                    TextFormatFlags.WordBreak).Height);

            return y + label_ModCacheStatus.Height + innerPad;
        }

        private void BrowseGameFolder()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select your Path of Exile installation folder (contains Content.ggpk)",
                UseDescriptionForTitle = true,
                SelectedPath = Directory.Exists(_settings.GameData.GameFolderPath)
                    ? _settings.GameData.GameFolderPath
                    : string.Empty,
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            _settings.GameData.GameFolderPath = dialog.SelectedPath;
            textBox_GameFolder._textBox.Text = dialog.SelectedPath;
            _userSettings.SaveSettings();
            UpdateModCacheStatusLabel();
        }

        private void RefreshModCache()
        {
            if (_gameDataRefresh.IsRefreshRunning)
            {
                AppLog.System(LogSeverity.Warn, "[Game data] Refresh already in progress.");
                return;
            }

            string folder = textBox_GameFolder._textBox.Text.Trim();
            _settings.GameData.GameFolderPath = folder;

            SetModCacheRefreshUi(running: true);

            Task.Run(() =>
            {
                try
                {
                    return _gameDataRefresh.Refresh(folder);
                }
                catch (Exception ex)
                {
                    return GameDataRefreshResult.Fail(ex.Message);
                }
            }).ContinueWith(task =>
            {
                if (IsDisposed || !IsHandleCreated)
                    return;

                BeginInvoke(CompleteModCacheRefresh, task);
            }, TaskScheduler.Default);
        }

        private void CompleteModCacheRefresh(Task<GameDataRefreshResult> task)
        {
            try
            {
                GameDataRefreshResult result = task.Status switch
                {
                    TaskStatus.RanToCompletion => task.Result,
                    TaskStatus.Faulted => GameDataRefreshResult.Fail(
                        task.Exception?.GetBaseException().Message ?? "Mod cache refresh failed."),
                    TaskStatus.Canceled => GameDataRefreshResult.Fail("Mod cache refresh was canceled."),
                    _ => GameDataRefreshResult.Fail("Mod cache refresh did not complete."),
                };

                if (result.Success)
                {
                    _settings.GameData.ModCacheRefreshedUtc = DateTime.UtcNow;
                    _settings.GameData.ModCacheEntryCount = result.EntryCount;
                }
                else
                {
                    AppLog.System(LogSeverity.Error, $"[Game data] {result.Message}");
                }

                _userSettings.SaveSettings();
                UpdateModCacheStatusLabel(result.Message);
            }
            finally
            {
                SetModCacheRefreshUi(running: false);
            }
        }

        private void SetModCacheRefreshUi(bool running)
        {
            button_RefreshModCache.Enabled = !running;
            button_BrowseGameFolder.Enabled = !running;
            textBox_GameFolder.ReadOnly = running;
            button_RefreshModCache.Text = running ? "Refreshing…" : "Refresh mod list";

            if (running)
                label_ModCacheStatus.Text = "Refreshing mod cache… see Logs tab for progress.";
        }

        private void UpdateModCacheStatusLabel(string? lastMessage = null)
        {
            if (label_ModCacheStatus is null)
                return;

            if (!string.IsNullOrWhiteSpace(lastMessage))
            {
                label_ModCacheStatus.Text = lastMessage;
            }
            else if (_modSuggestions.IsReady)
            {
                int count = _settings.GameData.ModCacheEntryCount > 0
                    ? _settings.GameData.ModCacheEntryCount
                    : _modCacheDatabase.Count();

                string refreshed = _settings.GameData.ModCacheRefreshedUtc is DateTime utc
                    ? $"Last refresh: {utc.ToLocalTime():g}"
                    : "Cached locally";

                string mapHint = _modCacheDatabase.HasMapTaggedEntries()
                    ? "Map modes use map-only suggestions (name + description, inserts mod name)."
                    : "Refresh mod list again to populate map modifier tags for Map / Map Exalt / Map T17 autocomplete.";

                label_ModCacheStatus.Text = $"{count} modifier names/descriptions available. {refreshed}. {mapHint}";
            }
            else if (string.IsNullOrWhiteSpace(_settings.GameData.GameFolderPath))
            {
                label_ModCacheStatus.Text = "Set the game folder and click Refresh mod list to enable rule autocomplete.";
            }
            else
            {
                label_ModCacheStatus.Text = "No cached mod list yet. Click Refresh mod list to read game files.";
            }

            if (IsHandleCreated)
                LayoutSettingsTab();
        }
    }
}
