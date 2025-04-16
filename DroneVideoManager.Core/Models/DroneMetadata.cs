using System;
using System.Collections.Generic;

namespace DroneVideoManager.Core.Models
{
    public class DroneMetadata
    {
        public int Id { get; set; }
        public int VideoFileId { get; set; }
        public string DroneModel { get; set; } = string.Empty;
        public DateTime FlightDate { get; set; }
        
        // Home position
        public double HomeLatitude { get; set; }
        public double HomeLongitude { get; set; }
        public double HomeAltitude { get; set; }

        // Navigation properties
        public virtual VideoFile VideoFile { get; set; }
        public virtual ICollection<TelemetryPoint> TelemetryPoints { get; set; } = new List<TelemetryPoint>();
    }
} 