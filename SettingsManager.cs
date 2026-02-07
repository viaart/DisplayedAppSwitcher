using DisplayedAppSwitcher.Properties;
using System.Reflection;

namespace DisplayedAppSwitcher;

/// <summary>
/// Centralized settings management with thread-safe access to user settings.
/// </summary>
public static class SettingsManager {

    private static readonly object _lockObject = new();

    /// <summary>
    /// Gets the current application version in short format (Major.Minor.Build).
    /// </summary>
    public static string CurrentVersion {
        get {
            try {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                if (version != null) {
                    return $"{version.Major}.{version.Minor}.{version.Build}";
                }
            } catch { }
            return "0.0.0";
        }
    }
    
    /// <summary>
    /// Gets whether auto-positioning of Zoom window is enabled.
    /// </summary>
    public static bool GetAutoPositionEnabled() {
        lock (_lockObject) {
            return Settings.Default.AutoPositionZoomWindow;
        }
    }
    
    /// <summary>
    /// Sets whether auto-positioning of Zoom window is enabled.
    /// </summary>
    /// <param name="enabled">True to enable auto-positioning, false to disable</param>
    public static void SetAutoPositionEnabled(bool enabled) {
        lock (_lockObject) {
            Settings.Default.AutoPositionZoomWindow = enabled;
            SaveSettings();
        }
    }
    
    /// <summary>
    /// Loads settings from the user configuration file.
    /// </summary>
    public static void LoadSettings() {
        lock (_lockObject) {
            try {
                Settings.Default.Reload();
            } catch (System.Configuration.ConfigurationErrorsException) {
                // Handle corrupted settings file by resetting to defaults
                ResetToDefaults();
            }
        }
    }
    
    /// <summary>
    /// Saves current settings to the user configuration file.
    /// </summary>
    public static void SaveSettings() {
        lock (_lockObject) {
            try {
                Settings.Default.Save();
            } catch (System.Configuration.ConfigurationErrorsException) {
                // Log error but don't throw - settings will use defaults
                // TODO: Add logging when logging infrastructure is available
            }
        }
    }
    
    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
    public static void ResetToDefaults() {
        lock (_lockObject) {
            try {
                Settings.Default.Reset();
                SaveSettings();
            } catch (System.Configuration.ConfigurationErrorsException) {
                // If reset fails, manually set known defaults
                Settings.Default.AutoPositionZoomWindow = true;
            }
        }
    }
    
    /// <summary>
    /// Initializes settings on application startup.
    /// Handles migration from previous versions if needed.
    /// </summary>
    public static void InitializeSettings() {
        lock (_lockObject) {
            try {
                // Check if this is the first run or settings need upgrade
                if (Settings.Default.AutoPositionZoomWindow == false && IsFirstRun()) {
                    // This might be a fresh install, set default to true
                    Settings.Default.AutoPositionZoomWindow = true;
                    SaveSettings();
                }
                
                LoadSettings();
            } catch (System.Configuration.ConfigurationErrorsException) {
                ResetToDefaults();
            }
        }
    }
    
    /// <summary>
    /// Determines if this is the first run of the application.
    /// This is a simple heuristic and could be enhanced with version tracking.
    /// </summary>
    private static bool IsFirstRun() {
        // Simple check - if the setting file doesn't exist yet
        // This is a basic implementation that could be enhanced
        try {
            var userConfigPath = System.Configuration.ConfigurationManager.OpenExeConfiguration(
                System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            return !System.IO.File.Exists(userConfigPath);
        } catch {
            return true; // Assume first run if we can't determine
        }
    }

    /// <summary>
    /// Gets the version when tips were last dismissed.
    /// </summary>
    public static string GetTipsLastDismissedVersion() {
        lock (_lockObject) {
            return Settings.Default.TipsLastDismissedVersion ?? "";
        }
    }

    /// <summary>
    /// Sets the version when tips were dismissed (marks tips as dismissed for this version).
    /// </summary>
    /// <param name="version">The version to record as dismissed</param>
    public static void SetTipsLastDismissedVersion(string version) {
        lock (_lockObject) {
            Settings.Default.TipsLastDismissedVersion = version;
            SaveSettings();
        }
    }

    /// <summary>
    /// Determines if the Tips window should be shown.
    /// Returns true if tips have never been dismissed or were dismissed for an older version.
    /// </summary>
    public static bool ShouldShowTips() {
        lock (_lockObject) {
            var lastDismissedVersion = Settings.Default.TipsLastDismissedVersion;

            // Show tips if never dismissed
            if (string.IsNullOrEmpty(lastDismissedVersion)) {
                return true;
            }

            // Show tips if current version is newer than last dismissed version
            try {
                var dismissed = new Version(lastDismissedVersion);
                var current = new Version(CurrentVersion);
                return current > dismissed;
            } catch {
                // If version parsing fails, show tips to be safe
                return true;
            }
        }
    }

    /// <summary>
    /// Marks tips as dismissed for the current version.
    /// </summary>
    public static void DismissTipsForCurrentVersion() {
        SetTipsLastDismissedVersion(CurrentVersion);
    }

    public static DateTime GetUpdatePostponedUntil() {
        lock (_lockObject) {
            return Settings.Default.UpdatePostponedUntil;
        }
    }

    public static void SetUpdatePostponedUntil(DateTime until) {
        lock (_lockObject) {
            Settings.Default.UpdatePostponedUntil = until;
            SaveSettings();
        }
    }

    public static bool IsUpdatePostponed() {
        return GetUpdatePostponedUntil() > DateTime.UtcNow;
    }
}
