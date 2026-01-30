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
    // Both windows seem to have similar hierarchy of children:
    //                                                       Main Window           Secondary Window
    //   "ApplicationFrameTitleBarWindow" (class)                [X]                    [ ]
    //   "ApplicationFrameTitleBarWindow" (class)                [ ]                    [ ]
    //                                                            |   
    //                                                            |
    //                                                          one of
    //                                                         despite of
    //                                                    full screen playback
    if (info.title == "Second Display ‎- JW Library") {
      // For a few months, second display had a unique name, so we could reliably detect it without
      // searching for any internals (with the LEFT-TO-RIGHT MARK unicode symbol inside the title).
      return true;
    }
    if (!(info.title == "JW Library")) {
      return false;
    }
    return !HasVisibleTitleBar(info);
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
    System.Diagnostics.Debug.WriteLine("DetectAndStoreMonitorLocation called");
    try {
      var h = new Windows.Win32.Foundation.HWND(hWnd);

      // Check if window is minimized using GetWindowPlacement
      // SW_SHOWMINIMIZED = 2, SW_MINIMIZE = 6, SW_HIDE = 0
      var placement = new Windows.Win32.UI.WindowsAndMessaging.WINDOWPLACEMENT();
      placement.length = (uint)System.Runtime.InteropServices.Marshal.SizeOf(placement);
      bool gotPlacement = PInvoke.GetWindowPlacement(h, ref placement);
      System.Diagnostics.Debug.WriteLine($"GetWindowPlacement result: {gotPlacement}, showCmd: {(uint)placement.showCmd}");

      if (gotPlacement) {
        uint showCmd = (uint)placement.showCmd;
        // Skip if minimized (2), hidden (0), or in minimize state (6)
        if (showCmd == 0 || showCmd == 2 || showCmd == 6) {
          System.Diagnostics.Debug.WriteLine($"JW Library window is minimized/hidden (showCmd={showCmd}) - skipping monitor detection");
          return;
        }
      }

      // If we already have a valid target monitor stored, don't overwrite it
      // This prevents the second JW Library window (main window) from overwriting
      // the correct monitor detected from the secondary display window
      if (TargetMonitorTracker.IsTargetMonitorValid()) {
        System.Diagnostics.Debug.WriteLine("Target monitor already set and valid - skipping");
        return;
      }

      // Get the monitor that contains this JW Library window
      var monitor = MonitorService.GetMonitorFromWindow(hWnd);

      if (monitor != null) {
        System.Diagnostics.Debug.WriteLine($"STORING target monitor: {monitor.Bounds}");
        // Store this as the target monitor for Zoom auto-positioning
        TargetMonitorTracker.SetJWLibraryTargetMonitor(monitor);
      } else {
        System.Diagnostics.Debug.WriteLine("Could not get monitor from JW Library window");
      }
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

