using Windows.Win32;

namespace DisplayedAppSwitcher {
  public class ZoomSwitcher : ISwitcher {
    public bool IsTheRightWindow(string title, string className, uint styleFlags, uint exStyleFlags) {
      // Zoom creates several windows. The one that we are to bring forward is named "Zoom".
      // It has a class "ZPContentViewWndClass".
      // 
      // Seems to be only one so far per Zoom app except when Zoom meeting is not started yet.
      // In this case a blank window that wasn't previously shown will popup, so the
      // way to avoid it will be to check whether the window is already visible or not.
      //
      // However currently we have to remove this because this code is also used in restoring the
      // window back to its visible state from the hidden one.
      return (title == "Zoom"
            && className == "ZPContentViewWndClass");
      // (styleFlags & Consts.WS_VISIBLE) != 0)
      ;
    }

    public void SwitchTo(IntPtr hWnd) {
      var h = new Windows.Win32.Foundation.HWND(hWnd);
      PInvoke.ShowWindow(h, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_RESTORE);
      PInvoke.ShowWindow(h, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_SHOW);
      PInvoke.SetForegroundWindow(h);

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

}
