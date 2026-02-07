using System.ComponentModel;
using System.Diagnostics;
using System.Security.Policy;
using System.Text.Json.Nodes;
using Windows.Win32;
using System;
using System.Management;


namespace DisplayedAppSwitcher;
public partial class SettingsForm : Form {

  public SettingsForm() {
    // Initialize settings first
    SettingsManager.InitializeSettings();
    
    InitializeComponent();
    switchers.Add(AppType.JWLibrary, new JWLibrarySwitcher());
    switchers.Add(AppType.Zoom, new ZoomWorkspaceSwitcher());
    InitializeTrayIcon();
    ScheduleCheckForUpdates();
  }

  private readonly Dictionary<AppType, ISwitcher> switchers = new();

  protected override void WndProc(ref Message m) {
    if (m.Msg == Win32Constants.WM_HOTKEY) {
      int id = m.WParam.ToInt32();
      switch (id) {
        case HotKeys.switchHotKeyId:
          DoSwitch(null, EventArgs.Empty);
          break;
        case HotKeys.oneHotKeyId:
        case HotKeys.oneHotKeySubId:
          BringToTop(AppType.JWLibrary);
          break;
        case HotKeys.secondHotKeyId:
        case HotKeys.secondHotKeySubId:
          BringToTop(AppType.Zoom);
          break;
      }
    }
    base.WndProc(ref m);
  }

  protected override void OnLoad(EventArgs e) {
    Visible = false;
    ShowInTaskbar = false;
    HotKeys.Register(Handle);
    base.OnLoad(e);

    // Show tips window if needed (new version or first run)
    ShowTipsIfNeeded();
  }

  private void ShowTipsIfNeeded() {
    if (SettingsManager.ShouldShowTips()) {
      // Use BeginInvoke to show the form after the main form is fully loaded
      BeginInvoke(new Action(() => {
        using var tipsForm = new TipsForm();
        tipsForm.ShowDialog();
      }));
    }
  }

  protected override void OnDeactivate(EventArgs e) {
    base.OnDeactivate(e);
  }

  protected override void OnClosing(CancelEventArgs e) {
    UnscheduleCheckUpdates();
    HotKeys.Unresister(Handle);
    base.OnClosing(e);
  }

  TrayIcon? icon;

  void InitializeTrayIcon() {
    icon = new TrayIcon();
    icon.ExitClick += (_, _) => {
      icon.Dispose();
      OnClosing(new CancelEventArgs());
      Application.Exit();
    };
    icon.SwitchClick += DoSwitch;
    icon.BringOneToTopClick += (_, _) => {
      BringToTop(AppType.JWLibrary);
    };
    icon.BringSecondToTopClick += (_, _) => {
      BringToTop(AppType.Zoom);
    };
    icon.AboutClick += (_, _) => {
      ShowAboutBox();
    };
    icon.CheckUpdateClick += CheckForUpdatesEvent(true);

    
  }

  private ManagementEventWatcher? powerEventWatcher;

  private void ScheduleCheckForUpdates() {
    // Once when the app starts
    _ = CheckForUpdates(false);
    // Create a ManagementEventWatcher to listen for power management events
    powerEventWatcher = new ManagementEventWatcher(
        new WqlEventQuery("SELECT * FROM Win32_PowerManagementEvent"));
    // Subscribe to the event
    powerEventWatcher.EventArrived += PowerEventWatcher_EventArrived;
    // Start listening for events
    powerEventWatcher.Start();
  }

  private void PowerEventWatcher_EventArrived(object sender, EventArrivedEventArgs e) {
    // Extract the event type
    ushort eventType = (ushort)e.NewEvent.Properties["EventType"].Value;
    if (eventType == 7) {
      // EventType 7 indicates the system has resumed from sleep or hibernation
      // Giving a few seconds for networks to join
      // TODO: Turn this into attempts
      wakeUpTimer = new System.Timers.Timer(10_000); 
      wakeUpTimer.Elapsed += (_, _) => {
        _ = CheckForUpdates(false);
      };
      wakeUpTimer.AutoReset = false; // Make sure the timer triggers only once
      wakeUpTimer.Start();
    }
  }

  private static System.Timers.Timer? wakeUpTimer;

  private void UnscheduleCheckUpdates() {
    powerEventWatcher?.Stop();
  }

  private void ShowAboutBox() {
    AboutBoxForm b = new(newVersionAvailable);
    b.ShowDialog();
  }

  private string newVersionAvailable = "";

  private EventHandler<EventArgs> CheckForUpdatesEvent(Boolean forceBalloon) {
    return async (_, _) => {
      await CheckForUpdates(forceBalloon);
    };
  }

