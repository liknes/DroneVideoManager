using System;

namespace DroneVideoManager.Core.Services
{
    public interface ILoggingService
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception exception = null);
        void LogDebug(string message);
    }
} 