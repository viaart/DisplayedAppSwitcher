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
    /// Gets whether newly appeared secondary Zoom windows are hidden automatically.
    /// </summary>
    public static bool GetAutoHideZoomSecondaryWindowEnabled() {
        lock (_lockObject) {
            return Settings.Default.AutoHideZoomSecondaryWindow;
        }
    }

    /// <summary>
    /// Sets whether newly appeared secondary Zoom windows are hidden automatically.
    /// </summary>
    /// <param name="enabled">True to enable auto-hiding, false to disable</param>
    public static void SetAutoHideZoomSecondaryWindowEnabled(bool enabled) {
        lock (_lockObject) {
            Settings.Default.AutoHideZoomSecondaryWindow = enabled;
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
                Settings.Default.AutoHideZoomSecondaryWindow = true;
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
    /// Which revision of the Tips content this build ships.
    ///
    /// To show the Tips window again to everyone, including users who ticked
    /// "Do not show again", increment this number. Nothing else needs renaming.
    ///
    /// Dismissal is tracked against this rather than against the application version.
    /// Keying on the version re-opens the window on every release, including patch
    /// releases that changed nothing a user needs to read. Keying on a revision means
    /// the window comes back exactly when the content actually changed.
    ///
    /// History:
    ///   1  Auto-positioning
    ///   2  Auto-hiding, and the Options submenu gaining a second entry
    /// </summary>
    public const int TipsContentRevision = 2;

    /// <summary>
    /// Gets the Tips revision that the user last dismissed. Zero means never dismissed.
    /// </summary>
    public static int GetTipsLastDismissedRevision() {
        lock (_lockObject) {
            return Settings.Default.TipsLastDismissedRevision;
        }
    }

    /// <summary>
    /// Records the Tips revision that the user has dismissed.
    /// </summary>
    /// <param name="revision">The revision to record as dismissed</param>
    public static void SetTipsLastDismissedRevision(int revision) {
        lock (_lockObject) {
            Settings.Default.TipsLastDismissedRevision = revision;
            SaveSettings();
        }
    }

    /// <summary>
    /// Determines if the Tips window should be shown.
    /// Returns true until the user dismisses the revision this build ships.
    /// A "less than" comparison rather than an equality check, so that a user who
    /// skipped several releases still sees the newest content exactly once.
    /// </summary>
    public static bool ShouldShowTips() {
        lock (_lockObject) {
            return Settings.Default.TipsLastDismissedRevision < TipsContentRevision;
        }
    }

    /// <summary>
    /// Marks the Tips content shipped by this build as dismissed.
    /// </summary>
    public static void DismissCurrentTips() {
        SetTipsLastDismissedRevision(TipsContentRevision);
    }

    /// <summary>
    /// Clears any dismissal, so the Tips window is shown again on the next start.
    /// </summary>
    public static void RestoreTips() {
        SetTipsLastDismissedRevision(0);
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
