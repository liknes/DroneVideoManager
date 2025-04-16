using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DroneVideoManager.Core.Models;
using DroneVideoManager.Core.Services;
using DroneVideoManager.Data;
using Microsoft.EntityFrameworkCore;

namespace DroneVideoManager.Services
{
    public class FolderService : IFolderService
    {
        private readonly DroneVideoDbContext _dbContext;
        private readonly IVideoFileService _videoFileService;
        private readonly ILoggingService _loggingService;
        private readonly List<Folder> _folders = new();
        private int _nextId = 1;

        public FolderService(
            DroneVideoDbContext dbContext, 
            IVideoFileService videoFileService,
            ILoggingService loggingService)
        {
            _dbContext = dbContext;
            _videoFileService = videoFileService;
            _loggingService = loggingService;
        }

        public async Task<IEnumerable<Folder>> GetAllFoldersAsync()
        {
            return await _dbContext.Folders
                .Include(f => f.SubFolders)
                .Include(f => f.Videos)
                .ToListAsync();
        }

        public async Task<Folder> ImportFolderAsync(string folderPath, IProgress<(int Processed, int Total)>? progress = null)
        {
            try
            {
                _loggingService.LogInformation($"Starting folder import: {folderPath}");
                
                var directoryInfo = new DirectoryInfo(folderPath);
                if (!directoryInfo.Exists)
                {
                    _loggingService.LogError($"Directory not found: {folderPath}");
                    throw new DirectoryNotFoundException($"Directory not found: {folderPath}");
                }

                var folder = new Folder
                {
                    Name = directoryInfo.Name,
                    Path = folderPath,
                    CreatedDate = DateTime.Now,
                    LastScannedDate = DateTime.Now,
                    IsWatched = false
                };

                _dbContext.Folders.Add(folder);
                await _dbContext.SaveChangesAsync();

                var videoExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
                    { ".mp4", ".mov", ".avi", ".mkv" };

                // Get all files but exclude those in Proxy folders
                var files = Directory.EnumerateFiles(folder.Path, "*.*", SearchOption.AllDirectories)
                    .Where(f => 
                        !f.Contains(Path.DirectorySeparatorChar + "Proxy" + Path.DirectorySeparatorChar) && // Exclude files in Proxy folders
                        !f.Contains(Path.DirectorySeparatorChar + "Proxy\\") && // Additional check for Windows paths
                        !f.Contains("/Proxy/") && // Additional check for forward slashes
                        videoExtensions.Contains(Path.GetExtension(f)))
                    .ToList();

                _loggingService.LogInformation($"Found {files.Count} video files (excluding Proxy folder contents)");

                int processedCount = 0;
                int totalCount = files.Count;
                
                progress?.Report((0, totalCount));

                foreach (var filePath in files)
                {
                    try
                    {
                        var video = await _videoFileService.ImportVideoFileAsync(filePath);
                        if (video != null && video.FolderId != folder.Id)
                        {
                            video.FolderId = folder.Id;
                            _dbContext.VideoFiles.Update(video);
                            await _dbContext.SaveChangesAsync();
                        }
                        processedCount++;
                        progress?.Report((processedCount, totalCount));
                        _loggingService.LogInformation($"Processed {processedCount}/{totalCount}: {Path.GetFileName(filePath)}");
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError($"Error processing file: {filePath}", ex);
                    }
                }

                folder.LastScannedDate = DateTime.Now;
                await _dbContext.SaveChangesAsync();
                _loggingService.LogInformation($"Completed folder scan. Processed {processedCount} files.");

                return folder;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error importing folder: {folderPath}", ex);
                throw;
            }
        }

        public async Task<Folder> GetFolderByIdAsync(int folderId)
        {
            return await _dbContext.Folders
                .Include(f => f.SubFolders)
                .Include(f => f.Videos)
                .FirstOrDefaultAsync(f => f.Id == folderId);
        }

