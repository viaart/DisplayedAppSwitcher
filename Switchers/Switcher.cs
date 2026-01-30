using Windows.Win32;
using DisplayedAppSwitcher.Services;
using System.Drawing;

namespace DisplayedAppSwitcher;
public class SwitcherWindowInfo {
  public readonly IntPtr hWnd;
  public readonly string title;
  public readonly string className;
  public readonly uint styleFlags;
  public readonly uint exStyleFlags;
  public SwitcherWindowInfo(IntPtr hWnd, string title, string className, uint styleFlags, uint exStyleFlags) {
    this.hWnd = hWnd;
    this.title = title;
    this.className = className;
    this.styleFlags = styleFlags;
    this.exStyleFlags = exStyleFlags;
  }
}

public enum Purpose {
  Show,
  Hide,
}

public interface ISwitcher {
  /// <summary>
  /// IsTheRightWindow will be called to decide whether the window is the right window among the others,
  /// that are part of an application.
  /// </summary>
  /// <param name="info">Data for the window</param>
  /// <param name="purpose">Purpose of the query</param>
  bool IsTheRightWindow(SwitcherWindowInfo info, Purpose purpose);
  void SwitchTo(IntPtr hWnd);
  /// <summary>
  /// What to fire before another switcher fires ISwitcher.SwitchTo.
  /// </summary>
  /// <param name="hWnd">This window handler</param>
  void OtherSwitchedBefore(IntPtr hWnd);
  /// <summary>
  /// What to fire after another switcher has fired ISwitcher.SwitchTo.
  /// </summary>
  /// <param name="hWnd">This window handler</param>
  void OtherSwitchedAfter(IntPtr hWnd);
}


public unsafe class Win32Helpers {
  internal unsafe struct Buffer {
    public fixed char fixedBuffer[255];
  }
  internal unsafe class BufferHolder {
    public Buffer buffer = default;
  }

  /// <summary>
  /// Recursively searches for a child window with the specified class name and optional title.
  /// </summary>
  /// <param name="parent">Parent window handle</param>
  /// <param name="className">Class name to search for</param>
  /// <param name="title">Window title to search for (optional)</param>
  /// <param name="maxDepth">Maximum depth of recursion (0 = check direct children only)</param>
  /// <returns>True if a matching child window was found</returns>
  public static bool FindChildWindowRecursive(IntPtr parent, string className, string? title, int maxDepth) {
    var zeroHWND = new Windows.Win32.Foundation.HWND(IntPtr.Zero);
    var parentHWND = new Windows.Win32.Foundation.HWND(parent);

    // Check direct children first
    var child = PInvoke.FindWindowEx(parentHWND, zeroHWND, className, title);
    if (child != IntPtr.Zero) {
      return true;
    }

    // Stop if we've reached the maximum depth
    if (maxDepth <= 0) {
      return false;
    }

    // Enumerate all children and search recursively
    var currentChild = PInvoke.FindWindowEx(parentHWND, zeroHWND, null as string, null as string);
    while (currentChild != IntPtr.Zero) {
      if (FindChildWindowRecursive(currentChild, className, title, maxDepth - 1)) {
        return true;
      }
      currentChild = PInvoke.FindWindowEx(parentHWND, new Windows.Win32.Foundation.HWND(currentChild), null as string, null as string);
    }

    return false;
  }

