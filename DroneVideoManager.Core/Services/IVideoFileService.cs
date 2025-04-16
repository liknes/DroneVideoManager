using System.Collections.Generic;
using System.Threading.Tasks;
using DroneVideoManager.Core.Models;

namespace DroneVideoManager.Core.Services
{
    public interface IVideoFileService
    {
        Task<VideoFile> ImportVideoFileAsync(string filePath);
        Task<IEnumerable<VideoFile>> GetRecentVideosAsync(int count);
        Task<IEnumerable<VideoFile>> GetVideosByFolderAsync(int folderId);
        Task<IEnumerable<VideoFile>> GetVideosByProjectAsync(int projectId);
        Task<IEnumerable<VideoFile>> SearchVideosAsync(string searchTerm);
        Task DeleteVideoAsync(int videoId);
        Task<VideoFile> GetVideoByIdAsync(int videoId);
    }
} 