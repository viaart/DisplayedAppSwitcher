using Windows.Win32;

namespace DisplayedAppSwitcher;

public class JWLibrarySwitcher : ISwitcher {
  public bool IsTheRightWindow(SwitcherWindowInfo info, Purpose purpose) {
    // JW Library creates several windows for dual monitor, all of them are called "JW Library".
    // Also both have a class "ApplicationFrameWindow". They both technically consist of the same varying hierarchy of children:
    //
    // Initial (simplified) hierarchy:
    // 
    // + "JW Library" ApplicationFrameWindow
    //   + "" ApplicationFrameTitleBarWindow
    //   + "" ApplicationFrameTitleBarWindow
    //   + "" ApplicationFrameInputSinkWindow
    //
    // Modified hierarchy to accommodate different views:
    // 
    // + "JW Library" ApplicationFrameWindow
    //   ...(original children)
    //   + "JW Library" Windows.UI.Core.CoreWindow
    //     + "" XAMLWebViewHowtWindowClass
    //       + "" Internet Explorer_Server
    //     + "CoreInput" Windows.UI.Core.CoreComponentInputSource
    //
    // The only distinguishing difference seems to be that the secondary monitor window
    // has an Extended Style with a flag of:
    //   WS_EX_TOPMOST
    // 
    return info.title == "JW Library"
          && info.className == "ApplicationFrameWindow"
          && (info.exStyleFlags & Win32Constants.WS_EX_TOPMOST) != 0;
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

