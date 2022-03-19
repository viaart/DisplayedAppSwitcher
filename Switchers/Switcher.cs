using Windows.Win32;

namespace DisplayedAppSwitcher {

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

  }

}