  private async Task CheckForUpdates(bool forceBalloon) {
    // Skip auto-check if user postponed updates (manual check always proceeds)
    if (!forceBalloon && SettingsManager.IsUpdatePostponed()) {
      return;
    }

    (bool yes, string newVersion) = await CheckForNewReleaseAsync();
    if (yes) {
      newVersionAvailable = newVersion;
      ShowUpdateForm();
    } else {
      newVersionAvailable = "";
      if (newVersion != "" && forceBalloon) {
        string message =
        $"No updates found.\nYou are using the latest version, v{AboutBoxForm.ShortAssemblyVersion}, of Displayed App Switcher.";
        const string caption = "Check for Updates";
        _ = MessageBox.Show(message, caption,
                                     MessageBoxButtons.OK,
                                     MessageBoxIcon.Question);
      }
    }
  }

  private void ShowUpdateForm() {
    using var form = new UpdateForm(newVersionAvailable, AboutBoxForm.ShortAssemblyVersion);
    form.ShowDialog();
  }

  private async
  Task<(bool, string)>
CheckForNewReleaseAsync() {
    using HttpClient client = new();
    try {
      client.DefaultRequestHeaders.UserAgent.TryParseAdd("request"); // Set the User Agent to avoid GitHub API rejection
      string url = $"https://api.github.com/repos/viaart/DisplayedAppSwitcher/releases/latest";
      HttpResponseMessage response = await client.GetAsync(url);
      response.EnsureSuccessStatusCode();

      string responseBody = await response.Content.ReadAsStringAsync();
      var release = JsonObject.Parse(responseBody);
      if (release != null) {
        string latestVersion = release["tag_name"]?.ToString() ?? "";
        if (IsNewerVersion(latestVersion, AboutBoxForm.ShortAssemblyVersion)) {
          return (true, latestVersion);
        } else {
          return (false, latestVersion);
        }
      }
      return (false, "");
    } catch (HttpRequestException) {
      // TODO: Catch a network exception
      return (false, "");
    } catch (Exception) {
      // TODO: Catch version parsing errors
      return (false, "");
    }
  }

  private bool IsNewerVersion(string latestVersion, string currentVersion) {
    Version latest = new Version(latestVersion.TrimStart('v')); // Trim 'v' if present
    Version current = new Version(currentVersion);
    return latest.CompareTo(current) > 0;
  }


  private int lastSwitcherIndex = 0;

  void DoSwitch(object? _, EventArgs __) {
    lastSwitcherIndex = (lastSwitcherIndex + 1) % switchers.Count;
    BringToTop((AppType)lastSwitcherIndex);
  }

  void BringToTop(AppType appTypeIndex) {
    ISwitcher switcher = switchers[appTypeIndex]; // Throws an exception if not found. We are fine with the early error discovery.
    // And remember the last one in case it's called aside of switch
    lastSwitcherIndex = (int)appTypeIndex;

    // Clear target monitor at the start of each switch to Zoom
    // This ensures fresh detection of JW Library's monitor location
    if (appTypeIndex == AppType.Zoom) {
      TargetMonitorTracker.ClearTargetMonitor();
    }

    Windows.Win32.Foundation.HWND hWndParent = new(IntPtr.Zero);

    var hWnd = IntPtr.Zero;
    while (true) {
      hWnd = PInvoke.FindWindowEx(hWndParent, new Windows.Win32.Foundation.HWND(hWnd), null as string, null as string);
      if (hWnd != IntPtr.Zero) {
        var p = Win32Helpers.GetWindowInfo(hWnd);
        foreach (var s in switchers) {
          if (!s.Value.Equals(switcher)) {
            if (s.Value.IsTheRightWindow(p, Purpose.Hide)) {
              s.Value.OtherSwitchedBefore(hWnd);
            }
          }
        }

      } else {
        break;
      }
    }

    hWnd = IntPtr.Zero;
    while (true) {
      hWnd = PInvoke.FindWindowEx(hWndParent, new Windows.Win32.Foundation.HWND(hWnd), null as string, null as string);
      if (hWnd != IntPtr.Zero) {
        var p = Win32Helpers.GetWindowInfo(hWnd);
        if (switcher.IsTheRightWindow(p, Purpose.Show)) {
          switcher.SwitchTo(hWnd);
        }
      } else {
        break;
      }
    }

    while (true) {
      hWnd = PInvoke.FindWindowEx(hWndParent, new Windows.Win32.Foundation.HWND(hWnd), null as string, null as string);
      if (hWnd != IntPtr.Zero) {
        var p = Win32Helpers.GetWindowInfo(hWnd);
        foreach (var s in switchers) {
          if (!s.Value.Equals(switcher)) {
            if (s.Value.IsTheRightWindow(p, Purpose.Hide)) {
              s.Value.OtherSwitchedAfter(hWnd);
            }
          }
        }
      } else {
        break;
      }
    }

  }
}

public class TrayIcon {
  private NotifyIcon trayIcon;
  private ContextMenuStrip trayMenu;

