﻿using Windows.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DisplayedAppSwitcher;
public class ZoomWorkspaceSwitcher : ISwitcher {
  public bool IsTheRightWindow(SwitcherWindowInfo info, Purpose purpose) {
    // Zoom creates several windows. The one that we are to bring forward is named "Zoom".
    // It has a class "ZPContentViewWndClass".

    if (info.title != "Zoom Workplace") return false;
    if (info.className != "ConfMultiTabContentWndClass") return false;

    // When the meeting is not running, there's two windows with the "Zoom Workplace" title
    // that are running behind the scenes and simply "hidden".
    //
    // Since we are utilizing hiding, we need something else to figure out which one of them is our window.
    //
    // One of the windows has a "ContentRightPanel" and the other is not.
    // 

    var zeroHWND = new Windows.Win32.Foundation.HWND(IntPtr.Zero);
    var panel_hWnd = PInvoke.FindWindowEx(new Windows.Win32.Foundation.HWND(info.hWnd),
      zeroHWND,
      null as string, "ContentRightPanel");
    if (panel_hWnd.Value.ToString().Equals("0")) {
      // The one without the right panel should be ours
      return true;
    }
    return false;
    // (styleFlags & Consts.WS_VISIBLE) != 0)
  }

  public void SwitchTo(IntPtr hWnd) {
    var h = new Windows.Win32.Foundation.HWND(hWnd);
    PInvoke.ShowWindow(h, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_RESTORE);
    PInvoke.ShowWindow(h, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_SHOW);

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

