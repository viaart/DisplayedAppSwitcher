namespace DisplayedAppSwitcher {
  internal static class Program {
    static Mutex mutex = new Mutex(true, "{E6DA3870-D05C-49A7-874D-0A3E6A121F38}");

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
      if (mutex.WaitOne(TimeSpan.Zero, true)) {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        var mainForm = new SettingsForm();
        var applicationContext = new ApplicationContext(mainForm);
        Application.Run(applicationContext);
        mutex.ReleaseMutex();
      }
    }
  }
}