using System.Threading.Tasks;
using DroneVideoManager.Core.Models;

namespace DroneVideoManager.Core.Services
{
    public interface IVideoMetadataService
    {
        Task ExtractAndSaveMetadataAsync(VideoFile videoFile);
        Task<VideoMetadata?> GetMetadataAsync(int videoFileId);
        Task UpdateMetadataAsync(VideoMetadata metadata);
    }
} 