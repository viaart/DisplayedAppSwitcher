using Windows.Win32;
using DisplayedAppSwitcher.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DisplayedAppSwitcher;
public class ZoomWorkspaceSwitcher : ISwitcher {
  public bool IsTheRightWindow(SwitcherWindowInfo info, Purpose purpose) {
    var zeroHWND = new Windows.Win32.Foundation.HWND(IntPtr.Zero);

    // Case 1: Zoom Workplace window (non-meeting or before joining)
    if (info.title == "Zoom Workplace" && info.className == "ConfMultiTabContentWndClass") {
      // When the meeting is not running, there's two windows with the "Zoom Workplace" title
      // that are running behind the scenes and simply "hidden".
      //
      // Since we are utilizing hiding, we need something else to figure out which one of them is our window.
      //
      // One of the windows has a "ContentRightPanel" and the other is not.
      var panel_hWnd = PInvoke.FindWindowEx(new Windows.Win32.Foundation.HWND(info.hWnd),
        zeroHWND,
        null as string, "ContentRightPanel");
      if (panel_hWnd.Value.ToString().Equals("0")) {
        // The one without the right panel should be ours
        return true;
      }
      return false;
    }

    // Case 2: Zoom Meeting secondary window (during active meeting)
    // This is the window without the control panel, used for screen sharing display
    if (info.title == "Zoom Meeting" && info.className == "ConfMultiTabContentWndClass") {
      // Recursively search for control panel - main window has it, secondary doesn't (max 3 levels deep)
      bool hasControlPanel = Win32Helpers.FindChildWindowRecursive(info.hWnd, "ZPControlPanelClass", null, 3);

      bool isSecondary = !hasControlPanel;
      System.Diagnostics.Debug.WriteLine($"ZoomWorkspaceSwitcher: Window 0x{info.hWnd:X} '{info.title}' hasControlPanel={hasControlPanel} isSecondary={isSecondary}");

      // Secondary window does NOT have the control panel
      return isSecondary;
    }

    return false;
    // (styleFlags & Consts.WS_VISIBLE) != 0)
  }

  public void SwitchTo(IntPtr hWnd) {
    var h = new Windows.Win32.Foundation.HWND(hWnd);

    // Restore and show the window first
    PInvoke.ShowWindow(h, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_RESTORE);
    PInvoke.ShowWindow(h, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_SHOW);

    // Check and apply auto-positioning every time
    // The positioning logic will skip if window is already correctly positioned
    ApplyAutoPositioning(hWnd);

    // Attempting to get Zoom to foreground through something like Stream Deck makes it simply
    // flash (apparently converting it into SetActiveWindow, FlashWindowEx sequence), perhaps
    // because of the way Stream Deck has been activated.

    //var result = PInvoke.SetForegroundWindow(h);

    // Full-Screen Zoom:
    // ===================================================
    // WS_VISIBLE                     WS_EX_LEFT
    // WS_CLIPSIBLINGS                WS_EX_LTRREADING
    // WS_CLIPCHILDREN                WS_EX_RIGHTSCROLLBAR
    // WS_MAXIMIZE
    // WS_OVERLAPPED
    //
    // (WS_MAXIMIZE disappears when Full-Screen Zoom is Moved by Zoom to the first screen upon Share Screen)
    //
    // Non-Full-Screen Zoom:
    // ===================================================
    // WS_CAPTION                     WS_EX_LEFT
    // WS_VISIBLE                     WS_EX_LTRREADING
    // WS_CLIPSIBLINGS                WS_EX_RIGHTSCROLLBAR
    // WS_CLIPCHILDREN                WS_EX_WINDOWEDGE       
    // WS_SYSMENU                     
    // WS_THICKFRAME                  
    // WS_OVERLAPPED
    // WS_MINIMIZEDBOX
    // WS_MAXIMIZEDBOX
  }
  
