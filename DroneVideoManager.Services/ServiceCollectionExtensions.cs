using DroneVideoManager.Core.Services;
using DroneVideoManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DroneVideoManager.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDroneVideoServices(this IServiceCollection services, string connectionString)
        {
            // Register DbContext
            services.AddDbContext<DroneVideoDbContext>(options =>
                options.UseSqlite(connectionString));

            // Register Services
            services.AddScoped<IVideoFileService, VideoFileService>();
            services.AddScoped<IFolderService, FolderService>();
            services.AddScoped<IProjectService, ProjectService>();

            return services;
        }
    }
} 