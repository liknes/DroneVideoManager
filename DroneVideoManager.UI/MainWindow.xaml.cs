using System.Windows;
using DroneVideoManager.Core.Services;
using DroneVideoManager.UI.ViewModels;

namespace DroneVideoManager.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(
            IVideoFileService videoFileService,
            IFolderService folderService,
            IProjectService projectService,
            ILoggingService loggingService)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(videoFileService, folderService, projectService, loggingService);
        }
    }
}