using DisplayedAppSwitcher.Services;

namespace DisplayedAppSwitcher;

public partial class UpdateForm : Form {
    private readonly string _newVersion;
    private readonly string _currentVersion;
    private CancellationTokenSource? _cts;
    private bool _downloading;

    public UpdateForm(string newVersion, string currentVersion) {
        _newVersion = newVersion;
        _currentVersion = currentVersion;
        InitializeComponent();
        labelVersionInfo.Text = $"Version {_newVersion.TrimStart('v')} is available.\r\nYou are currently running v{_currentVersion}.";
    }

    private void btnRemindNextTime_Click(object? sender, EventArgs e) {
        if (_downloading) return;
        SettingsManager.SetUpdatePostponedUntil(DateTime.MinValue);
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void btnRemind7Days_Click(object? sender, EventArgs e) {
        if (_downloading) return;
        SettingsManager.SetUpdatePostponedUntil(DateTime.UtcNow.AddDays(7));
        DialogResult = DialogResult.Ignore;
        Close();
    }

    private async void btnUpdateNow_Click(object? sender, EventArgs e) {
        if (_downloading) return;
        _downloading = true;
        SetButtonsEnabled(false);
        progressBar.Visible = true;
        labelProgress.Visible = true;
        progressBar.Style = ProgressBarStyle.Marquee;
        labelProgress.Text = "Downloading...";

        _cts = new CancellationTokenSource();

        var progress = new Progress<UpdateProgress>(p => {
            if (p.TotalBytes > 0) {
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Maximum = (int)(p.TotalBytes / 1024);
                progressBar.Value = (int)(p.BytesDownloaded / 1024);
                var downloadedMB = p.BytesDownloaded / (1024.0 * 1024.0);
                var totalMB = p.TotalBytes / (1024.0 * 1024.0);
                labelProgress.Text = $"{downloadedMB:F1} MB / {totalMB:F1} MB";
            } else {
                labelProgress.Text = $"{p.BytesDownloaded / (1024.0 * 1024.0):F1} MB downloaded...";
            }
        });

        var (installerPath, success, errorMessage) = await UpdateService.DownloadAndVerifyInstallerAsync(
            _newVersion, progress, _cts.Token);

        if (!success) {
            _downloading = false;
            SetButtonsEnabled(true);
            progressBar.Visible = false;
            labelProgress.Visible = false;
            MessageBox.Show(errorMessage, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        labelProgress.Text = "Installing update...";
        UpdateService.LaunchUpdaterScript(installerPath);
        Application.Exit();
    }

    protected override void OnFormClosing(FormClosingEventArgs e) {
        if (_downloading && e.CloseReason == CloseReason.UserClosing) {
            e.Cancel = true;
            return;
        }
        _cts?.Cancel();
        _cts?.Dispose();
        base.OnFormClosing(e);
    }

    private void SetButtonsEnabled(bool enabled) {
        btnRemindNextTime.Enabled = enabled;
        btnRemind7Days.Enabled = enabled;
        btnUpdateNow.Enabled = enabled;
    }
}
