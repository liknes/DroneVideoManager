using System;
using System.Collections.Generic;

namespace DroneVideoManager.Core.Models
{
    public class VideoFile
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public double FileSize { get; set; } // In MB
        public TimeSpan Duration { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ImportedDate { get; set; }
        public string? Description { get; set; }
        public int? FolderId { get; set; }
        
        // Technical Metadata
        public string ColorSpace { get; set; } = "Unknown";
        public int Width { get; set; }
        public int Height { get; set; }
        public double FramesPerSecond { get; set; }
        
        // GPS Data
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Altitude { get; set; }
        
        // Custom Metadata
        public string FileHash { get; set; } = string.Empty;
        
        // Navigation Properties
        public virtual ICollection<VideoTag> Tags { get; set; } = new List<VideoTag>();
        public virtual ICollection<VideoProject> Projects { get; set; } = new List<VideoProject>();
        public virtual DroneMetadata? DroneMetadata { get; set; }
        public virtual Folder? Folder { get; set; }
    }
} 