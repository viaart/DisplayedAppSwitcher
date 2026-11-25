using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;

namespace DisplayedAppSwitcher.Services;

/// <summary>
/// Detects a secondary Zoom meeting window that should not be on screen and reports it.
///
/// The rule is that the secondary window only appears when the user asks for it with
/// F10. While JW Library is the selected application, every appearance is reported,
/// including the same window coming back. Zoom does bring it back by itself: in a real
/// meeting it re-shows the hidden secondary window on the shared screen every time a
/// screen share stops. While Zoom is selected, the window F10 brought up is left alone
/// and only a newly created one is reported.
///
/// Version 1.7.0 and earlier remembered the last window seen and never reported it
/// again, whichever application was selected, and every switch recorded whatever was
/// visible at its end as wanted. The likely way the window was left on the shared
/// screen: a switch to JW Library ran while Zoom was re-showing it, the hide landed in
/// the middle of Zoom's own sequence, the window came back before that record was
/// taken, and from then on it was ignored until F9.
///
/// This class only detects. Acting on the detection is left to the caller, which
/// runs the same switch to JW Library that F9 performs. Hiding the Zoom window on
/// its own is not enough: switching to Zoom minimises the JW Library secondary
/// window, so hiding Zoom without restoring JW Library would leave the shared
/// monitor showing the desktop.
///
/// Zoom destroys and recreates its meeting windows on every meeting start, network
/// reconnect and crash recovery, so "a new window appeared" is the reliable signal.
/// Watching the process list is not: the window owning process is one of three
/// Zoom.exe processes, it is recycled at meeting END (when the windows are already
/// hidden and there is nothing to do) and it does NOT change at meeting START
/// (when the window becomes visible and hiding is actually wanted).
/// </summary>
public sealed class ZoomSecondaryWindowWatcher : IDisposable {

  // WinEvent ids for the window itself rather than one of its parts.
  private const int OBJID_WINDOW = 0;
  private const int CHILDID_SELF = 0;

  // Polling is the backstop. The window events below are what make the reaction
  // immediate, so the audience does not see the window for up to a poll interval.
  private readonly System.Windows.Forms.Timer timer;

  // Held in a field so the delegate outlives the native hooks that call it.
  private readonly WINEVENTPROC winEventProc;
  private UnhookWinEventSafeHandle? showHook;
  private UnhookWinEventSafeHandle? nameChangeHook;
  private SynchronizationContext? uiContext;
  private bool checkPending;
  private bool checking;

  // The last window this watcher reported, or the one F10 showed. While Zoom is
  // selected, this is what stops the watcher from fighting the user: F10 shows the
  // same handle again, so it is left alone. Ignored while JW Library is selected.
  private IntPtr lastHandledWindow = IntPtr.Zero;

  private bool disposed;

  /// <summary>
  /// Whether JW Library is the selected application. While it is, the secondary Zoom
  /// window must never be on screen, so every appearance is reported. Defaults to true
  /// because that is the application selected when this application starts.
  /// </summary>
  public bool JWLibrarySelected { get; set; } = true;

  /// <summary>
  /// Raised when a secondary Zoom meeting window that should not be on screen is visible.
  /// </summary>
  public event EventHandler? NewSecondaryWindowDetected;

  public ZoomSecondaryWindowWatcher(int pollIntervalMilliseconds = 500) {
    timer = new System.Windows.Forms.Timer {
      Interval = pollIntervalMilliseconds
    };
    timer.Tick += OnTick;
    winEventProc = OnWinEvent;
  }

  /// <summary>
  /// Starts watching. Any secondary window that is already visible counts as new,
  /// which is what handles the case of the application starting during a meeting.
  /// Must be called on the UI thread, which delivers the window events and is the
  /// only thread allowed to remove the hooks.
  /// </summary>
  public void Start() {
    uiContext = SynchronizationContext.Current;
    timer.Start();

    // Shown covers Zoom bringing a hidden window back, such as when a share stops.
    // A title change covers a meeting starting, where the window is shown while still
    // titled "Zoom Workplace" and only becomes "Zoom Meeting" afterwards.
    const uint flags = PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS;
    showHook ??= PInvoke.SetWinEventHook(PInvoke.EVENT_OBJECT_SHOW, PInvoke.EVENT_OBJECT_SHOW,
      (SafeHandle?)null, winEventProc, 0, 0, flags);
    nameChangeHook ??= PInvoke.SetWinEventHook(PInvoke.EVENT_OBJECT_NAMECHANGE, PInvoke.EVENT_OBJECT_NAMECHANGE,
      (SafeHandle?)null, winEventProc, 0, 0, flags);
  }

