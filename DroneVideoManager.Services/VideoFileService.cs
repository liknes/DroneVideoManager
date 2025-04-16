using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DroneVideoManager.Core.Models;
using DroneVideoManager.Core.Services;
using DroneVideoManager.Data;
using Microsoft.EntityFrameworkCore;
using TagLib;
using File = TagLib.File;
using IOFile = System.IO.File;

namespace DroneVideoManager.Services
{
    public class VideoFileService : IVideoFileService
    {
        private readonly DroneVideoDbContext _dbContext;
        private readonly List<VideoFile> _videos = new();
        private int _nextId = 1;
        private readonly ILoggingService _loggingService;
        private readonly IVideoMetadataService _metadataService;

        public VideoFileService(
            DroneVideoDbContext dbContext, 
            ILoggingService loggingService,
            IVideoMetadataService metadataService)
        {
            _dbContext = dbContext;
            _loggingService = loggingService;
            _metadataService = metadataService;
        }

        public async Task<IEnumerable<VideoFile>> GetRecentVideosAsync(int count)
        {
            return await _dbContext.VideoFiles
                .Include(v => v.Tags)
                .Include(v => v.DroneMetadata)
                .OrderByDescending(v => v.ImportedDate)
                .Take(count)
                .ToListAsync();
        }

        private string GetFileIdentifier(FileInfo fileInfo)
        {
            return $"{fileInfo.Length}_{fileInfo.LastWriteTimeUtc.Ticks}_{fileInfo.Name}";
        }

        public async Task<VideoFile> ImportVideoFileAsync(string filePath)
        {
            try
            {
                _loggingService.LogInformation($"Starting import of video file: {filePath}");
                
                var fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                {
                    _loggingService.LogError($"File not found: {filePath}");
                    throw new FileNotFoundException("Video file not found", filePath);
                }

                // Quick check using file size and modification date
                string fileIdentifier = GetFileIdentifier(fileInfo);
                _loggingService.LogDebug($"Generated file identifier: {fileIdentifier}");

                var existingVideo = await _dbContext.VideoFiles
                    .FirstOrDefaultAsync(v => v.FileHash == fileIdentifier);

                if (existingVideo != null)
                {
                    _loggingService.LogInformation($"Found existing video with identifier: {fileIdentifier}");
                    return existingVideo;
                }

                _loggingService.LogInformation("Creating new video entry");
                var video = new VideoFile
                {
                    FileName = fileInfo.Name,
                    FilePath = filePath,
                    FileSize = Math.Round(fileInfo.Length / (1024.0 * 1024.0), 2),
                    CreatedDate = fileInfo.CreationTime,
                    ImportedDate = DateTime.Now,
                    FileHash = fileIdentifier,
                    Width = 0,
                    Height = 0,
                    Duration = TimeSpan.Zero,
                    ColorSpace = "Unknown",  // Add default value for ColorSpace
                    FramesPerSecond = 0     // Add default value if this is also non-nullable
                };

                try
                {
                    _loggingService.LogDebug("Adding video to database");
                    _dbContext.VideoFiles.Add(video);
                    await _dbContext.SaveChangesAsync();
                    _loggingService.LogInformation($"Successfully saved video to database with ID: {video.Id}");
                }
                catch (Exception ex)
                {
                    _loggingService.LogError("Error saving video to database", ex);
                    throw new Exception("Failed to save video to database", ex);
                }

                // After saving the video file, extract metadata
                await _metadataService.ExtractAndSaveMetadataAsync(video);

                return video;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error importing video file: {filePath}", ex);
                throw;
            }
        }

        private async Task ExtractMetadataAsync(int videoId)
        {
            try
            {
                var video = await _dbContext.VideoFiles.FindAsync(videoId);
                if (video == null) return;

                using (var file = File.Create(video.FilePath))
                {
                    var videoProperties = file.Properties;
                    video.Duration = videoProperties.Duration;
                    if (videoProperties is TagLib.Properties properties)
                    {
                        video.Width = properties.VideoWidth;
                        video.Height = properties.VideoHeight;
                    }
                }

                // Extract metadata using the VideoMetadataService
                await _metadataService.ExtractAndSaveMetadataAsync(video);
            }
            catch (Exception)
            {
                // Log error but don't throw - this is background processing
            }
        }

        public async Task<IEnumerable<VideoFile>> SearchVideosAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetRecentVideosAsync(100);

            return await _dbContext.VideoFiles
                .Include(v => v.Tags)
                .Include(v => v.DroneMetadata)
                .Where(v => v.FileName.Contains(searchText) ||
                           v.Description.Contains(searchText) ||
                           v.Tags.Any(t => t.Tag.Name.Contains(searchText)))
                .OrderByDescending(v => v.ImportedDate)
                .ToListAsync();
        }

        public async Task<VideoFile> UpdateVideoAsync(VideoFile video)
        {
            _dbContext.Entry(video).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return video;
        }

        public async Task DeleteVideoAsync(int videoId)
        {
            var video = await _dbContext.VideoFiles.FindAsync(videoId);
            if (video != null)
            {
                _dbContext.VideoFiles.Remove(video);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<VideoFile>> GetVideosByFolderAsync(int folderId)
        {
            return await _dbContext.VideoFiles
                .Include(v => v.Tags)
                .Include(v => v.DroneMetadata)
                .Where(v => v.FolderId == folderId)
                .OrderByDescending(v => v.ImportedDate)
                .ToListAsync();
        }

        public async Task<VideoFile> GetVideoByIdAsync(int videoId)
        {
            return await _dbContext.VideoFiles
                .Include(v => v.Tags)
                .Include(v => v.DroneMetadata)
                .FirstOrDefaultAsync(v => v.Id == videoId);
        }

        public async Task<IEnumerable<VideoFile>> GetVideosByProjectAsync(int projectId)
        {
            return await _dbContext.VideoFiles
                .Include(v => v.Tags)
                .Include(v => v.DroneMetadata)
                .Where(v => v.Projects.Any(p => p.ProjectId == projectId))
                .OrderByDescending(v => v.ImportedDate)
                .ToListAsync();
        }
    }
}