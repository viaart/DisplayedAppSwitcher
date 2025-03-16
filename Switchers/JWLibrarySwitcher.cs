using System.Diagnostics;
using Windows.Win32;

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

  public void OtherSwitchedBefore(IntPtr hWnd) {
    var h = new Windows.Win32.Foundation.HWND(hWnd);
    PInvoke.ShowWindow(h, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_MINIMIZE);
  }

  public void OtherSwitchedAfter(IntPtr hWnd) {
  }

}

