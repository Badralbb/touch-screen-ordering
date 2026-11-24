namespace TouchScreenOrdering;

internal static class Program
{
    /// <summary>
    /// Starts the kiosk. Pass --kiosk to start full screen (F11 toggles, Esc leaves full screen).
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        int shots = Array.FindIndex(args, a => a.Equals("--screenshots", StringComparison.OrdinalIgnoreCase));
        if (shots >= 0 && shots + 1 < args.Length)
        {
            MainForm.SaveScreenshots(args[shots + 1]);
            return;
        }

        bool kiosk = args.Any(a => a.Equals("--kiosk", StringComparison.OrdinalIgnoreCase));
        Application.Run(new MainForm(kiosk));
    }
}
