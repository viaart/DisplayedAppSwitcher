using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;

namespace DisplayedAppSwitcher.Services;

/// <summary>
/// Reports when JW Library's secondary window has been minimized, so the caller can
/// bring it straight back.
///
/// On some machines the secondary window minimizes itself when a Zoom screen share
/// starts or stops, leaving the shared monitor showing the desktop until someone taps
/// F9. It does not happen everywhere. A window event log taken on a machine that is not
/// affected (Zoom 7.1.9 and 7.2.5, JW Library 15.10.9) recorded six share starts and six
/// stops without a single minimize. The same log showed Zoom covering every monitor with
/// full screen topmost windows while the share picker is open, and covering the shared
/// monitor while sharing, which is the most likely trigger on the machines that are.
///
/// The window cannot be made unminimizable from outside its own process. Removing
/// WS_MINIMIZEBOX only takes the button away; ShowWindow(SW_MINIMIZE) from any process
/// still works. Refusing a minimize outright needs a WH_CBT hook running inside
/// ApplicationFrameHost.exe, which means injecting a native DLL into a system process.
/// Reacting to the minimize as it happens is the safe equivalent.
///
/// This class only detects. Whether to restore is left to the caller, which knows
/// whether JW Library is the application the user currently wants on screen. That
/// matters because switching to Zoom minimizes this window on purpose, and those
/// minimizes arrive here as well.
/// </summary>
public sealed class JWLibraryMinimizeWatcher : IDisposable {

  // WinEvent ids for the window itself rather than one of its parts.
  private const int OBJID_WINDOW = 0;
  private const int CHILDID_SELF = 0;

  // Long enough to fold a burst of minimize events into one report, and to let the
  // check below see the settled state, so a minimize that something else undoes on
  // its own is left alone. Short enough that the shared screen is barely disturbed.
  private const int SettleMilliseconds = 100;

  private readonly ISwitcher jwLibrarySwitcher;

  // Held in a field so the delegate outlives the native hook that calls it.
  private readonly WINEVENTPROC winEventProc;

  private readonly System.Windows.Forms.Timer settleTimer;
  private UnhookWinEventSafeHandle? hook;
  private HWND pendingWindow;
  private bool disposed;

  /// <summary>
  /// Raised on the UI thread when JW Library's secondary window has been minimized and
  /// is still minimized a moment later.
  /// </summary>
  public event EventHandler? SecondaryWindowMinimized;

  /// <param name="jwLibrarySwitcher">
  /// The switcher used by F9, so this recognises exactly the window that F9 restores.
  /// </param>
  public JWLibraryMinimizeWatcher(ISwitcher jwLibrarySwitcher) {
    this.jwLibrarySwitcher = jwLibrarySwitcher;
    winEventProc = OnWinEvent;
    settleTimer = new System.Windows.Forms.Timer { Interval = SettleMilliseconds };
    settleTimer.Tick += OnSettled;
  }

  /// <summary>
  /// Starts listening. Must be called on the UI thread: an out of context hook delivers
  /// its events through the message loop of the thread that installed it, which is also
  /// the only thread allowed to remove it.
  /// </summary>
  public void Start() {
    if (hook != null && !hook.IsInvalid) {
      return;
    }
    hook = PInvoke.SetWinEventHook(
      PInvoke.EVENT_SYSTEM_MINIMIZESTART, PInvoke.EVENT_SYSTEM_MINIMIZESTART,
      (SafeHandle?)null, winEventProc, 0, 0,
      PInvoke.WINEVENT_OUTOFCONTEXT | PInvoke.WINEVENT_SKIPOWNPROCESS);
    if (hook.IsInvalid) {
      System.Diagnostics.Debug.WriteLine("JWLibraryMinimizeWatcher: SetWinEventHook failed");
    }
  }

  public void Stop() {
    settleTimer.Stop();
    pendingWindow = HWND.Null;
    hook?.Dispose();
    hook = null;
  }

  private void OnWinEvent(HWINEVENTHOOK hWinEventHook, uint @event, HWND hwnd,
      int idObject, int idChild, uint idEventThread, uint dwmsEventTime) {
    try {
      if (hwnd.IsNull || idObject != OBJID_WINDOW || idChild != CHILDID_SELF) {
        return;
      }

      // Every minimize on the system arrives here, so filter on the cheap title check
      // inside IsTheRightWindow before anything else happens.
      var info = Win32Helpers.GetWindowInfo(hwnd);
      if (!jwLibrarySwitcher.IsTheRightWindow(info, Purpose.Show)) {
        return;
      }

      pendingWindow = hwnd;
      settleTimer.Stop();
      settleTimer.Start();
    } catch (Exception ex) {
      // Never let a hook callback take the tray application down.
      System.Diagnostics.Debug.WriteLine($"Error in JWLibraryMinimizeWatcher: {ex.Message}");
    }
  }

  private void OnSettled(object? sender, EventArgs e) {
    settleTimer.Stop();
    var hwnd = pendingWindow;
    pendingWindow = HWND.Null;

    try {
      // Nothing to do if the window has gone, or something has already brought it back.
      if (hwnd.IsNull || !PInvoke.IsWindow(hwnd) || !PInvoke.IsIconic(hwnd)) {
        return;
      }

      System.Diagnostics.Debug.WriteLine($"JW Library secondary window minimized 0x{(IntPtr)hwnd:X}");
      SecondaryWindowMinimized?.Invoke(this, EventArgs.Empty);
    } catch (Exception ex) {
      System.Diagnostics.Debug.WriteLine($"Error in JWLibraryMinimizeWatcher: {ex.Message}");
    }
  }

  public void Dispose() {
    if (disposed) {
      return;
    }
    disposed = true;
    Stop();
    settleTimer.Tick -= OnSettled;
    settleTimer.Dispose();
  }
}
