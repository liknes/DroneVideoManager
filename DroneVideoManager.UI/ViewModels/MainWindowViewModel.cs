using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using DroneVideoManager.Core.Models;
using DroneVideoManager.Core.Services;
using Microsoft.Win32;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.IO;
using DroneVideoManager.UI.Views;
using FFMpegCore;

namespace DroneVideoManager.UI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IVideoFileService _videoFileService;
        private readonly IFolderService _folderService;
        private readonly IProjectService _projectService;
        private readonly ILoggingService _loggingService;

        public ObservableCollection<VideoFile> RecentVideos { get; private set; }
        public ObservableCollection<Folder> Folders { get; private set; }
        public ObservableCollection<Project> Projects { get; private set; }

        private VideoFile _selectedVideo;
        public VideoFile SelectedVideo
        {
            get => _selectedVideo;
            set
            {
                _selectedVideo = value;
                OnPropertyChanged();
            }
        }

        private Folder _selectedFolder;
        public Folder SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                _selectedFolder = value;
                LoadFolderVideos();
                OnPropertyChanged();
            }
        }

        private Project _selectedProject;
        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                LoadProjectVideos();
                OnPropertyChanged();
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        private bool _isImporting;
        public bool IsImporting
        {
            get => _isImporting;
            set
            {
                _isImporting = value;
                OnPropertyChanged();
            }
        }

        private string _importStatus;
        public string ImportStatus
        {
            get => _importStatus;
            set
            {
                _importStatus = value;
                OnPropertyChanged();
            }
        }

        public ICommand ImportVideoCommand { get; }
        public ICommand ImportFolderCommand { get; }
        public ICommand CreateProjectCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand DeleteVideoCommand { get; }
        public ICommand DeleteProjectCommand { get; }
        public ICommand AddToProjectCommand { get; }
        public ICommand RefreshCommand { get; }

        public MainWindowViewModel(
            IVideoFileService videoFileService,
            IFolderService folderService,
            IProjectService projectService,
            ILoggingService loggingService)
        {
            _videoFileService = videoFileService;
            _folderService = folderService;
            _projectService = projectService;
            _loggingService = loggingService;

            RecentVideos = new ObservableCollection<VideoFile>();
            Folders = new ObservableCollection<Folder>();
            Projects = new ObservableCollection<Project>();

            ImportVideoCommand = new AsyncRelayCommand(ImportVideo);
            ImportFolderCommand = new AsyncRelayCommand(ImportFolder);
            CreateProjectCommand = new RelayCommand(async () => await CreateProject());
            SearchCommand = new RelayCommand(async () => await Search());
            DeleteVideoCommand = new RelayCommand(async () => await DeleteVideo(), () => SelectedVideo != null);
            DeleteProjectCommand = new RelayCommand(async () => await DeleteProject(), () => SelectedProject != null);
            AddToProjectCommand = new RelayCommand(async () => await AddToProject(), () => SelectedVideo != null && SelectedProject != null);
            RefreshCommand = new RelayCommand(async () => await LoadData());

            _ = LoadData();
        }

        private async Task LoadData()
        {
            await LoadRecentVideos();
            await LoadFolders();
            await LoadProjects();
        }

        private async Task LoadRecentVideos()
        {
            var videos = await _videoFileService.GetRecentVideosAsync(20);
            RecentVideos.Clear();
            foreach (var video in videos)
            {
                RecentVideos.Add(video);
            }
        }

        private async Task LoadFolders()
        {
            var folders = await _folderService.GetAllFoldersAsync();
            Folders.Clear();
            foreach (var folder in folders)
            {
                Folders.Add(folder);
            }
        }

        private async Task LoadProjects()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            Projects.Clear();
            foreach (var project in projects)
            {
                Projects.Add(project);
            }
        }

        private async Task LoadFolderVideos()
        {
            if (SelectedFolder != null)
            {
                var videos = await _videoFileService.GetVideosByFolderAsync(SelectedFolder.Id);
                RecentVideos.Clear();
                foreach (var video in videos)
                {
                    RecentVideos.Add(video);
                }
            }
        }

        private async Task LoadProjectVideos()
        {
            if (SelectedProject != null)
            {
                var videos = await _videoFileService.GetVideosByProjectAsync(SelectedProject.Id);
                RecentVideos.Clear();
                foreach (var video in videos)
                {
                    RecentVideos.Add(video);
                }
            }
        }

        private async Task ImportVideo()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Video files (*.mp4;*.mov;*.avi;*.mkv)|*.mp4;*.mov;*.avi;*.mkv|All files (*.*)|*.*",
                    Multiselect = true
                };

                if (dialog.ShowDialog() == true)
                {
                    IsImporting = true;
                    ImportStatus = "Importing videos...";

                    foreach (var filename in dialog.FileNames)
                    {
                        try
                        {
                            await _videoFileService.ImportVideoFileAsync(filename);
                        }
                        catch (Exception ex)
                        {
                            _loggingService.LogError($"Error importing file: {filename}", ex);
                            MessageBox.Show(
                                $"Error importing {Path.GetFileName(filename)}\n\nError: {ex.Message}\n\nCheck the log file for more details.",
                                "Import Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    }
                    
                    await LoadRecentVideos();
                    ImportStatus = "Import completed";
                    await Task.Delay(3000);
                    ImportStatus = string.Empty;
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in import operation", ex);
                MessageBox.Show(
                    $"Error during import operation.\n\nError: {ex.Message}\n\nCheck the log file for more details.",
                    "Import Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsImporting = false;
            }
        }

        private async Task ImportFolder()
        {
            try
            {
                var folderDialog = new OpenFolderDialog
                {
                    Title = "Select Folder to Import"
                };

                if (folderDialog.ShowDialog() == true)
                {
                    var progressWindow = new ImportProgressWindow
                    {
                        Owner = Application.Current.MainWindow,
                        Status = "Preparing to import folder..."
                    };

                    try
                    {
                        _loggingService.LogInformation($"Starting folder import: {folderDialog.FolderName}");
                        
                        // Show progress window
                        progressWindow.Show();

                        // Create progress handler
                        var progress = new Progress<(int Processed, int Total)>(tuple => 
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                progressWindow.Progress = tuple.Total == 0 ? 0 : (tuple.Processed * 100.0) / tuple.Total;
                                progressWindow.DetailedStatus = $"Processing files: {tuple.Processed}/{tuple.Total}";
                            });
                        });

                        var folder = await _folderService.ImportFolderAsync(folderDialog.FolderName, progress);
                        
                        await LoadFolders();
                        await LoadRecentVideos();
                        
                        progressWindow.Status = "Import completed successfully";
                        progressWindow.Progress = 100;
                        _loggingService.LogInformation("Folder import completed");

                        await Task.Delay(2000); // Show completion for 2 seconds
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError("Error during folder import", ex);
                        progressWindow.Status = "Error importing folder";
                        MessageBox.Show(
                            $"Error importing folder:\n\n{ex.Message}\n\nCheck the log file for more details.",
                            "Import Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    finally
                    {
                        progressWindow.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in folder import operation", ex);
                MessageBox.Show(
                    $"Error during folder import operation:\n\n{ex.Message}\n\nCheck the log file for more details.",
                    "Import Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task CreateProject()
        {
            var project = new Project
            {
                Name = "New Project",
                Description = "Project Description",
                ClientName = "Client Name",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            await _projectService.CreateProjectAsync(project);
            await LoadProjects();
        }

        private async Task Search()
        {
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var videos = await _videoFileService.SearchVideosAsync(SearchText);
                RecentVideos.Clear();
                foreach (var video in videos)
                {
                    RecentVideos.Add(video);
                }
            }
            else
            {
                await LoadRecentVideos();
            }
        }

        private async Task DeleteVideo()
        {
            if (SelectedVideo != null)
            {
                await _videoFileService.DeleteVideoAsync(SelectedVideo.Id);
                RecentVideos.Remove(SelectedVideo);
                SelectedVideo = null;
            }
        }

        private async Task DeleteProject()
        {
            if (SelectedProject != null)
            {
                await _projectService.DeleteProjectAsync(SelectedProject.Id);
                Projects.Remove(SelectedProject);
                SelectedProject = null;
            }
        }

        private async Task AddToProject()
        {
            if (SelectedVideo != null && SelectedProject != null)
            {
                await _projectService.AddVideoToProjectAsync(SelectedProject.Id, SelectedVideo.Id);
                await LoadProjectVideos();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;

        public RelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public async void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    await _execute();
                }
                finally
                {
                    _isExecuting = false;
                }
            }
        }
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute)
        {
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting;
        }

        public async void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    CommandManager.InvalidateRequerySuggested();
                    
                    await _execute();
                }
                finally
                {
                    _isExecuting = false;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
    }
} 