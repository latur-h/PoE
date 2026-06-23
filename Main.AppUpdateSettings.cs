using PoE.dlls.Updates;
using PoE.dlls.Style;

namespace PoE
{
    public partial class Main
    {
        private FlatGroupBox groupBox_Update = null!;
        private Label label_UpdateCurrentVersion = null!;
        private Label label_UpdateStatus = null!;
        private Button button_CheckForUpdates = null!;
        private Button button_InstallUpdate = null!;

        private CancellationTokenSource? _updateCancellation;
        private GitHubReleaseInfo? _pendingUpdate;
        private bool _updateOperationInProgress;

        private void InitializeAppUpdateSettingsUi()
        {
            groupBox_Update = new FlatGroupBox
            {
                Text = "Update",
                Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point),
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
            };

            label_UpdateCurrentVersion = new Label
            {
                AutoSize = true,
                Text = $"Current version: {AppVersion.CurrentDisplay}",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
            };

            label_UpdateStatus = new Label
            {
                AutoSize = false,
                Text = "Checking GitHub for updates…",
                ForeColor = StaticColors.TabControlForeGround,
                BackColor = StaticColors.BackGround,
            };

            button_CheckForUpdates = new Button
            {
                Size = new Size(140, 30),
                Text = "Check for updates",
                Font = new Font("Segoe UI", 10F),
                ForeColor = StaticColors.ButtonForeGround,
                UseVisualStyleBackColor = true,
            };
            button_CheckForUpdates.Click += (_, _) => _ = CheckForUpdatesAsync(silent: false);

            button_InstallUpdate = new Button
            {
                Size = new Size(140, 30),
                Text = "Install update",
                Font = new Font("Segoe UI", 10F),
                ForeColor = StaticColors.ButtonForeGround,
                UseVisualStyleBackColor = true,
                Enabled = false,
                Visible = false,
            };
            button_InstallUpdate.Click += (_, _) => _ = InstallPendingUpdateAsync();

            groupBox_Update.Controls.Add(label_UpdateCurrentVersion);
            groupBox_Update.Controls.Add(label_UpdateStatus);
            groupBox_Update.Controls.Add(button_CheckForUpdates);
            groupBox_Update.Controls.Add(button_InstallUpdate);

            tabPage_Settings.Controls.Add(groupBox_Update);
        }

        private void LayoutAppUpdateSettingsGroup()
        {
            if (groupBox_Update is null)
                return;

            const int innerPad = 12;

            label_UpdateCurrentVersion.Location = new Point(innerPad, innerPad + 8);
            label_UpdateStatus.Location = new Point(innerPad, innerPad + 34);
            label_UpdateStatus.Width = Math.Max(160, groupBox_Update.Width - innerPad * 2 - 300);
            label_UpdateStatus.Height = 22;

            int buttonTop = innerPad + 30;
            if (button_InstallUpdate.Visible)
            {
                button_InstallUpdate.Location = new Point(groupBox_Update.Width - innerPad - button_InstallUpdate.Width, buttonTop);
                button_CheckForUpdates.Location = new Point(button_InstallUpdate.Left - button_CheckForUpdates.Width - 8, buttonTop);
            }
            else
            {
                button_CheckForUpdates.Location = new Point(groupBox_Update.Width - innerPad - button_CheckForUpdates.Width, buttonTop);
            }
        }

        private void SetupAppUpdateHints()
        {
            toolTip_Settings.SetToolTip(
                groupBox_Update,
                "Checks GitHub releases and installs the latest win-x64 build into the folder where PoE.exe is running.");
            toolTip_Settings.SetToolTip(
                button_CheckForUpdates,
                "Check https://github.com/latur-h/PoE/releases for a newer version.");
            toolTip_Settings.SetToolTip(
                button_InstallUpdate,
                "Download the latest release zip and replace files in the current install folder, then restart the app.");
        }

        private void BeginStartupUpdateCheck()
        {
            _ = CheckForUpdatesAsync(silent: true);
        }