  public static SwitcherWindowInfo GetWindowInfo(IntPtr hWnd) {
    var h = new Windows.Win32.Foundation.HWND(hWnd);
    var tBuffHolder = new BufferHolder();
    var cBuffHolder = new BufferHolder();
    string s;
    string c;
    unsafe {
      fixed (char* charPtr = tBuffHolder.buffer.fixedBuffer) {
        var b = new Windows.Win32.Foundation.PWSTR(charPtr);
        _ = PInvoke.GetWindowText(h, b, 256);
        s = new string(charPtr);
      }
      fixed (char* charPtr = cBuffHolder.buffer.fixedBuffer) {
        var b = new Windows.Win32.Foundation.PWSTR(charPtr);
        _ = PInvoke.GetClassName(h, b, 256);
        c = new string(charPtr);
      }

    }
    uint styleFlags = (uint)PInvoke.GetWindowLong(h, Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX.GWL_STYLE);
    uint exStyleFlags = (uint)PInvoke.GetWindowLong(h, Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
    return new SwitcherWindowInfo(hWnd, s, c, styleFlags, exStyleFlags);
  }

  /// <summary>
  /// Gets the rectangle (position and size) of a window.
  /// </summary>
  /// <param name="hWnd">Handle to the window</param>
  /// <returns>Rectangle representing the window bounds, or empty rectangle if failed</returns>
  public static Rectangle GetWindowRect(IntPtr hWnd) {
    try {
      var h = new Windows.Win32.Foundation.HWND(hWnd);
      if (PInvoke.GetWindowRect(h, out var rect)) {
        return new Rectangle(
          rect.left,
          rect.top,
          rect.right - rect.left,
          rect.bottom - rect.top
        );
      }
    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error getting window rect: {ex.Message}");
    }
    return Rectangle.Empty;
  }
  
  /// <summary>
  /// Moves a window to the specified monitor while preserving its relative position and size.
  /// </summary>
  /// <param name="hWnd">Handle to the window to move</param>
  /// <param name="targetMonitor">Target monitor information</param>
  /// <returns>True if the window was moved successfully, false otherwise</returns>
  public static bool MoveWindowToMonitor(IntPtr hWnd, MonitorInfo targetMonitor) {
    if (hWnd == IntPtr.Zero || targetMonitor == null) {
      return false;
    }
    
    try {
      var h = new Windows.Win32.Foundation.HWND(hWnd);
      
      // Get current window position and size
      var currentRect = GetWindowRect(hWnd);
      if (currentRect.IsEmpty) {
        return false;
      }
      
      // Get current monitor
      var currentMonitor = MonitorService.GetMonitorFromWindow(hWnd);
      if (currentMonitor == null) {
        return false;
      }
      
      // If already on target monitor, no need to move
      if (currentMonitor.Handle.Value == targetMonitor.Handle.Value) {
        return true;
      }
      
      // Calculate relative position on current monitor
      var relativeX = (double)(currentRect.X - currentMonitor.Bounds.X) / currentMonitor.Bounds.Width;
      var relativeY = (double)(currentRect.Y - currentMonitor.Bounds.Y) / currentMonitor.Bounds.Height;
      
      // Calculate new position on target monitor
      var newX = targetMonitor.Bounds.X + (int)(relativeX * targetMonitor.Bounds.Width);
      var newY = targetMonitor.Bounds.Y + (int)(relativeY * targetMonitor.Bounds.Height);
      
      // Ensure window fits within target monitor bounds
      var newWidth = Math.Min(currentRect.Width, targetMonitor.Bounds.Width);
      var newHeight = Math.Min(currentRect.Height, targetMonitor.Bounds.Height);
      
      // Adjust position if window would extend beyond monitor bounds
      if (newX + newWidth > targetMonitor.Bounds.Right) {
        newX = targetMonitor.Bounds.Right - newWidth;
      }
      if (newY + newHeight > targetMonitor.Bounds.Bottom) {
        newY = targetMonitor.Bounds.Bottom - newHeight;
      }
      
      // Ensure position is not negative
      newX = Math.Max(newX, targetMonitor.Bounds.X);
      newY = Math.Max(newY, targetMonitor.Bounds.Y);
      
      // Move the window
      return PInvoke.SetWindowPos(
        h,
        new Windows.Win32.Foundation.HWND(),
        newX,
        newY,
        newWidth,
        newHeight,
        Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_NOZORDER |
        Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
      );
    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error moving window to monitor: {ex.Message}");
      return false;
    }
  }
  
  /// <summary>
  /// Moves a window to fill the entire monitor (full screen positioning).
  /// </summary>
  /// <param name="hWnd">Handle to the window to move</param>
  /// <param name="targetMonitor">Target monitor information</param>
  /// <returns>True if the window was positioned successfully, false otherwise</returns>
  public static bool MoveWindowToMonitorFullScreen(IntPtr hWnd, MonitorInfo targetMonitor) {
    if (hWnd == IntPtr.Zero || targetMonitor == null) {
      return false;
    }
    
    try {
      var h = new Windows.Win32.Foundation.HWND(hWnd);
      
      // Position window to fill the entire monitor, using SWP_FRAMECHANGED to ensure proper sizing
      bool result = PInvoke.SetWindowPos(
        h,
        new Windows.Win32.Foundation.HWND(),
        targetMonitor.Bounds.X,
        targetMonitor.Bounds.Y,
        targetMonitor.Bounds.Width,
        targetMonitor.Bounds.Height,
        Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_NOZORDER |
        Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE |
        Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
      );
      
      return result;
    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error moving window to monitor full screen: {ex.Message}");
      return false;
    }
  }
  
  /// <summary>
  /// Gets the monitor that contains the specified window.
  /// </summary>
  /// <param name="hWnd">Handle to the window</param>
  /// <returns>MonitorInfo for the monitor containing the window, or null if not found</returns>
  public static MonitorInfo? GetWindowMonitor(IntPtr hWnd) {
    return MonitorService.GetMonitorFromWindow(hWnd);
  }
  
  /// <summary>
  /// Checks if a window is currently full screen on its monitor.
  /// </summary>
  /// <param name="hWnd">Handle to the window</param>
  /// <returns>True if the window appears to be full screen, false otherwise</returns>
  public static bool IsWindowFullScreen(IntPtr hWnd) {
    var windowRect = GetWindowRect(hWnd);
    if (windowRect.IsEmpty) {
      return false;
    }
    
    var monitor = GetWindowMonitor(hWnd);
    if (monitor == null) {
      return false;
    }
    
    // Consider window full screen if it covers most of the monitor
    // Allow for small differences due to taskbar or other UI elements
    var tolerance = 10;
    return Math.Abs(windowRect.X - monitor.Bounds.X) <= tolerance &&
           Math.Abs(windowRect.Y - monitor.Bounds.Y) <= tolerance &&
           Math.Abs(windowRect.Width - monitor.Bounds.Width) <= tolerance &&
           Math.Abs(windowRect.Height - monitor.Bounds.Height) <= tolerance;
  }
  
  /// <summary>
  /// Checks if a window should be positioned full-screen based on multiple criteria.
  /// This is more comprehensive than IsWindowFullScreen for positioning decisions.
  /// </summary>
  /// <param name="hWnd">Handle to the window</param>
  /// <returns>True if the window should be positioned full-screen</returns>
  public static bool ShouldPositionFullScreen(IntPtr hWnd) {
    try {
      // First check if it's already full-screen
      if (IsWindowFullScreen(hWnd)) {
        return true;
      }
      
      // Check if window is maximized
      if (IsWindowMaximized(hWnd)) {
        return true;
      }
      
      // For Zoom windows, check if they occupy most of the monitor
      // (Zoom might not be exactly full-screen but still should be positioned as such)
      var windowRect = GetWindowRect(hWnd);
      var monitor = GetWindowMonitor(hWnd);
      
      if (windowRect.IsEmpty || monitor == null) {
        return false;
      }
      
      // Calculate how much of the monitor the window covers
      double widthCoverage = (double)windowRect.Width / monitor.Bounds.Width;
      double heightCoverage = (double)windowRect.Height / monitor.Bounds.Height;
      
      // If window covers more than 80% of monitor area, treat as full-screen
      return widthCoverage > 0.8 && heightCoverage > 0.8;
      
    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error in ShouldPositionFullScreen: {ex.Message}");
      return false;
    }
  }
  
  /// <summary>
  /// Checks if a window is maximized.
  /// </summary>
  /// <param name="hWnd">Handle to the window</param>
  /// <returns>True if the window is maximized</returns>
  public static bool IsWindowMaximized(IntPtr hWnd) {
    try {
      var h = new Windows.Win32.Foundation.HWND(hWnd);
      var placement = new Windows.Win32.UI.WindowsAndMessaging.WINDOWPLACEMENT();
      placement.length = (uint)System.Runtime.InteropServices.Marshal.SizeOf(placement);
      
      bool result = PInvoke.GetWindowPlacement(h, ref placement);
      if (result) {
        return placement.showCmd == Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED;
      }
      
      return false;
    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error checking if window is maximized: {ex.Message}");
      return false;
    }
  }
  
  /// <summary>
  /// Aggressively moves and resizes a window to fill the entire monitor using multiple approaches.
  /// This method tries several techniques to ensure the window fills the monitor completely.
  /// </summary>
  /// <param name="hWnd">Handle to the window to move</param>
  /// <param name="targetMonitor">Target monitor information</param>
  /// <returns>True if the window was positioned successfully, false otherwise</returns>
  public static bool ForceWindowToFillMonitor(IntPtr hWnd, MonitorInfo targetMonitor) {
    if (hWnd == IntPtr.Zero || targetMonitor == null) {
      return false;
    }
    
    try {
      var h = new Windows.Win32.Foundation.HWND(hWnd);
      
      // First, try normal positioning
      bool result1 = PInvoke.SetWindowPos(
        h,
        new Windows.Win32.Foundation.HWND(),
        targetMonitor.Bounds.X,
        targetMonitor.Bounds.Y,
        targetMonitor.Bounds.Width,
        targetMonitor.Bounds.Height,
        Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_NOZORDER |
        Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE |
        Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
      );
      
      // Give Windows time to process
      System.Threading.Thread.Sleep(50);
      
      // Check if it worked
      var newRect = GetWindowRect(hWnd);
      
      // Calculate tolerance for "close enough"
      int tolerance = 5;
      bool correctPosition = Math.Abs(newRect.X - targetMonitor.Bounds.X) <= tolerance &&
                           Math.Abs(newRect.Y - targetMonitor.Bounds.Y) <= tolerance &&
                           Math.Abs(newRect.Width - targetMonitor.Bounds.Width) <= tolerance &&
                           Math.Abs(newRect.Height - targetMonitor.Bounds.Height) <= tolerance;
      
      if (correctPosition) {
        return true;
      }
      
      // If not quite right, try a second approach with different flags
      bool result2 = PInvoke.SetWindowPos(
        h,
        new Windows.Win32.Foundation.HWND(),
        targetMonitor.Bounds.X,
        targetMonitor.Bounds.Y,
        targetMonitor.Bounds.Width,
        targetMonitor.Bounds.Height,
        Windows.Win32.UI.WindowsAndMessaging.SET_WINDOW_POS_FLAGS.SWP_NOZORDER
      );
      
      return result1 || result2;
      
    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error in ForceWindowToFillMonitor: {ex.Message}");
      return false;
    }
  }

}
