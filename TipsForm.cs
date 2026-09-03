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
        // Clamp before centring, so the window is centred at the size it will keep.
        ClampToWorkingArea();
        CenterToScreen();
        FitScreenshotToItsAspect();
        BuildExplanation();

        // Reflect the stored choice, so reopening from About shows the real state
        // rather than always starting unticked.
        doNotShowAgainCheckBox.Checked = !SettingsManager.ShouldShowTips();
    }

    /// <summary>
    /// Keeps the window inside the screen's working area.
    ///
    /// The layout scales with the font, so at high display scaling it grows well beyond
    /// its design size. Measured against a 1080p working area it outgrows the screen at
    /// 225 percent, and a 768p laptop at 150 percent. This dialog has a fixed border and
    /// cannot be resized by the user, so without this the buttons would sit off screen
    /// with no way to reach them.
    ///
    /// Once clamped, the TableLayoutPanel compresses its rows to fit rather than
    /// overflowing, which keeps the buttons on screen on its own. Measured down to a
    /// 220px tall window with the buttons still visible, so mainPanel.AutoScroll is
    /// only a backstop and did not engage at any size tested. The explanation keeps its
    /// own vertical scroll bar, which is what recovers the text when that row is
    /// squeezed.
    /// </summary>
    private void ClampToWorkingArea() {
        var workingArea = Screen.FromControl(this).WorkingArea;

        // A minimum bigger than the screen would win over the clamp and put the buttons
        // back off screen, so give way on it first.
        if (MinimumSize.Width > workingArea.Width || MinimumSize.Height > workingArea.Height) {
            MinimumSize = new Size(
                Math.Min(MinimumSize.Width, workingArea.Width),
                Math.Min(MinimumSize.Height, workingArea.Height));
        }

        int width = Math.Min(Width, workingArea.Width);
        int height = Math.Min(Height, workingArea.Height);

        if (width != Width || height != Height) {
            Size = new Size(width, height);
        }
    }

    protected override void OnResize(EventArgs e) {
        base.OnResize(e);

        // The row height changes as the window is resized, so the screenshot has to be
        // re-fitted or it goes back to being centred inside a box of the wrong shape.
        FitScreenshotToItsAspect();
    }

    /// <summary>
    /// Fills in the explanation, with each setting name in bold and everything else,
    /// including the "(since ...)" marker, in the normal weight.
    /// </summary>
    private void BuildExplanation() {
        var regular = explanationText.Font;
        var bold = new Font(regular, FontStyle.Bold);

        explanationText.Clear();

        Append("Auto-position Secondary Zoom Window", bold);
        Append(" (since 1.5.1)\n", regular);
        Append("The secondary Zoom window is moved to the same monitor where "
             + "JW Library's secondary window is displayed.\n\n", regular);

        Append("Auto-hide New Secondary Zoom Window", bold);
        Append(" (since 1.6.0)\n", regular);
        Append("Zoom rebuilds its meeting windows every time a meeting starts, a "
             + "connection recovers, or Zoom restarts, and each rebuild puts the "
             + "secondary window back on the shared screen. It is now hidden "
             + "automatically, and JW Library is brought back at the same time, "
             + "exactly as tapping F9 would do. A window you bring up yourself with "
             + "F10 stays up.\n\n", regular);

        Append("Both options are enabled by default. Use the Options submenu on the "
             + "tray icon to change them.", regular);

        // Leave the caret at the start with nothing selected, so no highlight shows.
        explanationText.SelectionStart = 0;
        explanationText.SelectionLength = 0;
    }

    /// <summary>
    /// Appends a run of text in the given font.
    /// </summary>
    private void Append(string text, Font font) {
        explanationText.SelectionStart = explanationText.TextLength;
        explanationText.SelectionLength = 0;
        explanationText.SelectionFont = font;
        explanationText.AppendText(text);
    }

    /// <summary>
    /// Narrows the screenshot box to the image's own aspect ratio at whatever height
    /// the layout ended up with, so the image sits flush against the left edge.
    ///
    /// A fixed design time size is not enough. The form scales by font, and that scales
    /// width and height by different factors, which leaves the box a slightly different
    /// shape than the image. PictureBoxSizeMode.Zoom then centres the image inside the
    /// leftover space instead of starting at the left edge.
    /// </summary>
    private bool adjustingScreenshot;

    private void FitScreenshotToItsAspect() {
        // Setting the width re-runs layout, which can call back in here.
        if (adjustingScreenshot) {
            return;
        }

        var image = screenshotPictureBox.Image;
        if (image == null || image.Height <= 0) {
            return;
        }

        int height = screenshotPictureBox.Height;
        if (height <= 0) {
            return;
        }

        int width = (int)Math.Round(height * (double)image.Width / image.Height);
        if (width == screenshotPictureBox.Width) {
            return;
        }

        adjustingScreenshot = true;
        try {
            screenshotPictureBox.Width = width;
        } finally {
            adjustingScreenshot = false;
        }
    }

    private void okButton_Click(object sender, EventArgs e) {
        // A two way toggle. Ticking records the Tips revision this build ships;
        // unticking clears it, so the window returns on the next start.
        if (doNotShowAgainCheckBox.Checked) {
            SettingsManager.DismissCurrentTips();
        } else {
            SettingsManager.RestoreTips();
        }
        this.Close();
    }
}
