using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace DisplayedAppSwitcher.Services;

/// <summary>
/// Information about a display monitor.
/// </summary>
public class MonitorInfo {
    internal HMONITOR Handle { get; set; }
    public Rectangle Bounds { get; set; }
    public Rectangle WorkArea { get; set; }
    public bool IsPrimary { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    
    public override string ToString() {
        return $"Monitor: {DeviceName} ({Bounds.Width}x{Bounds.Height}) at ({Bounds.X}, {Bounds.Y}) - Primary: {IsPrimary}";
    }
}

/// <summary>
/// Service for detecting and managing display monitors using Win32 APIs.
/// </summary>
public static unsafe class MonitorService {
    
    private static readonly object _lockObject = new();
    private static List<MonitorInfo>? _cachedMonitors;
    private static DateTime _lastCacheTime = DateTime.MinValue;
    private static readonly TimeSpan CacheValidityPeriod = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Gets all available monitors in the system.
    /// Results are cached for performance.
    /// </summary>
    public static List<MonitorInfo> GetAllMonitors() {
        lock (_lockObject) {
            // Check if cache is still valid
            if (_cachedMonitors != null && DateTime.Now - _lastCacheTime < CacheValidityPeriod) {
                return new List<MonitorInfo>(_cachedMonitors);
            }
            
            // Refresh monitor information
            var monitors = new List<MonitorInfo>();
            
            try {
                // Enumerate all monitors using Win32 API
                PInvoke.EnumDisplayMonitors(new HDC(), (RECT?)null, (HMONITOR hMonitor, HDC hdcMonitor, RECT* lprcMonitor, LPARAM dwData) => {
                    var monitorInfo = GetMonitorInfoFromHandle(hMonitor);
                    if (monitorInfo != null) {
                        monitors.Add(monitorInfo);
                    }
                    return true; // Continue enumeration
                }, 0);
            } catch (Exception ex) {
                // Log error but don't throw - return empty list for graceful degradation
                // TODO: Add logging when logging infrastructure is available
                System.Diagnostics.Debug.WriteLine($"Error enumerating monitors: {ex.Message}");
                return new List<MonitorInfo>();
            }
            
            // Update cache
            _cachedMonitors = monitors;
            _lastCacheTime = DateTime.Now;
            
            return new List<MonitorInfo>(monitors);
        }
    }
    
    /// <summary>
    /// Gets the monitor that contains the specified window.
    /// </summary>
    /// <param name="hWnd">Handle to the window</param>
    /// <returns>MonitorInfo for the monitor containing the window, or null if not found</returns>
    public static MonitorInfo? GetMonitorFromWindow(IntPtr hWnd) {
        if (hWnd == IntPtr.Zero) {
            return null;
        }
        
        try {
            var hMonitor = PInvoke.MonitorFromWindow(new HWND(hWnd), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
            return GetMonitorInfoFromHandle(hMonitor);
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Error getting monitor from window: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Gets the primary monitor.
    /// </summary>
    /// <returns>MonitorInfo for the primary monitor, or null if not found</returns>
    public static MonitorInfo? GetPrimaryMonitor() {
        var monitors = GetAllMonitors();
        return monitors.FirstOrDefault(m => m.IsPrimary);
    }
    
    /// <summary>
    /// Gets monitor information from a monitor handle.
    /// </summary>
    /// <param name="hMonitor">Handle to the monitor</param>
    /// <returns>MonitorInfo object or null if failed</returns>
    private static MonitorInfo? GetMonitorInfoFromHandle(HMONITOR hMonitor) {
        try {
            var monitorInfo = new MONITORINFO();
            monitorInfo.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<MONITORINFO>();
            
            if (!PInvoke.GetMonitorInfo(hMonitor, ref monitorInfo)) {
                return null;
            }
            
            var monitor = new MonitorInfo {
                Handle = hMonitor,
                Bounds = new Rectangle(
                    monitorInfo.rcMonitor.left,
                    monitorInfo.rcMonitor.top,
                    monitorInfo.rcMonitor.right - monitorInfo.rcMonitor.left,
                    monitorInfo.rcMonitor.bottom - monitorInfo.rcMonitor.top
                ),
                WorkArea = new Rectangle(
                    monitorInfo.rcWork.left,
                    monitorInfo.rcWork.top,
                    monitorInfo.rcWork.right - monitorInfo.rcWork.left,
                    monitorInfo.rcWork.bottom - monitorInfo.rcWork.top
                ),
                IsPrimary = (monitorInfo.dwFlags & 1) != 0, // MONITORINFOF_PRIMARY = 1
                DeviceName = "Monitor" // Simplified for now
            };
            
            return monitor;
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Error getting monitor info: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Invalidates the monitor cache, forcing a refresh on next access.
    /// Call this when monitor configuration might have changed.
    /// </summary>
    public static void InvalidateCache() {
        lock (_lockObject) {
            _cachedMonitors = null;
            _lastCacheTime = DateTime.MinValue;
        }
    }
    
    /// <summary>
    /// Gets the number of monitors in the system.
    /// </summary>
    public static int GetMonitorCount() {
        return GetAllMonitors().Count;
    }
    
    /// <summary>
    /// Checks if the system has multiple monitors.
    /// </summary>
    public static bool HasMultipleMonitors() {
        return GetMonitorCount() > 1;
    }
}
