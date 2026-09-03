using System.Drawing.Drawing2D;

namespace DisplayedAppSwitcher;

/// <summary>
/// Small self-dismissing popup used to confirm an automatic action.
///
/// A tray balloon is not used on purpose. NotifyIcon.ShowBalloonTip routes through
/// the Windows toast system on Windows 10 and 11, which ignores the requested
/// timeout and keeps a copy in the notification centre.
///
/// The popup never appears on the monitor showing the JW Library secondary window,
/// because that is the monitor being shared with the audience.
/// </summary>
public sealed class AutoHideNotificationForm : Form {

  private const int WS_EX_NOACTIVATE = 0x08000000;
  private const int WS_EX_TOOLWINDOW = 0x00000080;

  private const int ScreenMargin = 24;
  private const int HorizontalPadding = 22;
  private const int VerticalPadding = 16;
  private const int CornerRadius = 10;

  private static AutoHideNotificationForm? current;

  private readonly System.Windows.Forms.Timer closeTimer;
  private readonly string message;

  private AutoHideNotificationForm(string message, int durationMilliseconds) {
    this.message = message;

    FormBorderStyle = FormBorderStyle.None;
    ShowInTaskbar = false;
    TopMost = true;
    StartPosition = FormStartPosition.Manual;
    BackColor = Color.FromArgb(32, 32, 32);
    ForeColor = Color.White;
    Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
    DoubleBuffered = true;

    Size = MeasureSize(message);
    Location = ChooseLocation(Size);

    closeTimer = new System.Windows.Forms.Timer { Interval = durationMilliseconds };
    closeTimer.Tick += (_, _) => Close();
  }

  /// <summary>
  /// Shows the notification. Any notification still on screen is replaced.
  /// </summary>
  /// <param name="message">Text to display</param>
  /// <param name="durationMilliseconds">How long the popup stays visible</param>
  public static void ShowNotification(string message, int durationMilliseconds = 2000) {
    try {
      current?.Close();

      var form = new AutoHideNotificationForm(message, durationMilliseconds);
      current = form;
      form.FormClosed += (_, _) => {
        if (ReferenceEquals(current, form)) {
          current = null;
        }
        form.Dispose();
      };
      form.Show();
    } catch (Exception ex) {
      // A missing notification must never break the action it was reporting.
      System.Diagnostics.Debug.WriteLine($"Error showing notification: {ex.Message}");
    }
  }

  /// <summary>
  /// Keeps the popup from taking focus away from whatever the user is doing.
  /// </summary>
  protected override bool ShowWithoutActivation => true;

  protected override CreateParams CreateParams {
    get {
      var cp = base.CreateParams;
      cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
      return cp;
    }
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
    ApplyRoundedCorners();
    closeTimer.Start();
  }

  protected override void OnPaint(PaintEventArgs e) {
    base.OnPaint(e);
    e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
    TextRenderer.DrawText(e.Graphics, message, Font, ClientRectangle, ForeColor,
      TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
  }

  protected override void OnFormClosed(FormClosedEventArgs e) {
    closeTimer.Stop();
    closeTimer.Dispose();
    base.OnFormClosed(e);
  }

  private void ApplyRoundedCorners() {
    using var path = new GraphicsPath();
    int d = CornerRadius * 2;
    path.AddArc(0, 0, d, d, 180, 90);
    path.AddArc(Width - d - 1, 0, d, d, 270, 90);
    path.AddArc(Width - d - 1, Height - d - 1, d, d, 0, 90);
    path.AddArc(0, Height - d - 1, d, d, 90, 90);
    path.CloseFigure();
    Region = new Region(path);
  }

  private static Size MeasureSize(string message) {
    using var font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
    var textSize = TextRenderer.MeasureText(message, font);
    return new Size(textSize.Width + (HorizontalPadding * 2),
                    textSize.Height + (VerticalPadding * 2));
  }

  /// <summary>
  /// Picks the bottom right corner of a monitor that is not being shared.
  /// </summary>
  private static Point ChooseLocation(Size size) {
    var screen = ChooseScreen();
    var area = screen.WorkingArea;
    return new Point(area.Right - size.Width - ScreenMargin,
                     area.Bottom - size.Height - ScreenMargin);
  }

  private static Screen ChooseScreen() {
    var primary = Screen.PrimaryScreen ?? Screen.AllScreens[0];

    // The shared monitor is the one holding the JW Library secondary window, so
    // the notification must not land there. It carries no taskbar, which means
    // the primary screen is normally the correct choice already.
    var sharedMonitor = TargetMonitorTracker.JWLibraryTargetMonitor;
    if (sharedMonitor == null || primary.Bounds != sharedMonitor.Bounds) {
      return primary;
    }

    foreach (var screen in Screen.AllScreens) {
      if (screen.Bounds != sharedMonitor.Bounds) {
        return screen;
      }
    }
    return primary;
  }
}
