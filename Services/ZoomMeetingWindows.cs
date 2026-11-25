using Windows.Win32;

namespace DisplayedAppSwitcher.Services;

/// <summary>
/// Finds Zoom's meeting windows.
///
/// For the duration of a meeting Zoom has two top level windows of the same class, both
/// titled "Zoom Meeting": the meeting control window, which owns a control panel, and
/// the secondary (dual monitor) window, which does not. Zoom pre-creates both at launch,
/// hidden and titled "Zoom Workplace", renames them when a meeting starts and destroys
/// them when it ends. The titles survive while the windows are hidden, both for the
/// secondary window hidden by F9 and for the control window that Zoom hides for the
/// whole of a screen share.
/// </summary>
public static class ZoomMeetingWindows {

  public const string ClassName = "ConfMultiTabContentWndClass";

  // Only windows carrying the meeting title are safe to act on. While Zoom is bringing
  // a meeting up there is a brief period (measured at 385ms to 447ms, present in 3 of
  // 5 observed meeting starts) where both windows are already visible but still titled
  // "Zoom Workplace" and neither has grown its control panel yet.
  public const string MeetingTitle = "Zoom Meeting";

  private const string ControlPanelClassName = "ZPControlPanelClass";
  private const int ControlPanelSearchDepth = 3;

  /// <summary>
  /// Whether a Zoom meeting is currently running, visible or not.
  /// </summary>
  public static bool IsMeetingInProgress() {
    return MeetingWindows().Count > 0;
  }

  /// <summary>
  /// The secondary meeting windows: "Zoom Meeting" windows without a control panel, but
  /// only while another "Zoom Meeting" window has one. Empty otherwise.
  ///
  /// The lack of a control panel alone is not enough to tell them apart. The control
  /// window is without its panel for a moment while Zoom builds or rebuilds it, and the
  /// window events this application listens to arrive within a millisecond, well inside
  /// that gap. On a test bench, the panel check alone hid the control window when it was
  /// shown a moment before its panel was added. Requiring a sibling that has the panel
  /// means nothing qualifies while Zoom is midway, so nothing is touched, and the next
  /// event or poll acts once Zoom has finished.
  /// </summary>
  public static List<IntPtr> FindSecondaryWindows() {
    var withoutPanel = new List<IntPtr>();
    var anyWithPanel = false;
    foreach (var hWnd in MeetingWindows()) {
      if (Win32Helpers.FindChildWindowRecursive(hWnd, ControlPanelClassName, null, ControlPanelSearchDepth)) {
        anyWithPanel = true;
      } else {
        withoutPanel.Add(hWnd);
      }
    }
    return anyWithPanel ? withoutPanel : new List<IntPtr>();
  }

  private static List<IntPtr> MeetingWindows() {
    var windows = new List<IntPtr>();
    var zeroHWND = new Windows.Win32.Foundation.HWND(IntPtr.Zero);
    var hWnd = IntPtr.Zero;
    while (true) {
      hWnd = PInvoke.FindWindowEx(zeroHWND, new Windows.Win32.Foundation.HWND(hWnd), ClassName, null as string);
      if (hWnd == IntPtr.Zero) {
        return windows;
      }
      if (Win32Helpers.GetWindowInfo(hWnd).title == MeetingTitle) {
        windows.Add(hWnd);
      }
    }
  }
}
