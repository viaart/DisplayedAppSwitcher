using Windows.Win32;

namespace DisplayedAppSwitcher.Services;

/// <summary>
/// Detects a newly appeared secondary Zoom meeting window and reports it once.
///
/// This class only detects. Acting on the detection is left to the caller, which
/// runs the same switch to JW Library that F9 performs. Hiding the Zoom window on
/// its own is not enough: switching to Zoom minimises the JW Library secondary
/// window, so hiding Zoom without restoring JW Library would leave the shared
/// monitor showing the desktop.
///
/// Zoom destroys and recreates its meeting windows on every meeting start, network
/// reconnect and crash recovery, so "a new window appeared" is the reliable signal.
/// Watching the process list is not: the window owning process is one of three
/// Zoom.exe processes, it is recycled at meeting END (when the windows are already
/// hidden and there is nothing to do) and it does NOT change at meeting START
/// (when the window becomes visible and hiding is actually wanted).
/// </summary>
public sealed class ZoomSecondaryWindowWatcher : IDisposable {

  // Both the meeting control window and the secondary window share this class.
  private const string ZoomWindowClassName = "ConfMultiTabContentWndClass";

  // Only windows carrying the meeting title are safe to act on. While Zoom is
  // bringing a meeting up there is a brief period (measured at 385ms to 447ms,
  // present in 3 of 5 observed meeting starts) where both windows are already
  // visible but still titled "Zoom Workplace" and neither has grown its control
  // panel yet. Acting during that window hides the meeting control window too.
  private const string ZoomMeetingWindowTitle = "Zoom Meeting";

  // The meeting control window owns a control panel; the secondary window does not.
  private const string ZoomControlPanelClassName = "ZPControlPanelClass";
  private const int ControlPanelSearchDepth = 3;

  private readonly System.Windows.Forms.Timer timer;

  // The last window this watcher reported. Anything else that is visible is treated
  // as new. Keeping this handle is what stops the watcher from fighting the user:
  // when a hotkey shows the secondary window again it is the same handle, so it is
  // left alone.
  private IntPtr lastHandledWindow = IntPtr.Zero;

  private bool disposed;

  /// <summary>
  /// Raised when a secondary Zoom meeting window that has not been handled yet
  /// becomes visible.
  /// </summary>
  public event EventHandler? NewSecondaryWindowDetected;

  public ZoomSecondaryWindowWatcher(int pollIntervalMilliseconds = 500) {
    timer = new System.Windows.Forms.Timer {
      Interval = pollIntervalMilliseconds
    };
    timer.Tick += OnTick;
  }

  /// <summary>
  /// Starts watching. Any secondary window that is already visible counts as new,
  /// which is what handles the case of the application starting during a meeting.
  /// </summary>
  public void Start() {
    timer.Start();
  }

  public void Stop() {
    timer.Stop();
  }

  /// <summary>
  /// Treats the currently visible secondary window as already handled.
  /// Called when the user switches the option on so that a window they are
  /// looking at right now does not disappear from under them.
  /// </summary>
  public void SeedFromCurrentState() {
    lastHandledWindow = FindVisibleSecondaryMeetingWindow();
  }

  private void OnTick(object? sender, EventArgs e) {
    try {
      if (!SettingsManager.GetAutoHideZoomSecondaryWindowEnabled()) {
        return;
      }

      // Forget the remembered window once it is gone, so the next window Zoom
      // creates is recognised as new. Without this the feature would fire once
      // and then stay silent for the rest of the session.
      if (lastHandledWindow != IntPtr.Zero &&
          !PInvoke.IsWindow(new Windows.Win32.Foundation.HWND(lastHandledWindow))) {
        lastHandledWindow = IntPtr.Zero;
      }

      var hWnd = FindVisibleSecondaryMeetingWindow();
      if (hWnd == IntPtr.Zero || hWnd == lastHandledWindow) {
        return;
      }

      // Record before raising, so a slow handler cannot cause a second report of
      // the same window on the next tick.
      lastHandledWindow = hWnd;

      System.Diagnostics.Debug.WriteLine($"New secondary Zoom window detected 0x{hWnd:X}");
      NewSecondaryWindowDetected?.Invoke(this, EventArgs.Empty);

    } catch (Exception ex) {
      // Never let a scan failure take the tray application down.
      System.Diagnostics.Debug.WriteLine($"Error in ZoomSecondaryWindowWatcher: {ex.Message}");
    }
  }

  /// <summary>
  /// Finds the visible secondary Zoom meeting window, or IntPtr.Zero if there is none.
  ///
  /// This deliberately does not reuse ZoomWorkspaceSwitcher.IsTheRightWindow. That
  /// method also matches "Zoom Workplace" windows, and with no meeting running it
  /// reports BOTH of them as the secondary window. That ambiguity is harmless when
  /// switching (the windows are hidden anyway) but would trigger on the wrong window
  /// here. Waiting for the meeting title also means that by the time the switch runs,
  /// the ambiguity has resolved and the switch itself matches only one window.
  /// </summary>
  private static IntPtr FindVisibleSecondaryMeetingWindow() {
    var zeroHWND = new Windows.Win32.Foundation.HWND(IntPtr.Zero);
    var hWnd = IntPtr.Zero;

    while (true) {
      hWnd = PInvoke.FindWindowEx(zeroHWND, new Windows.Win32.Foundation.HWND(hWnd),
        ZoomWindowClassName, null as string);
      if (hWnd == IntPtr.Zero) {
        return IntPtr.Zero;
      }

      if (!PInvoke.IsWindowVisible(new Windows.Win32.Foundation.HWND(hWnd))) {
        continue;
      }

      var info = Win32Helpers.GetWindowInfo(hWnd);
      if (info.title != ZoomMeetingWindowTitle) {
        continue;
      }

      // The secondary window is the one WITHOUT a control panel.
      if (Win32Helpers.FindChildWindowRecursive(hWnd, ZoomControlPanelClassName, null,
            ControlPanelSearchDepth)) {
        continue;
      }

      return hWnd;
    }
  }

  public void Dispose() {
    if (disposed) {
      return;
    }
    disposed = true;
    timer.Tick -= OnTick;
    timer.Stop();
    timer.Dispose();
  }
}
