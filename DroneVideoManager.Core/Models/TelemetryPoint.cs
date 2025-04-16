using System;

namespace DroneVideoManager.Core.Models
{
    public class TelemetryPoint
    {
        public int Id { get; set; }
        public int DroneMetadataId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double Speed { get; set; }
        public double Heading { get; set; }
        
        // Navigation properties
        public virtual DroneMetadata DroneMetadata { get; set; } = null!;
        public virtual CameraMetadata? CameraMetadata { get; set; }
    }
} 