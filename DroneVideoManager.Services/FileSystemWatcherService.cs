using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using DroneVideoManager.Core.Services;

namespace DroneVideoManager.Services
{
    public class FileSystemWatcherService : IFileSystemWatcherService, IDisposable
    {
        private readonly ConcurrentDictionary<string, FileSystemWatcher> _watchers = new();
        private readonly ILoggingService _loggingService;
        private readonly IVideoFileService _videoFileService;

        public FileSystemWatcherService(
            ILoggingService loggingService,
            IVideoFileService videoFileService)
        {
            _loggingService = loggingService;
            _videoFileService = videoFileService;
        }

        public Task StartWatchingFolderAsync(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                _loggingService.LogError($"Directory not found: {folderPath}");
                return Task.CompletedTask;
            }

            if (_watchers.ContainsKey(folderPath))
            {
                _loggingService.LogWarning($"Already watching folder: {folderPath}");
                return Task.CompletedTask;
            }

            var watcher = new FileSystemWatcher(folderPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.mp4;*.mov;*.avi;*.mkv",
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };

            watcher.Created += OnFileCreated;
            watcher.Changed += OnFileChanged;
            watcher.Deleted += OnFileDeleted;
            watcher.Renamed += OnFileRenamed;
            watcher.Error += OnError;

            _watchers.TryAdd(folderPath, watcher);
            _loggingService.LogInformation($"Started watching folder: {folderPath}");

            return Task.CompletedTask;
        }

        public Task StopWatchingFolderAsync(string folderPath)
        {
            if (_watchers.TryRemove(folderPath, out var watcher))
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                _loggingService.LogInformation($"Stopped watching folder: {folderPath}");
            }

            return Task.CompletedTask;
        }

        public async Task StopWatchingAllFoldersAsync()
        {
            foreach (var folderPath in _watchers.Keys)
            {
                await StopWatchingFolderAsync(folderPath);
            }
        }

        public bool IsWatching(string folderPath)
        {
            return _watchers.ContainsKey(folderPath);
        }

        private async void OnFileCreated(object sender, System.IO.FileSystemEventArgs e)
        {
            try
            {
                _loggingService.LogInformation($"New file detected: {e.FullPath}");
                await _videoFileService.ImportVideoFileAsync(e.FullPath);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error processing new file: {e.FullPath}", ex);
            }
        }

        private void OnFileChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            _loggingService.LogInformation($"File changed: {e.FullPath}");
        }

        private void OnFileDeleted(object sender, System.IO.FileSystemEventArgs e)
        {
            _loggingService.LogInformation($"File deleted: {e.FullPath}");
        }

        private void OnFileRenamed(object sender, System.IO.RenamedEventArgs e)
        {
            _loggingService.LogInformation($"File renamed from {e.OldFullPath} to {e.FullPath}");
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            _loggingService.LogError("File system watcher error", e.GetException());
        }

        public void Dispose()
        {
            foreach (var watcher in _watchers.Values)
            {
                watcher.Dispose();
            }
            _watchers.Clear();
        }
    }
} 