using System.Diagnostics;
using Windows.Win32;
using DisplayedAppSwitcher.Services;

namespace DisplayedAppSwitcher;

public class JWLibrarySwitcher : ISwitcher {
  public bool IsTheRightWindow(SwitcherWindowInfo info, Purpose purpose) {
    // JW Library creates several windows for dual monitor.
    //
    // Both windows have the same class "ApplicationFrameWindow".
    //
    // Both windows can be made full screen, and have the WS_EX_TOPMOST flag, supposedly set internally.
    //
    // Detection strategy:
    // 1. If title is "Second Display ‎- JW Library" -> definitely secondary (has LEFT-TO-RIGHT MARK)
    // 2. If title is "JW Library":
    //    a. Check if window is fullscreen on its monitor (secondary display is always fullscreen)
    //    b. Fallback: check if window has no visible title bar
    //
    if (info.title == "Second Display ‎- JW Library") {
      // Second display has a unique name with LEFT-TO-RIGHT MARK unicode symbol
      System.Diagnostics.Debug.WriteLine($"IsTheRightWindow: hWnd=0x{info.hWnd:X} title='{info.title}' -> TRUE (Second Display title)");
      return true;
    }
    if (info.title != "JW Library") {
      return false;
    }

    // For windows with title "JW Library", check both criteria:
    // 1. Window is fullscreen on its monitor (secondary display is always fullscreen)
    // 2. Window has no visible title bar (secondary has no title bar, main does even when maximized)
    bool isFullscreen = IsWindowFullscreenOnMonitor(info.hWnd);
    bool hasVisibleTitleBar = HasVisibleTitleBar(info);

    // Require BOTH: fullscreen AND no visible title bar
    // This filters out maximized main window (which is fullscreen but HAS title bar)
    bool isSecondary = isFullscreen && !hasVisibleTitleBar;

    System.Diagnostics.Debug.WriteLine($"IsTheRightWindow: hWnd=0x{info.hWnd:X} title='{info.title}' isFullscreen={isFullscreen} hasVisibleTitleBar={hasVisibleTitleBar} -> {isSecondary}");
    return isSecondary;
    // if ((info.styleFlags & Win32Constants.WS_CAPTION) > 0 ) {
    //   return false;
    // }
    // return true;

    // if ((info.styleFlags & Win32Constants.WS_POPUP) != 0) {

    // FIXME: JW Library may occasionally starts in full screen mode,
    // possibly when the screen arrangement is swapped (secondary screen to the left).
    // In this case it is becoming harder to discriminate.

    //var toolbarFound = false;
    //var visibleToolbarFound = false;
    //while (true) {
    //  hWnd = PInvoke.FindWindowEx(hWndParent, new Windows.Win32.Foundation.HWND(hWnd), "ApplicationFrameTitleBarWindow", null as string);
    //  if (hWnd != IntPtr.Zero) {
    //    toolbarFound = true;
    //    if (PInvoke.IsWindowVisible(new Windows.Win32.Foundation.HWND(hWnd))) {
    //      visibleToolbarFound = true;
    //      break;
    //    }
    //  } else {
    //    break;
    //  }
    //}
    //if (toolbarFound) return true; // !visibleToolbarFound;
    //return false;
  }

  private bool HasVisibleTitleBar(SwitcherWindowInfo info) {
    Windows.Win32.Foundation.HWND hWndParent = new(info.hWnd);
    var hWnd = IntPtr.Zero;
    while (true) {
      hWnd = PInvoke.FindWindowEx(hWndParent, new Windows.Win32.Foundation.HWND(hWnd), "ApplicationFrameTitleBarWindow", null as string);
      if (hWnd != IntPtr.Zero) {
        if (PInvoke.IsWindowVisible(new Windows.Win32.Foundation.HWND(hWnd))) {
          return true;
        }
      } else {
        break;
      }
    }
    return false;
  }

  /// <summary>
  /// Checks if the window is fullscreen on its monitor (covers the entire monitor).
  /// The secondary display window is always fullscreen, while the main window is usually windowed.
  /// For minimized windows, checks the normal (restored) position.
  /// </summary>
  private bool IsWindowFullscreenOnMonitor(IntPtr hWnd) {
    const int tolerance = 50;
    var h = new Windows.Win32.Foundation.HWND(hWnd);

    // Get window placement to check if minimized and get normal position
    var placement = new Windows.Win32.UI.WindowsAndMessaging.WINDOWPLACEMENT();
    placement.length = (uint)System.Runtime.InteropServices.Marshal.SizeOf(placement);

    System.Drawing.Rectangle windowRect;

    if (PInvoke.GetWindowPlacement(h, ref placement)) {
      // Use rcNormalPosition for minimized windows, current rect for visible windows
      var normalPos = placement.rcNormalPosition;
      int width = normalPos.right - normalPos.left;
      int height = normalPos.bottom - normalPos.top;

      // Use normal position - this works for both minimized and visible windows
      windowRect = new System.Drawing.Rectangle(normalPos.left, normalPos.top, width, height);
    } else {
      // Fallback to current window rect
      windowRect = Win32Helpers.GetWindowRect(hWnd);
    }

    if (windowRect.IsEmpty || windowRect.Width < 100 || windowRect.Height < 100) {
      return false;
    }

    var monitor = MonitorService.GetMonitorFromWindow(hWnd);
    if (monitor == null) {
      return false;
    }

    bool isFullscreen = Math.Abs(windowRect.Width - monitor.Bounds.Width) <= tolerance &&
                        Math.Abs(windowRect.Height - monitor.Bounds.Height) <= tolerance;

    return isFullscreen;
  }

