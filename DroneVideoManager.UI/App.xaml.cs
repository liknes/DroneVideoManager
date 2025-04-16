using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DroneVideoManager.Core.Services;
using DroneVideoManager.Services;
using DroneVideoManager.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using FFMpegCore;
using System.IO.Abstractions;
using DroneVideoManager.UI.Services;
using DroneVideoManager.UI.ViewModels;

namespace DroneVideoManager.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        private IFileSystemWatcherService _fileSystemWatcherService;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Initialize database
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DroneVideoDbContext>();
                
                // Ensure the database directory exists
                var dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "DroneVideoManager");
                Directory.CreateDirectory(dbPath);

                // Initialize database with seed data
                DatabaseInitializer.Initialize(dbContext);
            }

            // Get the file system watcher service
            _fileSystemWatcherService = _serviceProvider.GetRequiredService<IFileSystemWatcherService>();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configure DbContext
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DroneVideoManager",
                "dronevideo.db");

            services.AddDbContext<DroneVideoDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Register services
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IVideoFileService, VideoFileService>();
            services.AddSingleton<IFolderService, FolderService>();
            services.AddSingleton<IProjectService, ProjectService>();
            services.AddSingleton<IFileSystemWatcherService, FileSystemWatcherService>();
            services.AddScoped<IVideoMetadataService, VideoMetadataService>();
            services.AddSingleton<IFileSystem, FileSystem>();

            // Register main window
            services.AddTransient<MainWindow>();

            // Configure FFmpeg
            FFMpegCore.GlobalFFOptions.Configure(new FFMpegCore.FFOptions
            {
                BinaryFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg"),
                TemporaryFilesFolder = Path.GetTempPath()
            });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Apply light theme by default
            ThemeManager.ApplyLightTheme();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_fileSystemWatcherService != null)
            {
                await _fileSystemWatcherService.StopWatchingAllFoldersAsync();
                if (_fileSystemWatcherService is IDisposable disposableWatcher)
                {
                    disposableWatcher.Dispose();
                }
            }

            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }
    }
}