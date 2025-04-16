using System.Collections.Generic;
using System.Threading.Tasks;
using DroneVideoManager.Core.Models;

namespace DroneVideoManager.Core.Services
{
    public interface IProjectService
    {
        Task<Project> CreateProjectAsync(Project project);
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project> GetProjectByIdAsync(int projectId);
        Task DeleteProjectAsync(int projectId);
        Task<Project> UpdateProjectAsync(Project project);
        Task AddVideoToProjectAsync(int projectId, int videoId);
        Task RemoveVideoFromProjectAsync(int projectId, int videoId);
        Task<IEnumerable<Project>> SearchProjectsAsync(string searchText);
        Task UpdateVideoProjectNotesAsync(int projectId, int videoId, string notes);
        Task MarkVideoAsUsedInFinalCutAsync(int projectId, int videoId, bool isUsed);
    }
} 