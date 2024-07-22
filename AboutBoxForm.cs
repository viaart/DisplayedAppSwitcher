using System.Diagnostics;
using System.Reflection;

namespace DisplayedAppSwitcher;
partial class AboutBoxForm : Form {
  public AboutBoxForm(string newVersionAvailable) {
    InitializeComponent();
    this.Text = String.Format("{0} {1}", AssemblyTitle, ShortAssemblyVersion);
    this.labelProductName.Text = AssemblyProduct;
    if (newVersionAvailable == "") {
      this.labelVersion.Text = String.Format("Version {0}", ShortAssemblyVersion);
      this.labelCopyright.Text = AssemblyCopyright;
    } else {
      this.labelVersion.Text = String.Format("Update {1} is available! Click the link below to download it:", ShortAssemblyVersion, newVersionAvailable);
      this.labelCopyright.Text = AssemblyCopyright+"/releases";
    }
    
    this.labelCompanyName.Text = AssemblyCompany;
    this.textBoxDescription.Text = AssemblyDescription;
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
    CenterToScreen();
  }

  #region Assembly Attribute Accessors

  public static string AssemblyTitle {
    get {
      object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
      if (attributes.Length > 0) {
        AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
        if (titleAttribute.Title != "") {
          return titleAttribute.Title;
        }
      }
      return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location ?? "");
    }
  }

  public static string ShortAssemblyVersion {
    get {
      try {
        var parsed = Version.Parse(AssemblyVersion);
        return $"{parsed.Major}.{parsed.Minor}.{parsed.Revision}";
      } catch {
        return "0.0.0";
      }
      // return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
    }
  }

  public static string AssemblyVersion {
    get {
      return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
    }
  }


  public static string AssemblyDescription {
    get {
      object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
      if (attributes.Length == 0) {
        return "";
      }
      return ((AssemblyDescriptionAttribute)attributes[0]).Description;
    }
  }

  public static string AssemblyProduct {
    get {
      object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
      if (attributes.Length == 0) {
        return "";
      }
      return ((AssemblyProductAttribute)attributes[0]).Product;
    }
  }

  public static string AssemblyCopyright {
    get {
      object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
      if (attributes.Length == 0) {
        return "";
      }
      return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
    }
  }

  public static string AssemblyCompany {
    get {
      object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
      if (attributes.Length == 0) {
        return "";
      }
      return ((AssemblyCompanyAttribute)attributes[0]).Company;
    }
  }
  #endregion

  private void labelCopyright_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
    try {
      Process.Start(new ProcessStartInfo(((LinkLabel)sender).Text) { UseShellExecute = true });
      this.Close();
    } catch (Exception other) {
      MessageBox.Show(other.Message);
    }
  }
}
