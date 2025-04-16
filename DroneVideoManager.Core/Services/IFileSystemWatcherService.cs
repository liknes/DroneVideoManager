using System;
using System.Threading.Tasks;

namespace DroneVideoManager.Core.Services
{
    public interface IFileSystemWatcherService
    {
        Task StartWatchingFolderAsync(string folderPath);
        Task StopWatchingFolderAsync(string folderPath);
        Task StopWatchingAllFoldersAsync();
        bool IsWatching(string folderPath);
    }

    public class FileSystemEventArgs : EventArgs
    {
        public string FilePath { get; }
        public FileChangeType ChangeType { get; }

        public FileSystemEventArgs(string filePath, FileChangeType changeType)
        {
            FilePath = filePath;
            ChangeType = changeType;
        }
    }

    public enum FileChangeType
    {
        Created,
        Modified,
        Deleted,
        Renamed
    }
} 