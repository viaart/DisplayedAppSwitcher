using System.ComponentModel;
using Windows.Win32;

namespace DisplayedAppSwitcher {

  public partial class SettingsForm : Form {

    public SettingsForm() {
      InitializeComponent();
      switchers.Add(new JWLibrarySwitcher());
      switchers.Add(new ZoomSwitcher());
      InitializeTrayIcon();
    }

    private readonly List<ISwitcher> switchers = new List<ISwitcher>();

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
    }

    protected override void OnDeactivate(EventArgs e) {
      base.OnDeactivate(e);
    }

    protected override void OnClosing(CancelEventArgs e) {
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
        AboutBoxForm b = new();
        b.ShowDialog();
      };
    }

    private int lastSwitcherIndex = 0;

    void DoSwitch(object? _, EventArgs __) {
      lastSwitcherIndex = (lastSwitcherIndex + 1) % switchers.Count;
      BringToTop((AppType)lastSwitcherIndex);
    }

    void BringToTop(AppType appTypeIndex) {
      var switcher = switchers[(int)appTypeIndex];
      // And remember the last one in case it's called aside of switch
      lastSwitcherIndex = (int)appTypeIndex;

      Windows.Win32.Foundation.HWND hWndParent = new(IntPtr.Zero);

      var hWnd = IntPtr.Zero;
      while (true) {
        hWnd = PInvoke.FindWindowEx(hWndParent, new Windows.Win32.Foundation.HWND(hWnd), null as string, null as string);
        if (hWnd != IntPtr.Zero) {
          var p = Win32Helpers.GetWindowInfo(hWnd);
          switchers.ForEach(s => {
            if (!s.Equals(switcher)) {
              if (s.IsTheRightWindow(p, Purpose.Hide)) {
                s.OtherSwitchedBefore(hWnd);
              }
            }
          });
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
          switchers.ForEach(s => {
            if (!s.Equals(switcher)) {
              if (s.IsTheRightWindow(p, Purpose.Hide)) {
                s.OtherSwitchedAfter(hWnd);
              }
            }
          });
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
      trayMenu = new ContextMenuStrip();
      trayMenu.Items.Add(new ToolStripMenuItem("JW Library", null, OnBringOneToTopClick));
      trayMenu.Items.Add(new ToolStripMenuItem("Zoom", null, OnBringSecondToTopClick));
      trayMenu.Items.Add(new ToolStripSeparator());
      trayMenu.Items.Add(new ToolStripMenuItem("Switch", null, OnSwitchClick));
      trayMenu.Items.Add(new ToolStripSeparator());
      trayMenu.Items.Add(new ToolStripMenuItem("About", null, OnAboutClick));
      trayMenu.Items.Add(new ToolStripSeparator());
      trayMenu.Items.Add(new ToolStripMenuItem("Exit", null, OnExitClick));

      trayIcon = new NotifyIcon {
        Text = "Displayed App Switcher",
        Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath),
        ContextMenuStrip = trayMenu,
        Visible = true,
      };
      trayIcon.DoubleClick += OnSwitchClick;
    }

    public void Dispose() {
      trayIcon.ContextMenuStrip.Dispose();
      trayIcon.ContextMenuStrip = null;
      trayIcon.Dispose();
    }

    public event EventHandler<EventArgs>? BringOneToTopClick;
    public event EventHandler<EventArgs>? BringSecondToTopClick;

    public event EventHandler<EventArgs>? SwitchClick;

    public event EventHandler<EventArgs>? AboutClick;

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

    private void OnSettingsClick(object? sender, EventArgs e) {
      SettingsClick?.Invoke(this, e);
    }


  }

  public class BrintToTopEventArgs : EventArgs {
    int index;
    public BrintToTopEventArgs(int index) => this.index = index;

  }

}