  public TrayIcon() {
    ToolStripMenuItem v;

    trayMenu = new ContextMenuStrip();

    v = new ToolStripMenuItem("Switch", null, OnSwitchClick);
    v.ShortcutKeyDisplayString = "Ctrl+NumPad0";
    trayMenu.Items.Add(v);
    trayMenu.Items.Add(new ToolStripSeparator());

    v = new ToolStripMenuItem("JW Library", null, OnBringOneToTopClick);
    trayMenu.Items.Add(v);

    v.ShortcutKeyDisplayString = "F9  |  Ctrl+NumPad1";
    v = new ToolStripMenuItem("Zoom", null, OnBringSecondToTopClick);
    trayMenu.Items.Add(v);

    v.ShortcutKeyDisplayString = "F10  |  Ctrl+NumPad2";
    trayMenu.Items.Add(new ToolStripSeparator());

    // Options submenu
    var optionsMenu = new ToolStripMenuItem("Options");
    
    // Auto-position Zoom Window checkbox
    var autoPositionMenuItem = new ToolStripMenuItem("Auto-position Secondary Zoom Window", null, OnAutoPositionToggleClick);
    autoPositionMenuItem.CheckOnClick = true;
    autoPositionMenuItem.Checked = SettingsManager.GetAutoPositionEnabled();
    optionsMenu.DropDownItems.Add(autoPositionMenuItem);
    
    trayMenu.Items.Add(optionsMenu);
    trayMenu.Items.Add(new ToolStripSeparator());

    trayMenu.Items.Add(new ToolStripMenuItem("About", null, OnAboutClick));
    trayMenu.Items.Add(new ToolStripMenuItem("Check for updates...", null, OnCheckUpdateClick));
    
    trayMenu.Items.Add(new ToolStripMenuItem("Exit", null, OnExitClick));

    trayIcon = new NotifyIcon {
      Text = "Displayed App Switcher",
      Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
      ContextMenuStrip = trayMenu,
      Visible = true,
    };
    trayIcon.DoubleClick += OnSwitchClick;
    
    // Set initial tooltip with auto-positioning status
    UpdateTrayIconTooltip();
  }

  public void Dispose() {
    trayIcon.ContextMenuStrip?.Dispose();
    trayIcon.ContextMenuStrip = null;
    trayIcon.Dispose();
  }

  public event EventHandler<EventArgs>? BringOneToTopClick;
  public event EventHandler<EventArgs>? BringSecondToTopClick;

  public event EventHandler<EventArgs>? SwitchClick;

  public event EventHandler<EventArgs>? AboutClick;
  public event EventHandler<EventArgs>? CheckUpdateClick;

  public event EventHandler<EventArgs>? ExitClick;
  public event EventHandler<EventArgs>? SettingsClick;

  private void OnBringOneToTopClick(object? sender, EventArgs e) {
    BringOneToTopClick?.Invoke(this, e);
  }

  private void OnBringSecondToTopClick(object? sender, EventArgs e) {
    BringSecondToTopClick?.Invoke(this, e);
  }

  private void OnSwitchClick(object? sender, EventArgs e) {
    SwitchClick?.Invoke(this, e);
  }

  private void OnExitClick(object? sender, EventArgs e) {
    ExitClick?.Invoke(this, e);
  }

  private void OnAboutClick(object? sender, EventArgs e) {
    AboutClick?.Invoke(this, e);
  }

  private void OnCheckUpdateClick(object? sender, EventArgs e) {
    CheckUpdateClick?.Invoke(this, e);
  }

  private void OnAutoPositionToggleClick(object? sender, EventArgs e) {
    try {
      if (sender is ToolStripMenuItem menuItem) {
        // Update the setting
        SettingsManager.SetAutoPositionEnabled(menuItem.Checked);
        
        // Update tooltip to reflect the change
        UpdateTrayIconTooltip();
      }
    } catch (Exception ex) {
      MessageBox.Show($"Failed to update auto-positioning setting: {ex.Message}", 
                      "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }

  private void OnSettingsClick(object? sender, EventArgs e) {
    SettingsClick?.Invoke(this, e);
  }

  /// <summary>
  /// Updates the tray icon tooltip to reflect the current auto-positioning status.
  /// </summary>
  private void UpdateTrayIconTooltip() {
    try {
      bool autoPositionEnabled = SettingsManager.GetAutoPositionEnabled();
      string baseTooltip = "Displayed App Switcher";
      string statusTooltip = autoPositionEnabled ? 
        $"{baseTooltip}\nAuto-positioning: Enabled" : 
        $"{baseTooltip}\nAuto-positioning: Disabled";
      
      if (trayIcon != null) {
        trayIcon.Text = statusTooltip;
      }
    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error updating tray icon tooltip: {ex.Message}");
    }
  }

}

public class BrintToTopEventArgs : EventArgs {
  int index;
  public BrintToTopEventArgs(int index) => this.index = index;

}


