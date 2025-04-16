using System;

namespace DroneVideoManager.Core.Models
{
    public class VideoMetadata
    {
        public int Id { get; set; }
        public int VideoFileId { get; set; }

        // Technical Information
        public long BitRate { get; set; }
        public string ColorSpace { get; set; } = string.Empty;
        public int ColorDepth { get; set; }
        public string VideoCodec { get; set; } = string.Empty;
        public bool IsVariableFrameRate { get; set; }

        // Audio Information
        public string AudioCodec { get; set; } = string.Empty;
        public int AudioChannels { get; set; }
        public int AudioSampleRate { get; set; }
        public long AudioBitRate { get; set; }

        // Camera Settings
        public string CameraModel { get; set; } = string.Empty;
        public string CameraSettings { get; set; } = string.Empty; // JSON string of camera settings

        // Quality and Organization
        public string Category { get; set; } = string.Empty;
        public DateTime ExactCreationTime { get; set; }
        public string RecordingMode { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Navigation Property
        public virtual VideoFile VideoFile { get; set; } = null!;
    }
} 