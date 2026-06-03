using System;
using System.IO;
using System.Text;

namespace VelocityLite.Controls
{
    internal static class Logger
    {
        private static readonly string LogDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Velocity Ui");

        private static readonly string LogFile =
            Path.Combine(LogDir, "tab_session_log.txt");

        private static readonly object _lock = new();

        public static void Log(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                Directory.CreateDirectory(LogDir);

                string logEntry =
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}{Environment.NewLine}";

                lock (_lock)
                {
                    File.AppendAllText(LogFile, logEntry, Encoding.UTF8);
                }
            }
            catch
            {
                // intentionally ignored (avoid crashing UI thread)
            }
        }

        public static void LogException(Exception ex, string context = "")
        {
            Log($"[ERROR] {context} {ex.GetType().Name}: {ex.Message}");
        }

        public static void LogInfo(string message)
        {
            Log($"[INFO] {message}");
        }

        public static void LogWarning(string message)
        {
            Log($"[WARN] {message}");
        }
    }
}