  public void SwitchTo(IntPtr hWnd) {
    var h = new Windows.Win32.Foundation.HWND(hWnd);
    
    PInvoke.ShowWindow(h, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_RESTORE);
    PInvoke.ShowWindow(h, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_SHOW);
    PInvoke.SetForegroundWindow(h);

    // User32ExLib.SetForegroundWindow(hWnd);
    //  0 - HWND_TOP
    //  1 - HWND_BOTTOM
    // -2 - HWND_NOTOPMOST
    // -1 - HWND_TOPMOST
    //var hWndInsertAfter = new Windows.Win32.Foundation.HWND(-2);

    //if (PInvoke.SetWindowPos(h, hWndInsertAfter, 0, 0, 0, 0,
    //  Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_NOSIZE
    //  | Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_ASYNCWINDOWPOS
    //  | Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_NOMOVE
    //  | Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW
    //  | Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
    //  )) {
    //}
  }
  
  /// <summary>
  /// Detects the monitor location of the JW Library secondary window and stores it for auto-positioning.
  /// Only updates the stored location if the window is visible and has a reasonable size (not minimized/hidden).
  /// </summary>
  /// <param name="hWnd">Handle to the JW Library secondary window</param>
  private void DetectAndStoreMonitorLocation(IntPtr hWnd) {
    System.Diagnostics.Debug.WriteLine($"DetectAndStoreMonitorLocation called for hWnd=0x{hWnd:X}");
    try {
      var h = new Windows.Win32.Foundation.HWND(hWnd);

      // Get window rect first to show in debug output
      var windowRect = Win32Helpers.GetWindowRect(hWnd);
      System.Diagnostics.Debug.WriteLine($"JW Library window rect: X={windowRect.X}, Y={windowRect.Y}, W={windowRect.Width}, H={windowRect.Height}");

      // Check if window is minimized using GetWindowPlacement
      var placement = new Windows.Win32.UI.WindowsAndMessaging.WINDOWPLACEMENT();
      placement.length = (uint)System.Runtime.InteropServices.Marshal.SizeOf(placement);
      bool gotPlacement = PInvoke.GetWindowPlacement(h, ref placement);
      System.Diagnostics.Debug.WriteLine($"GetWindowPlacement result: {gotPlacement}, showCmd: {placement.showCmd}");

      if (gotPlacement) {
        var showCmd = placement.showCmd;
        // Skip if window is in any minimized or hidden state
        if (showCmd == Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_HIDE ||
            showCmd == Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_SHOWMINIMIZED ||
            showCmd == Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_MINIMIZE ||
            showCmd == Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_SHOWMINNOACTIVE ||
            showCmd == Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_FORCEMINIMIZE) {
          System.Diagnostics.Debug.WriteLine($"JW Library window is minimized/hidden (showCmd={showCmd}) - skipping monitor detection");
          return;
        }
      }

      // Validate the window rect - minimized windows are often at (-32000, -32000)
      // or have tiny/zero dimensions
      if (windowRect.IsEmpty || windowRect.Width < 100 || windowRect.Height < 100) {
        System.Diagnostics.Debug.WriteLine($"JW Library window has invalid size ({windowRect.Width}x{windowRect.Height}) - skipping monitor detection");
        return;
      }

      // Check for off-screen position (minimized windows are typically at -32000, -32000)
      if (windowRect.X < -10000 || windowRect.Y < -10000) {
        System.Diagnostics.Debug.WriteLine($"JW Library window is at off-screen position ({windowRect.X}, {windowRect.Y}) - skipping monitor detection");
        return;
      }

      // Get the monitor that contains this JW Library window
      var monitor = MonitorService.GetMonitorFromWindow(hWnd);

      if (monitor == null) {
        System.Diagnostics.Debug.WriteLine("Could not get monitor from JW Library window");
        return;
      }

      System.Diagnostics.Debug.WriteLine($"STORING target monitor: {monitor.Bounds}");
      // Store this as the target monitor for Zoom auto-positioning
      TargetMonitorTracker.SetJWLibraryTargetMonitor(monitor);
    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error detecting JW Library monitor: {ex.Message}");
      // Don't throw - this is not critical functionality
    }
  }

  public void OtherSwitchedBefore(IntPtr hWnd) {
    // Detect and store the monitor location BEFORE minimizing the window
    DetectAndStoreMonitorLocation(hWnd);
    
    var h = new Windows.Win32.Foundation.HWND(hWnd);
    PInvoke.ShowWindow(h, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_MINIMIZE);
  }

  public void OtherSwitchedAfter(IntPtr hWnd) {
  }

}

