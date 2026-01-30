using DisplayedAppSwitcher.Services;

namespace DisplayedAppSwitcher;

/// <summary>
/// Static storage for target monitor information used for auto-positioning.
/// Thread-safe storage of JW Library monitor location for Zoom positioning.
/// </summary>
public static class TargetMonitorTracker {
    
    private static readonly object _lockObject = new();
    private static MonitorInfo? _jwLibraryTargetMonitor;
    private static DateTime _lastDetectionTime = DateTime.MinValue;
    
    /// <summary>
    /// Gets the target monitor where JW Library secondary window was last detected.
    /// </summary>
    public static MonitorInfo? JWLibraryTargetMonitor {
        get {
            lock (_lockObject) {
                return _jwLibraryTargetMonitor;
            }
        }
    }
    
    /// <summary>
    /// Sets the target monitor based on JW Library secondary window detection.
    /// </summary>
    /// <param name="monitor">Monitor information where JW Library secondary window is located</param>
    public static void SetJWLibraryTargetMonitor(MonitorInfo? monitor) {
        lock (_lockObject) {
            _jwLibraryTargetMonitor = monitor;
            _lastDetectionTime = DateTime.Now;
            
            System.Diagnostics.Debug.WriteLine($"JW Library target monitor set: {monitor?.ToString() ?? "None"}");
        }
    }
    
    /// <summary>
    /// Gets the time when the JW Library target monitor was last detected.
    /// </summary>
    public static DateTime LastDetectionTime {
        get {
            lock (_lockObject) {
                return _lastDetectionTime;
            }
        }
    }
    
    /// <summary>
    /// Checks if the target monitor information is still valid (not too old).
    /// </summary>
    /// <param name="maxAge">Maximum age of the detection to consider valid</param>
    /// <returns>True if the target monitor info is recent enough to be trusted</returns>
    public static bool IsTargetMonitorValid(TimeSpan? maxAge = null) {
        lock (_lockObject) {
            var ageLimit = maxAge ?? TimeSpan.FromMinutes(30); // Default: 30 minutes
            return _jwLibraryTargetMonitor != null && 
                   DateTime.Now - _lastDetectionTime <= ageLimit;
        }
    }
    
    /// <summary>
    /// Clears the target monitor information.
    /// </summary>
    public static void ClearTargetMonitor() {
        lock (_lockObject) {
            _jwLibraryTargetMonitor = null;
            _lastDetectionTime = DateTime.MinValue;
            
            System.Diagnostics.Debug.WriteLine("JW Library target monitor cleared");
        }
    }
    
    /// <summary>
    /// Gets diagnostic information about the current target monitor state.
    /// </summary>
    public static string GetDiagnosticInfo() {
        lock (_lockObject) {
            if (_jwLibraryTargetMonitor == null) {
                return "No target monitor set";
            }
            
            var age = DateTime.Now - _lastDetectionTime;
            return $"Target: {_jwLibraryTargetMonitor}, Age: {age.TotalMinutes:F1} minutes";
        }
    }
}
