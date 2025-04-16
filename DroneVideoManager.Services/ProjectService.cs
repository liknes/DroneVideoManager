using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DroneVideoManager.Core.Models;
using DroneVideoManager.Core.Services;
using DroneVideoManager.Data;
using Microsoft.EntityFrameworkCore;

namespace DroneVideoManager.Services
{
    public class ProjectService : IProjectService
    {
        private readonly DroneVideoDbContext _dbContext;
        private readonly List<Project> _projects = new();
        private readonly Dictionary<int, HashSet<int>> _projectVideos = new();
        private int _nextId = 1;

        public ProjectService(DroneVideoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _dbContext.Projects
                .Include(p => p.Videos)
                    .ThenInclude(vp => vp.VideoFile)
                .OrderByDescending(p => p.ModifiedDate)
                .ToListAsync();
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            project.Id = _nextId++;
            project.CreatedDate = DateTime.Now;
            project.ModifiedDate = DateTime.Now;
            
            _projects.Add(project);
            _projectVideos[project.Id] = new HashSet<int>();
            
            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();
            
            return project;
        }

        public async Task<Project> GetProjectByIdAsync(int projectId)
        {
            return await _dbContext.Projects
                .Include(p => p.Videos)
                    .ThenInclude(vp => vp.VideoFile)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            project.ModifiedDate = DateTime.Now;
            _dbContext.Entry(project).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return project;
        }

        public async Task DeleteProjectAsync(int projectId)
        {
            var project = await _dbContext.Projects.FindAsync(projectId);
            if (project != null)
            {
                _dbContext.Projects.Remove(project);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Project>> SearchProjectsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllProjectsAsync();

            return await _dbContext.Projects
                .Include(p => p.Videos)
                    .ThenInclude(vp => vp.VideoFile)
                .Where(p => p.Name.Contains(searchText) ||
                           p.Description.Contains(searchText) ||
                           p.ClientName.Contains(searchText))
                .OrderByDescending(p => p.ModifiedDate)
                .ToListAsync();
        }

        public async Task AddVideoToProjectAsync(int projectId, int videoId)
        {
            var project = await _dbContext.Projects.FindAsync(projectId);
            var video = await _dbContext.VideoFiles.FindAsync(videoId);

            if (project == null || video == null)
                throw new ArgumentException("Project or video not found");

            var videoProject = new VideoProject
            {
                ProjectId = projectId,
                VideoFileId = videoId,
                DateAdded = DateTime.Now,
                IsUsedInFinalCut = false
            };

            _dbContext.Set<VideoProject>().Add(videoProject);
            project.ModifiedDate = DateTime.Now;
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveVideoFromProjectAsync(int projectId, int videoId)
        {
            var videoProject = await _dbContext.Set<VideoProject>()
                .FirstOrDefaultAsync(vp => vp.ProjectId == projectId && vp.VideoFileId == videoId);

            if (videoProject != null)
            {
                _dbContext.Set<VideoProject>().Remove(videoProject);
                var project = await _dbContext.Projects.FindAsync(projectId);
                if (project != null)
                {
                    project.ModifiedDate = DateTime.Now;
                }
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateVideoProjectNotesAsync(int projectId, int videoId, string notes)
        {
            var videoProject = await _dbContext.Set<VideoProject>()
                .FirstOrDefaultAsync(vp => vp.ProjectId == projectId && vp.VideoFileId == videoId);

            if (videoProject != null)
            {
                videoProject.Notes = notes;
                var project = await _dbContext.Projects.FindAsync(projectId);
                if (project != null)
                {
                    project.ModifiedDate = DateTime.Now;
                }
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task MarkVideoAsUsedInFinalCutAsync(int projectId, int videoId, bool isUsed)
        {
            var videoProject = await _dbContext.Set<VideoProject>()
                .FirstOrDefaultAsync(vp => vp.ProjectId == projectId && vp.VideoFileId == videoId);

            if (videoProject != null)
            {
                videoProject.IsUsedInFinalCut = isUsed;
                var project = await _dbContext.Projects.FindAsync(projectId);
                if (project != null)
                {
                    project.ModifiedDate = DateTime.Now;
                }
                await _dbContext.SaveChangesAsync();
            }
        }
    }
} 