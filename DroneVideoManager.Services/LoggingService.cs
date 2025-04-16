using System;
using System.IO;
using DroneVideoManager.Core.Services;

namespace DroneVideoManager.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();

        public LoggingService()
        {
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DroneVideoManager",
                "Logs");
            
            Directory.CreateDirectory(logDirectory);
            _logFilePath = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");
        }

        private void WriteLog(string level, string message, Exception exception = null)
        {
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
            if (exception != null)
            {
                logMessage += $"\nException: {exception.GetType().Name}\nMessage: {exception.Message}\nStack Trace: {exception.StackTrace}";
                if (exception.InnerException != null)
                {
                    logMessage += $"\nInner Exception: {exception.InnerException.Message}";
                }
            }

            Console.WriteLine(logMessage);

            lock (_lockObject)
            {
                File.AppendAllText(_logFilePath, logMessage + "\n");
            }
        }

        public void LogInformation(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogWarning(string message)
        {
            WriteLog("WARN", message);
        }

        public void LogError(string message, Exception exception = null)
        {
            WriteLog("ERROR", message, exception);
        }

        public void LogDebug(string message)
        {
            WriteLog("DEBUG", message);
        }
    }
} 