        private async Task CheckForUpdatesAsync(bool silent)
        {
            if (_updateOperationInProgress)
                return;

            CancelUpdateOperation();
            _updateCancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = _updateCancellation.Token;

            SetUpdateUiBusy(true, silent ? "Checking GitHub for updates…" : "Checking for updates…");

            try
            {
                GitHubReleaseInfo? latest = await GitHubReleaseClient.GetLatestReleaseAsync(cancellationToken).ConfigureAwait(true);
                if (latest is null)
                {
                    ApplyUpdateStatus("Could not read the latest GitHub release.", showInstall: false);
                    if (!silent)
                        MessageBox.Show(this, "Could not read the latest GitHub release.", "Update check", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                if (AppVersion.IsNewerThanCurrent(latest.Version))
                {
                    _pendingUpdate = latest;
                    ApplyUpdateStatus($"Update available: v{latest.Version} (latest on GitHub).", showInstall: true);
                    if (!silent)
                    {
                        MessageBox.Show(
                            this,
                            $"Version v{latest.Version} is available.\r\n\r\nClick Install update to download and replace files in:\r\n{AppUpdater.GetInstallDirectory()}",
                            "Update available",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }

                    return;
                }

                _pendingUpdate = null;
                ApplyUpdateStatus($"Up to date (latest release: v{latest.Version}).", showInstall: false);
                if (!silent)
                {
                    MessageBox.Show(
                        this,
                        $"You are running v{AppVersion.CurrentDisplay}. That matches the latest GitHub release.",
                        "Update check",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                ApplyUpdateStatus("Update check failed.", showInstall: false);
                if (!silent)
                {
                    MessageBox.Show(
                        this,
                        $"Could not check for updates.\r\n\r\n{ex.Message}",
                        "Update check",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            finally
            {
                SetUpdateUiBusy(false, null);
            }
        }

        private async Task InstallPendingUpdateAsync()
        {
            if (_pendingUpdate is null || _updateOperationInProgress)
                return;

            GitHubReleaseInfo release = _pendingUpdate;
            DialogResult confirm = MessageBox.Show(
                this,
                $"Install v{release.Version} into:\r\n{AppUpdater.GetInstallDirectory()}\r\n\r\nThe app will close and restart automatically.",
                "Install update",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.OK)
                return;

            CancelUpdateOperation();
            _updateCancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = _updateCancellation.Token;

            SetUpdateUiBusy(true, $"Downloading v{release.Version}…");

            try
            {
                string zipPath = Path.Combine(Path.GetTempPath(), release.AssetName);
                var progress = new Progress<long>(percent =>
                {
                    if (percent is > 0 and <= 100)
                        ApplyUpdateStatus($"Downloading v{release.Version}… {percent}%", showInstall: false);
                });

                await AppUpdater.DownloadReleaseAsync(release, zipPath, progress, cancellationToken).ConfigureAwait(true);

                ApplyUpdateStatus($"Installing v{release.Version}…", showInstall: false);
                AppUpdater.ScheduleApplyAndRestart(
                    zipPath,
                    AppUpdater.GetInstallDirectory(),
                    AppUpdater.GetExecutablePath(),
                    Environment.ProcessId);

                Application.Exit();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                ApplyUpdateStatus("Update failed.", showInstall: _pendingUpdate is not null);
                MessageBox.Show(
                    this,
                    $"Could not install the update.\r\n\r\n{ex.Message}",
                    "Update failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetUpdateUiBusy(false, null);
            }
        }

        private void ApplyUpdateStatus(string status, bool showInstall)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => ApplyUpdateStatus(status, showInstall));
                return;
            }

            label_UpdateStatus.Text = status;
            button_InstallUpdate.Visible = showInstall;
            button_InstallUpdate.Enabled = showInstall && !_updateOperationInProgress;
            if (showInstall && _pendingUpdate is not null)
                button_InstallUpdate.Text = $"Install v{_pendingUpdate.Version}";

            LayoutAppUpdateSettingsGroup();
        }

        private void SetUpdateUiBusy(bool busy, string? status)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => SetUpdateUiBusy(busy, status));
                return;
            }

            _updateOperationInProgress = busy;
            button_CheckForUpdates.Enabled = !busy;
            button_InstallUpdate.Enabled = !busy && _pendingUpdate is not null;

            if (!string.IsNullOrWhiteSpace(status))
                label_UpdateStatus.Text = status;
        }

        private void CancelUpdateOperation()
        {
            _updateCancellation?.Cancel();
            _updateCancellation?.Dispose();
            _updateCancellation = null;
        }
    }
}
