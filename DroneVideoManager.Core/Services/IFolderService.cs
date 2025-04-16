using System.Collections.Generic;
using System.Threading.Tasks;
using DroneVideoManager.Core.Models;

namespace DroneVideoManager.Core.Services
{
    public interface IFolderService
    {
        Task<Folder> ImportFolderAsync(string folderPath);
        Task<IEnumerable<Folder>> GetAllFoldersAsync();
        Task<Folder> GetFolderByIdAsync(int folderId);
        Task DeleteFolderAsync(int folderId);
        Task<Folder> UpdateFolderAsync(Folder folder);
        Task ScanFolderForChangesAsync(int folderId);
    }
} 