using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace DisplayedAppSwitcher {
  public class HotKeys {
    public const int switchHotKeyId = 100;
    public const int oneHotKeyId = 101;
    public const int oneHotKeySubId = 102;
    public const int secondHotKeyId = 102;
    public const int secondHotKeySubId = 103;

    public static void Register(IntPtr handle) {
      var h = new HWND(handle);
      _ = PInvoke.RegisterHotKey(h, HotKeys.switchHotKeyId,
        HOT_KEY_MODIFIERS.MOD_CONTROL,
        (uint)Keys.NumPad0.GetHashCode());
      _ = PInvoke.RegisterHotKey(h, HotKeys.oneHotKeyId,
        HOT_KEY_MODIFIERS.MOD_CONTROL,
        (uint)Keys.NumPad1.GetHashCode());
      _ = PInvoke.RegisterHotKey(h, HotKeys.oneHotKeySubId,
        0,
        (uint)Keys.F5.GetHashCode());
      _ = PInvoke.RegisterHotKey(h, HotKeys.secondHotKeyId,
        HOT_KEY_MODIFIERS.MOD_CONTROL,
        (uint)Keys.NumPad2.GetHashCode());
      _ = PInvoke.RegisterHotKey(h, HotKeys.secondHotKeySubId,
        0,
        (uint)Keys.F6.GetHashCode());
    }

    public static void Unresister(IntPtr handle) {
      var h = new HWND(handle);
      PInvoke.UnregisterHotKey(h, HotKeys.switchHotKeyId);
      PInvoke.UnregisterHotKey(h, HotKeys.oneHotKeyId);
      PInvoke.UnregisterHotKey(h, HotKeys.oneHotKeySubId);
      PInvoke.UnregisterHotKey(h, HotKeys.secondHotKeyId);
      PInvoke.UnregisterHotKey(h, HotKeys.secondHotKeySubId);
    }
  }

}