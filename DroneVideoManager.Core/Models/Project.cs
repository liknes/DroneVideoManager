using System;
using System.Collections.Generic;

namespace DroneVideoManager.Core.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        
        public virtual ICollection<VideoProject> Videos { get; set; }
    }
    
    public class VideoProject
    {
        public int VideoFileId { get; set; }
        public int ProjectId { get; set; }
        
        public virtual VideoFile VideoFile { get; set; }
        public virtual Project Project { get; set; }
        
        public DateTime DateAdded { get; set; }
        public string Notes { get; set; }
        public bool IsUsedInFinalCut { get; set; }
    }
} 