using System;
using System.IO;

namespace InspectionAI.Classes.Managers
{
    /// <summary>
    /// Application logger untuk error tracking & audit trail
    /// </summary>
    public static class Logger
    {
        private static string logFolder = "Logs";
        private static object lockObj = new object();

        static Logger()
        {
            // Create logs folder
            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);
        }

        /// <summary>
        /// Log error message
        /// </summary>
        public static void LogError(string message, Exception ex = null)
        {
            Log("ERROR", message, ex);
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public static void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        /// <summary>
        /// Log info message
        /// </summary>
        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        /// <summary>
        /// Log audit event
        /// </summary>
        public static void LogAudit(string action, string user, string details)
        {
            string message = $"Action: {action} | User: {user} | Details: {details}";
            Log("AUDIT", message);
        }

        /// <summary>
        /// Main log method
        /// </summary>
        private static void Log(string level, string message, Exception ex = null)
        {
            try
            {
                lock (lockObj)
                {
                    string logFile = Path.Combine(logFolder, $"log_{DateTime.Now:yyyyMMdd}.txt");
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    string logEntry = $"[{timestamp}] [{level}] {message}";

                    if (ex != null)
                    {
                        logEntry += $"\nException: {ex.Message}\nStack Trace: {ex.StackTrace}";
                    }

                    File.AppendAllText(logFile, logEntry + Environment.NewLine);

                    // Also write to debug output
                    System.Diagnostics.Debug.WriteLine(logEntry);
                }
            }
            catch
            {
                // Don't throw exception in logger
            }
        }

        /// <summary>
        /// Get today's log file path
        /// </summary>
        public static string GetTodayLogFile()
        {
            return Path.Combine(logFolder, $"log_{DateTime.Now:yyyyMMdd}.txt");
        }

        /// <summary>
        /// Clean old log files (keep last N days)
        /// </summary>
        public static void CleanOldLogs(int keepDays = 30)
        {
            try
            {
                string[] files = Directory.GetFiles(logFolder, "log_*.txt");
                DateTime cutoff = DateTime.Now.AddDays(-keepDays);

                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.CreationTime < cutoff)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}