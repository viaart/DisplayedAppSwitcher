using DisplayedAppSwitcher.Services;
using System.Drawing;

namespace DisplayedAppSwitcher.Services;

/// <summary>
/// Service for positioning windows across monitors with state preservation.
/// </summary>
public static class WindowPositioningService {

  /// <summary>
  /// Moves a window to the same monitor as a target window, preserving window state.
  /// </summary>
  /// <param name="windowToMove">Handle of the window to move</param>
  /// <param name="targetWindow">Handle of the window whose monitor to target</param>
  /// <param name="useFullScreen">If true, position window to fill the entire monitor</param>
  /// <returns>True if positioning was successful, false otherwise</returns>
  public static bool MoveWindowToSameMonitorAs(IntPtr windowToMove, IntPtr targetWindow, bool useFullScreen = false) {
    if (windowToMove == IntPtr.Zero || targetWindow == IntPtr.Zero) {
      return false;
    }

    try {
      // Get the monitor of the target window
      var targetMonitor = MonitorService.GetMonitorFromWindow(targetWindow);
      if (targetMonitor == null) {
        System.Diagnostics.Debug.WriteLine("Could not determine target window monitor");
        return false;
      }

      // Move the window to the target monitor
      return MoveWindowToMonitor(windowToMove, targetMonitor, useFullScreen);
    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error moving window to same monitor: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Moves a window to the specified monitor.
  /// </summary>
  /// <param name="hWnd">Handle of the window to move</param>
  /// <param name="targetMonitor">Target monitor information</param>
  /// <param name="useFullScreen">If true, position window to fill the entire monitor</param>
  /// <returns>True if positioning was successful, false otherwise</returns>
  public static bool MoveWindowToMonitor(IntPtr hWnd, MonitorInfo targetMonitor, bool useFullScreen = false) {
    if (hWnd == IntPtr.Zero || targetMonitor == null) {
      return false;
    }

    try {
      // Check if window is already on the target monitor
      var currentMonitor = MonitorService.GetMonitorFromWindow(hWnd);
      if (currentMonitor != null && currentMonitor.Handle == targetMonitor.Handle) {
        return true;
      }

      // Determine if the window should be positioned full screen
      bool shouldUseFullScreen = useFullScreen || Win32Helpers.ShouldPositionFullScreen(hWnd);

      // Move the window
      bool result;
      if (shouldUseFullScreen) {
        // For full-screen positioning, use the aggressive method to ensure it fills the monitor
        result = Win32Helpers.ForceWindowToFillMonitor(hWnd, targetMonitor);
      } else {
        result = Win32Helpers.MoveWindowToMonitor(hWnd, targetMonitor);
      }

      return result;
    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error in MoveWindowToMonitor: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Gets diagnostic information about window positioning for debugging.
  /// </summary>
  /// <param name="hWnd">Handle of the window</param>
  /// <returns>String containing positioning information</returns>
  public static string GetWindowPositioningInfo(IntPtr hWnd) {
    try {
      var windowRect = Win32Helpers.GetWindowRect(hWnd);
      var monitor = MonitorService.GetMonitorFromWindow(hWnd);
      var isFullScreen = Win32Helpers.IsWindowFullScreen(hWnd);

      return $"Window: {windowRect}, Monitor: {monitor?.ToString() ?? "Unknown"}, FullScreen: {isFullScreen}";
    } catch (Exception ex) {
      return $"Error getting positioning info: {ex.Message}";
    }
  }
}