  public void Stop() {
    timer.Stop();
    showHook?.Dispose();
    showHook = null;
    nameChangeHook?.Dispose();
    nameChangeHook = null;
  }

  /// <summary>
  /// Treats the currently visible secondary window as already handled.
  /// Called when the user switches the option on so that a window they are
  /// looking at right now does not disappear from under them.
  /// </summary>
  public void SeedFromCurrentState() {
    lastHandledWindow = FindVisibleSecondaryMeetingWindow();
  }

  private void OnTick(object? sender, EventArgs e) {
    Check();
  }

  private void OnWinEvent(HWINEVENTHOOK hWinEventHook, uint @event, HWND hwnd,
      int idObject, int idChild, uint idEventThread, uint dwmsEventTime) {
    try {
      if (hwnd.IsNull || idObject != OBJID_WINDOW || idChild != CHILDID_SELF) {
        return;
      }
      // Every window on the system arrives here; the class name is the cheap filter.
      if (Win32Helpers.GetWindowInfo(hwnd).className != ZoomMeetingWindows.ClassName) {
        return;
      }

      // Check from the message loop rather than inside the callback. Acting here
      // would call into Zoom while Windows may still be delivering its events, and a
      // posted message is only handled once the current switch has finished.
      if (uiContext != null && !checkPending) {
        checkPending = true;
        uiContext.Post(_ => {
          checkPending = false;
          Check();
        }, null);
      }
    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error in ZoomSecondaryWindowWatcher: {ex.Message}");
    }
  }

  private void Check() {
    // A switch raised from here hides the window, which raises more events; they are
    // posted, so this only guards against a nested message loop.
    if (checking) {
      return;
    }
    checking = true;
    try {
      if (!SettingsManager.GetAutoHideZoomSecondaryWindowEnabled()) {
        return;
      }

      // Forget the remembered window once it is gone, so the next window Zoom
      // creates is recognised as new. Without this the feature would fire once
      // and then stay silent for the rest of the session.
      if (lastHandledWindow != IntPtr.Zero &&
          !PInvoke.IsWindow(new Windows.Win32.Foundation.HWND(lastHandledWindow))) {
        lastHandledWindow = IntPtr.Zero;
      }

      var hWnd = FindVisibleSecondaryMeetingWindow();
      if (hWnd == IntPtr.Zero) {
        return;
      }

      // While Zoom is selected, the window F10 brought up stays. While JW Library is
      // selected nothing is left alone: the window only appears on F10.
      if (!JWLibrarySelected && hWnd == lastHandledWindow) {
        return;
      }

      // Record before raising, so a slow handler cannot cause a second report of
      // the same window on the next tick.
      lastHandledWindow = hWnd;

      System.Diagnostics.Debug.WriteLine($"Secondary Zoom window to hide 0x{hWnd:X}");
      NewSecondaryWindowDetected?.Invoke(this, EventArgs.Empty);

    } catch (Exception ex) {
      // Never let a scan failure take the tray application down.
      System.Diagnostics.Debug.WriteLine($"Error in ZoomSecondaryWindowWatcher: {ex.Message}");
    } finally {
      checking = false;
    }
  }

  /// <summary>
  /// Finds the visible secondary Zoom meeting window, or IntPtr.Zero if there is none.
  ///
  /// This deliberately does not match "Zoom Workplace" windows the way the switch does.
  /// With no meeting running both of them look like the secondary window, and while a
  /// meeting comes up both are briefly visible under that title. Waiting for the meeting
  /// title, and for the control window to have its panel, means that by the time the
  /// switch runs it matches only the secondary window.
  /// </summary>
  private static IntPtr FindVisibleSecondaryMeetingWindow() {
    foreach (var hWnd in ZoomMeetingWindows.FindSecondaryWindows()) {
      if (PInvoke.IsWindowVisible(new Windows.Win32.Foundation.HWND(hWnd))) {
        return hWnd;
      }
    }
    return IntPtr.Zero;
  }

  public void Dispose() {
    if (disposed) {
      return;
    }
    disposed = true;
    Stop();
    timer.Tick -= OnTick;
    timer.Dispose();
  }
}
