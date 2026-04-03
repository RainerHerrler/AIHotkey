using System.Windows.Forms;

namespace AIHotkey;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new TrayApplicationContext());
        }
        catch (Exception ex)
        {
            var logger = new AppLogger();
            logger.Error("Unhandled startup error.", ex);
            MessageBox.Show(
                ex.ToString(),
                "AIHotkey startup failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
