using System;

namespace DroneVideoManager.Core.Models
{
    public class CameraMetadata
    {
        public int Id { get; set; }
        public int TelemetryPointId { get; set; }
        
        // Camera Settings
        public int ISO { get; set; }
        public string ShutterSpeed { get; set; } = string.Empty;
        public double Aperture { get; set; }
        public int ExposureValue { get; set; }
        public int ColorTemperature { get; set; }
        public string ColorMode { get; set; } = string.Empty;
        public int FocalLength { get; set; }
        public int DigitalZoomRatio { get; set; }
        public int DigitalZoomDelta { get; set; }
        
        // Navigation Property
        public virtual TelemetryPoint TelemetryPoint { get; set; } = null!;
    }
} 