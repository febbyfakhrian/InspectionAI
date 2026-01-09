using System;
using System.Windows.Forms;

namespace InspectionAI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // Setup application
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Enable DPI awareness
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    SetProcessDPIAware();
                }

                // Create and run main form
                using (var mainForm = new MainForm())
                {
                    Application.Run(mainForm);
                }
            }
            catch (Exception ex)
            {
                // Show detailed error
                string errorMessage =
                    "APPLICATION STARTUP ERROR\n\n" +
                    "Message: " + ex.Message + "\n\n" +
                    "Type: " + ex.GetType().Name + "\n\n";

                if (ex.InnerException != null)
                {
                    errorMessage += "Inner Exception: " + ex.InnerException.Message + "\n\n";
                }

                errorMessage += "Stack Trace:\n" + ex.StackTrace;

                MessageBox.Show(
                    errorMessage,
                    "Fatal Error - InspectionAI",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                // Also write to file for debugging
                try
                {
                    string logPath = System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "startup_error.log"
                    );
                    System.IO.File.WriteAllText(logPath, errorMessage);
                }
                catch { }
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}