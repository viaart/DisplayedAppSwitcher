namespace DisplayedAppSwitcher;

/// <summary>
/// Form that displays tips about new features to the user.
/// Shows on first launch or when a new version is installed.
/// </summary>
public partial class TipsForm : Form {

    public TipsForm() {
        InitializeComponent();
        this.Text = $"Tips - Displayed App Switcher v{SettingsManager.CurrentVersion}";
    }

    protected override void OnLoad(EventArgs e) {
        base.OnLoad(e);
        CenterToScreen();
    }

    private void okButton_Click(object sender, EventArgs e) {
        // If "Do not show again" is checked, save the current version
        if (doNotShowAgainCheckBox.Checked) {
            SettingsManager.DismissTipsForCurrentVersion();
        }
        this.Close();
    }
}
