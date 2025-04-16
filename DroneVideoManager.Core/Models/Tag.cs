using System;
using System.Collections.Generic;

namespace DroneVideoManager.Core.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        
        public virtual ICollection<VideoTag> Videos { get; set; } = new List<VideoTag>();
    }
    
    public class VideoTag
    {
        public int VideoFileId { get; set; }
        public int TagId { get; set; }
        
        public virtual VideoFile VideoFile { get; set; }
        public virtual Tag Tag { get; set; }
        
        public DateTime DateTagged { get; set; }
    }
} 