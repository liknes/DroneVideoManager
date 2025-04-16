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
using System.IO.Abstractions;

namespace DroneVideoManager.Services
{
    public class VideoMetadataService : IVideoMetadataService
    {
        private readonly DroneVideoDbContext _dbContext;
        private readonly ILoggingService _loggingService;
        private readonly IFileSystem _fileSystem;

        public VideoMetadataService(DroneVideoDbContext dbContext, ILoggingService loggingService, IFileSystem fileSystem)
        {
            _dbContext = dbContext;
            _loggingService = loggingService;
            _fileSystem = fileSystem;
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

                // Check for DJI SRT file
                string srtPath = Path.ChangeExtension(videoFile.FilePath, ".SRT");
                if (_fileSystem.File.Exists(srtPath))
                {
                    _loggingService.LogInformation($"Found SRT file: {srtPath}");
                    var droneMetadata = await ExtractDroneMetadataAsync(srtPath, videoFile);
                    videoFile.DroneMetadata = droneMetadata;
                }
                else
                {
                    _loggingService.LogInformation($"No SRT file found for: {videoFile.FileName}");
                }

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

        private async Task<DroneMetadata> ExtractDroneMetadataAsync(string srtPath, VideoFile videoFile)
        {
            _loggingService.LogInformation($"Processing SRT file: {Path.GetFileName(srtPath)}");
            
            var metadata = new DroneMetadata
            {
                VideoFile = videoFile,
                FlightDate = _fileSystem.File.GetCreationTime(srtPath),
                TelemetryPoints = new List<TelemetryPoint>()
            };

            // Read and parse SRT file
            string[] lines = await _fileSystem.File.ReadAllLinesAsync(srtPath);
            _loggingService.LogInformation($"Found {lines.Length} lines in SRT file");
            
            int totalEntries = lines.Length / 5;
            int successfulEntries = 0;
            int skippedEntries = 0;
            
            // DJI SRT files have 5 lines per entry:
            // 1. Sequence number (e.g., "1")
            // 2. Time range (e.g., "00:00:00,000 --> 00:00:00,033")
            // 3. SrtCnt and DiffTime with HTML (e.g., "<font size="28">SrtCnt : 1, DiffTime : 33ms")
            // 4. Timestamp (e.g., "2025-03-22 11:05:15.023")
            // 5. Telemetry data with HTML formatting
            for (int i = 0; i < lines.Length; i += 5)
            {
                if (i + 4 >= lines.Length)
                {
                    _loggingService.LogWarning($"Incomplete entry at line {i}, skipping remaining lines");
                    break;
                }

                try
                {
                    // Extract timestamp from the fourth line
                    var timestampLine = lines[i + 3];
                    if (string.IsNullOrEmpty(timestampLine))
                    {
                        skippedEntries++;
                        continue;
                    }

                    if (!DateTime.TryParseExact(timestampLine.Trim(), "yyyy-MM-dd HH:mm:ss.fff", null, System.Globalization.DateTimeStyles.None, out var timestamp))
                    {
                        skippedEntries++;
                        continue;
                    }

                    // Parse telemetry data from the fifth line
                    var telemetryLine = lines[i + 4];
                    if (string.IsNullOrEmpty(telemetryLine))
                    {
                        skippedEntries++;
                        continue;
                    }

                    // Remove HTML formatting
                    telemetryLine = telemetryLine.Replace("<font size=\"28\">", "").Replace("</font>", "").Trim();

                    var telemetryData = new Dictionary<string, string>();
                    var parts = telemetryLine.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var keyValue = part.Split(':', 2);
                        if (keyValue.Length == 2)
                        {
                            telemetryData[keyValue[0].Trim()] = keyValue[1].Trim();
                        }
                    }

                    // Extract coordinates and altitude
                    if (telemetryData.TryGetValue("latitude", out var latStr) &&
                        telemetryData.TryGetValue("longitude", out var lonStr) &&
                        telemetryData.TryGetValue("rel_alt", out var altStr))
                    {
                        // Parse the altitude string which contains both relative and absolute altitude
                        // Format: "78.500 abs_alt: 54.090"
                        var altitudeParts = altStr.Split(new[] { ' ', 'a', 'b', 's', '_', 'a', 'l', 't', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        var relativeAltitude = double.Parse(altitudeParts[0]);

                        var telemetryPoint = new TelemetryPoint
                        {
                            DroneMetadata = metadata,
                            Timestamp = timestamp,
                            Latitude = double.Parse(latStr),
                            Longitude = double.Parse(lonStr),
                            Altitude = relativeAltitude,
                            Speed = 0, // Will be calculated later
                            Heading = 0 // Will be calculated later
                        };

                        // Create and populate camera metadata
                        var cameraMetadata = new CameraMetadata
                        {
                            TelemetryPoint = telemetryPoint,
                            ISO = int.Parse(telemetryData["iso"]),
                            ShutterSpeed = telemetryData["shutter"],
                            Aperture = double.Parse(telemetryData["fnum"]) / 100.0, // Convert from DJI format (e.g., 170 = f/1.7)
                            ExposureValue = int.Parse(telemetryData["ev"]),
                            ColorTemperature = int.Parse(telemetryData["ct"]),
                            ColorMode = telemetryData["color_md"],
                            FocalLength = int.Parse(telemetryData["focal_len"])
                        };

                        // Parse digital zoom data
                        var zoomData = telemetryData["dzoom_ratio"].Split(',');
                        cameraMetadata.DigitalZoomRatio = int.Parse(zoomData[0]);
                        cameraMetadata.DigitalZoomDelta = int.Parse(zoomData[1].Split(':')[1]);

                        telemetryPoint.CameraMetadata = cameraMetadata;
                        metadata.TelemetryPoints.Add(telemetryPoint);

                        // Set home position from first telemetry point
                        if (i == 0)
                        {
                            metadata.HomeLatitude = telemetryPoint.Latitude;
                            metadata.HomeLongitude = telemetryPoint.Longitude;
                            metadata.HomeAltitude = telemetryPoint.Altitude;
                            _loggingService.LogInformation($"Home position set: Lat={metadata.HomeLatitude}, Lon={metadata.HomeLongitude}, Alt={metadata.HomeAltitude}");
                        }

                        successfulEntries++;
                    }
                    else
                    {
                        skippedEntries++;
                    }
                }
                catch (Exception ex)
                {
                    skippedEntries++;
                    if (skippedEntries % 100 == 0) // Only log every 100 skipped entries to reduce log size
                    {
                        _loggingService.LogWarning($"Skipped {skippedEntries} entries so far. Last error: {ex.Message}");
                    }
                }
            }

            // Calculate speed and heading for all telemetry points
            CalculateSpeedAndHeading(metadata.TelemetryPoints);

            _loggingService.LogInformation($"SRT processing complete: {successfulEntries}/{totalEntries} entries processed successfully, {skippedEntries} entries skipped");

            // Set drone model based on file name pattern
            if (videoFile.FileName.StartsWith("DJI_", StringComparison.OrdinalIgnoreCase))
            {
                metadata.DroneModel = "DJI Mini 3 Pro";
            }
            else
            {
                metadata.DroneModel = "Unknown";
            }

            return metadata;
        }

        private void CalculateSpeedAndHeading(ICollection<TelemetryPoint> points)
        {
            if (points.Count < 2)
            {
                _loggingService.LogWarning("Not enough points to calculate speed and heading");
                return;
            }

            var pointsList = points.ToList();
            for (int i = 0; i < pointsList.Count; i++)
            {
                if (i == 0)
                {
                    // First point - use next point for calculations
                    if (pointsList.Count > 1)
                    {
                        CalculatePointSpeedAndHeading(pointsList[i], pointsList[i + 1]);
                    }
                }
                else if (i == pointsList.Count - 1)
                {
                    // Last point - use previous point for calculations
                    CalculatePointSpeedAndHeading(pointsList[i - 1], pointsList[i]);
                }
                else
                {
                    // Middle point - use both previous and next points
                    var prevPoint = pointsList[i - 1];
                    var nextPoint = pointsList[i + 1];
                    var currentPoint = pointsList[i];

                    // Calculate speed using both directions
                    var speedFromPrev = CalculateSpeed(prevPoint, currentPoint);
                    var speedToNext = CalculateSpeed(currentPoint, nextPoint);
                    currentPoint.Speed = (speedFromPrev + speedToNext) / 2;

                    // Calculate heading using both directions
                    var headingFromPrev = CalculateHeading(prevPoint, currentPoint);
                    var headingToNext = CalculateHeading(currentPoint, nextPoint);
                    currentPoint.Heading = (headingFromPrev + headingToNext) / 2;
                }
            }
        }

        private void CalculatePointSpeedAndHeading(TelemetryPoint point1, TelemetryPoint point2)
        {
            point2.Speed = CalculateSpeed(point1, point2);
            point2.Heading = CalculateHeading(point1, point2);
        }

        private double CalculateSpeed(TelemetryPoint point1, TelemetryPoint point2)
        {
            // Calculate time difference in seconds
            var timeDiff = (point2.Timestamp - point1.Timestamp).TotalSeconds;
            if (timeDiff <= 0) return 0;

            // Calculate distance using Haversine formula
            var distance = CalculateHaversineDistance(point1, point2);

            // Speed in meters per second
            return distance / timeDiff;
        }

        private double CalculateHeading(TelemetryPoint point1, TelemetryPoint point2)
        {
            var lat1 = DegreesToRadians(point1.Latitude);
            var lon1 = DegreesToRadians(point1.Longitude);
            var lat2 = DegreesToRadians(point2.Latitude);
            var lon2 = DegreesToRadians(point2.Longitude);

            var y = Math.Sin(lon2 - lon1) * Math.Cos(lat2);
            var x = Math.Cos(lat1) * Math.Sin(lat2) - 
                   Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1);

            var heading = Math.Atan2(y, x);
            heading = RadiansToDegrees(heading);
            heading = (heading + 360) % 360; // Normalize to 0-360 degrees

            return heading;
        }

        private double CalculateHaversineDistance(TelemetryPoint point1, TelemetryPoint point2)
        {
            const double R = 6371000; // Earth's radius in meters

            var lat1 = DegreesToRadians(point1.Latitude);
            var lon1 = DegreesToRadians(point1.Longitude);
            var lat2 = DegreesToRadians(point2.Latitude);
            var lon2 = DegreesToRadians(point2.Longitude);

            var dLat = lat2 - lat1;
            var dLon = lon2 - lon1;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private double RadiansToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }
    }
} 