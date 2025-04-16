using System;
using System.Collections.Generic;

namespace DroneVideoManager.Core.Models
{
    public class Folder
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime LastScannedDate { get; set; }
        public bool IsWatched { get; set; }
        
        // Self-referencing relationship for folder hierarchy
        public int? ParentFolderId { get; set; }
        public virtual Folder ParentFolder { get; set; }
        public virtual ICollection<Folder> SubFolders { get; set; }
        
        // Videos in this folder
        public virtual ICollection<VideoFile> Videos { get; set; }
    }
} 