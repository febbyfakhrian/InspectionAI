using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InspectionAI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Add this:
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    SetProcessDPIAware();
                }

                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                // DETAILED ERROR MESSAGE
                MessageBox.Show(
                    $"STARTUP ERROR:\n\n" +
                    $"Message: {ex.Message}\n\n" +
                    $"Source: {ex.Source}\n\n" +
                    $"StackTrace:\n{ex.StackTrace}\n\n" +
                    $"Inner: {ex.InnerException?.Message}",
                    "Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        
        //private static extern bool SetProcessDPIAware();
    }
}