  /// <summary>
  /// Applies auto-positioning logic to move Zoom window to JW Library's monitor location.
  /// </summary>
  /// <param name="hWnd">Handle to the Zoom secondary window</param>
  private void ApplyAutoPositioning(IntPtr hWnd) {
    try {
      // Check if auto-positioning is enabled
      if (!SettingsManager.GetAutoPositionEnabled()) {
        System.Diagnostics.Debug.WriteLine("Auto-positioning is DISABLED");
        return;
      }

      // Get the target monitor from JW Library detection
      var targetMonitor = TargetMonitorTracker.JWLibraryTargetMonitor;

      // If we cannot find the JW Library secondary window location, do nothing
      if (targetMonitor == null) {
        System.Diagnostics.Debug.WriteLine("Target monitor is NULL - no JW Library location stored");
        return;
      }

      if (!TargetMonitorTracker.IsTargetMonitorValid()) {
        System.Diagnostics.Debug.WriteLine("Target monitor is EXPIRED");
        return;
      }

      System.Diagnostics.Debug.WriteLine($"Target monitor: {targetMonitor.Bounds}");

      // Get current window position
      var windowRect = Win32Helpers.GetWindowRect(hWnd);
      System.Diagnostics.Debug.WriteLine($"Zoom window rect: {windowRect}");

      if (windowRect.IsEmpty || windowRect.Width < 100 || windowRect.Height < 100) {
        System.Diagnostics.Debug.WriteLine("Zoom window has invalid size - skipping repositioning");
        return;
      }

      // Check if Zoom window is already correctly positioned on the target monitor
      bool alreadyCorrect = IsWindowCorrectlyPositioned(windowRect, targetMonitor);

      // ALWAYS reposition for now to test if repositioning works
      // Move Zoom window to the target monitor with full-screen positioning
      bool result = WindowPositioningService.MoveWindowToMonitor(hWnd, targetMonitor, true);

    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error in auto-positioning: {ex.Message}");
      // Don't throw - this is not critical functionality, should gracefully fall back
    }
  }

  /// <summary>
  /// Checks if a window is correctly positioned on the target monitor.
  /// A window is correctly positioned if its top-left corner aligns with the monitor
  /// and its size matches the monitor size.
  /// </summary>
  /// <param name="windowRect">Current window rectangle</param>
  /// <param name="monitor">Target monitor to compare against</param>
  /// <param name="tolerance">Pixel tolerance for position/size comparison (default: 20)</param>
  /// <returns>True if the window is correctly positioned on the monitor</returns>
  private bool IsWindowCorrectlyPositioned(System.Drawing.Rectangle windowRect, MonitorInfo monitor, int tolerance = 20) {
    try {
      // Check if window position matches monitor position (top-left alignment)
      bool positionMatches = Math.Abs(windowRect.X - monitor.Bounds.X) <= tolerance &&
                             Math.Abs(windowRect.Y - monitor.Bounds.Y) <= tolerance;

      // Check if window size matches monitor size
      bool sizeMatches = Math.Abs(windowRect.Width - monitor.Bounds.Width) <= tolerance &&
                         Math.Abs(windowRect.Height - monitor.Bounds.Height) <= tolerance;

      System.Diagnostics.Debug.WriteLine($"Position check: window({windowRect.X},{windowRect.Y}) vs monitor({monitor.Bounds.X},{monitor.Bounds.Y}) = {positionMatches}");
      System.Diagnostics.Debug.WriteLine($"Size check: window({windowRect.Width}x{windowRect.Height}) vs monitor({monitor.Bounds.Width}x{monitor.Bounds.Height}) = {sizeMatches}");

      return positionMatches && sizeMatches;
    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error checking window position: {ex.Message}");
      return false;
    }
  }

  public void OtherSwitchedBefore(IntPtr hWnd) {
    // The trick is to hide Zoom window when we are switching to JW Library.
    // This is because otherwise, if you are trying to share screen, you are getting
    // into a trouble of Zoom secondary monitor window attempting to get back to the first screen,
    // and then it completely covers the whole screen, being in a full screen mode.
    //
    // However, hiding it, seems to not only removing it from the equation, but also,
    // when we show it back, it restores WITH all the flags, like full screen.
    // 
    var h = new Windows.Win32.Foundation.HWND(hWnd);
    PInvoke.ShowWindow(h, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_HIDE);
  }

  public void OtherSwitchedAfter(IntPtr hWnd) {

  }

}