        public async Task<IEnumerable<Folder>> GetSubFoldersAsync(int parentFolderId)
        {
            return await _dbContext.Folders
                .Include(f => f.Videos)
                .Where(f => f.ParentFolderId == parentFolderId)
                .ToListAsync();
        }

        public async Task<Folder> GetRootFolderAsync()
        {
            return await _dbContext.Folders
                .Include(f => f.SubFolders)
                .Include(f => f.Videos)
                .FirstOrDefaultAsync(f => f.ParentFolderId == null);
        }

        public async Task<Folder> UpdateFolderAsync(Folder folder)
        {
            _dbContext.Entry(folder).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return folder;
        }

        public async Task DeleteFolderAsync(int folderId)
        {
            var folder = await _dbContext.Folders
                .Include(f => f.SubFolders)
                .Include(f => f.Videos)
                .FirstOrDefaultAsync(f => f.Id == folderId);

            if (folder != null)
            {
                // Recursively delete subfolders
                foreach (var subFolder in folder.SubFolders.ToList())
                {
                    await DeleteFolderAsync(subFolder.Id);
                }

                // Remove folder from database (but don't delete actual files)
                _dbContext.Folders.Remove(folder);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Folder>> SearchFoldersAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllFoldersAsync();

            return await _dbContext.Folders
                .Include(f => f.SubFolders)
                .Include(f => f.Videos)
                .Where(f => f.Name.Contains(searchText) || f.Path.Contains(searchText))
                .ToListAsync();
        }

        public async Task ScanFolderForChangesAsync(int folderId)
        {
            var folder = await GetFolderByIdAsync(folderId);
            if (folder == null) return;

            var directoryInfo = new DirectoryInfo(folder.Path);
            if (!directoryInfo.Exists) return;

            var videoFiles = directoryInfo.GetFiles("*.*")
                .Where(f => IsVideoFile(f.Extension));

            foreach (var videoFile in videoFiles)
            {
                await _videoFileService.ImportVideoFileAsync(videoFile.FullName);
            }

            folder.LastScannedDate = DateTime.Now;
        }

        private bool IsVideoFile(string extension)
        {
            var videoExtensions = new[] { ".mp4", ".mov", ".avi", ".mkv" };
            return videoExtensions.Contains(extension.ToLower());
        }

        private async Task ScanFolderForVideosAsync(int folderId)
        {
            try
            {
                var folder = await _dbContext.Folders.FindAsync(folderId);
                if (folder == null)
                {
                    _loggingService.LogError($"Folder not found for scanning: {folderId}");
                    return;
                }

                _loggingService.LogInformation($"Scanning folder: {folder.Path}");

                var videoExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
                    { ".mp4", ".mov", ".avi", ".mkv" };

                var files = Directory.EnumerateFiles(folder.Path, "*.*", SearchOption.AllDirectories)
                    .Where(f => videoExtensions.Contains(Path.GetExtension(f)))
                    .ToList();

                _loggingService.LogInformation($"Found {files.Count} video files");

                int processedCount = 0;
                foreach (var filePath in files)
                {
                    try
                    {
                        var video = await _videoFileService.ImportVideoFileAsync(filePath);
                        if (video != null && video.FolderId != folderId)
                        {
                            video.FolderId = folderId;
                            _dbContext.VideoFiles.Update(video);
                            await _dbContext.SaveChangesAsync();
                        }
                        processedCount++;
                        _loggingService.LogInformation($"Processed {processedCount}/{files.Count}: {Path.GetFileName(filePath)}");
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError($"Error processing file: {filePath}", ex);
                        // Continue with next file
                    }
                }

                folder.LastScannedDate = DateTime.Now;
                await _dbContext.SaveChangesAsync();
                _loggingService.LogInformation($"Completed folder scan. Processed {processedCount} files.");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error scanning folder: {folderId}", ex);
                throw;
            }
        }
    }
} 