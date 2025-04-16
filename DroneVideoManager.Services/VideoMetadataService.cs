using System;
using System.Threading.Tasks;
using DroneVideoManager.Core.Models;
using DroneVideoManager.Core.Services;
using DroneVideoManager.Data;
using Microsoft.EntityFrameworkCore;
using FFMpegCore;
using FFMpegCore.Pipes;
using FFMpegCore.Enums;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DroneVideoManager.Services
{
    public class VideoMetadataService : IVideoMetadataService
    {
        private readonly DroneVideoDbContext _dbContext;
        private readonly ILoggingService _loggingService;

        public VideoMetadataService(DroneVideoDbContext dbContext, ILoggingService loggingService)
        {
            _dbContext = dbContext;
            _loggingService = loggingService;
        }

        public async Task ExtractAndSaveMetadataAsync(VideoFile videoFile)
        {
            try
            {
                _loggingService.LogInformation($"Extracting metadata for: {videoFile.FileName}");

                var mediaInfo = await FFProbe.AnalyseAsync(videoFile.FilePath);
                if (mediaInfo == null)
                {
                    _loggingService.LogError($"FFProbe analysis returned null for {videoFile.FileName}");
                    return;
                }

                var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
                if (videoStream == null)
                {
                    _loggingService.LogError($"No video stream found in {videoFile.FileName}");
                    return;
                }

                // Update VideoFile properties directly
                videoFile.Width = videoStream.Width;
                videoFile.Height = videoStream.Height;
                videoFile.Duration = TimeSpan.FromSeconds(mediaInfo.Duration.TotalSeconds);
                videoFile.ColorSpace = GetColorSpace(videoStream);
                videoFile.FramesPerSecond = videoStream.FrameRate;

                // Create VideoMetadata
                var metadata = new VideoMetadata
                {
                    VideoFileId = videoFile.Id,
                    BitRate = videoStream.BitRate,
                    ColorSpace = GetColorSpace(videoStream),
                    ColorDepth = GetColorDepth(videoStream),
                    VideoCodec = videoStream.CodecName ?? "Unknown",
                    IsVariableFrameRate = IsVariableFrameRate(videoStream),

                    AudioCodec = mediaInfo.AudioStreams.FirstOrDefault()?.CodecName ?? "Unknown",
                    AudioChannels = mediaInfo.AudioStreams.FirstOrDefault()?.Channels ?? 0,
                    AudioSampleRate = mediaInfo.AudioStreams.FirstOrDefault()?.SampleRateHz ?? 0,
                    AudioBitRate = mediaInfo.AudioStreams.FirstOrDefault()?.BitRate ?? 0,

                    CameraModel = ExtractCameraModel(mediaInfo),
                    CameraSettings = ExtractCameraSettings(mediaInfo),
                    Category = DetermineCategoryFromPath(videoFile.FilePath),
                    ExactCreationTime = File.GetCreationTime(videoFile.FilePath),
                    RecordingMode = DetermineRecordingMode(videoStream)
                };

                await SaveMetadataAsync(metadata);
                await _dbContext.SaveChangesAsync();
                
                _loggingService.LogInformation($"Successfully extracted metadata for {videoFile.FileName}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error extracting metadata for {videoFile.FileName}", ex);
                throw;
            }
        }

        public async Task<VideoMetadata?> GetMetadataAsync(int videoFileId)
        {
            return await _dbContext.Set<VideoMetadata>()
                .FirstOrDefaultAsync(m => m.VideoFileId == videoFileId);
        }

        public async Task UpdateMetadataAsync(VideoMetadata metadata)
        {
            _dbContext.Entry(metadata).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        private string ExtractCameraModel(IMediaAnalysis mediaInfo)
        {
            var tags = mediaInfo.Format.Tags;
            if (tags != null)
            {
                var cameraTag = tags.FirstOrDefault(t => 
                    t.Key.Contains("camera", StringComparison.OrdinalIgnoreCase) ||
                    t.Key.Contains("make", StringComparison.OrdinalIgnoreCase));
                
                if (!string.IsNullOrEmpty(cameraTag.Value))
                {
                    return cameraTag.Value;
                }
            }
            return "Unknown";
        }

        private string ExtractCameraSettings(IMediaAnalysis mediaInfo)
        {
            var settings = new Dictionary<string, string>();
            var tags = mediaInfo.Format.Tags;
            
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    if (tag.Key.Contains("iso", StringComparison.OrdinalIgnoreCase) || 
                        tag.Key.Contains("exposure", StringComparison.OrdinalIgnoreCase) || 
                        tag.Key.Contains("aperture", StringComparison.OrdinalIgnoreCase))
                    {
                        settings[tag.Key] = tag.Value;
                    }
                }
            }
            return System.Text.Json.JsonSerializer.Serialize(settings);
        }

        private string DetermineCategoryFromPath(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            var dirName = Path.GetFileName(directory)?.ToLower() ?? "";

            if (dirName.Contains("drone") || dirName.Contains("aerial"))
                return "Aerial";
            if (dirName.Contains("broll") || dirName.Contains("b-roll"))
                return "B-Roll";
            return "Uncategorized";
        }

        private string DetermineRecordingMode(VideoStream? videoStream)
        {
            if (videoStream == null) return "Unknown";
            
            var colorSpace = GetColorSpace(videoStream).ToLower();
            if (colorSpace.Contains("bt2020"))
                return "HDR";
            if (colorSpace.Contains("bt709"))
                return "Standard";
            return "Unknown";
        }

        private string GetColorSpace(VideoStream? videoStream)
        {
            if (videoStream == null) return "Unknown";
            
            var colorSpace = videoStream.ColorSpace;
            if (string.IsNullOrEmpty(colorSpace))
            {
                // Try to determine from pixel format
                if (videoStream.PixelFormat?.Contains("yuv420p") ?? false)
                    return "bt709";
            }
            return colorSpace ?? "Unknown";
        }

        private int GetColorDepth(VideoStream? videoStream)
        {
            if (videoStream == null) return 0;
            
            var pixelFormat = videoStream.PixelFormat ?? "";
            if (pixelFormat.Contains("p10"))
                return 10;
            if (pixelFormat.Contains("p12"))
                return 12;
            return 8; // Default to 8-bit
        }

        private bool IsVariableFrameRate(VideoStream? videoStream)
        {
            if (videoStream == null) return false;
            
            // Get the frame rates from the stream properties - these are doubles
            var avgFrameRate = videoStream.AvgFrameRate;
            var rFrameRate = videoStream.FrameRate;

            // Check if either rate is 0 (invalid)
            if (avgFrameRate <= 0 || rFrameRate <= 0)
                return false;

            // Direct comparison of the frame rates
            return Math.Abs(avgFrameRate - rFrameRate) > 0.001;
        }

        private async Task SaveMetadataAsync(VideoMetadata metadata)
        {
            // Check if metadata already exists
            var existingMetadata = await _dbContext.Set<VideoMetadata>()
                .FirstOrDefaultAsync(m => m.VideoFileId == metadata.VideoFileId);

            if (existingMetadata != null)
            {
                // Update existing metadata
                _dbContext.Entry(existingMetadata).CurrentValues.SetValues(metadata);
            }
            else
            {
                // Add new metadata
                _dbContext.Set<VideoMetadata>().Add(metadata);
            }
        }
    }
} 