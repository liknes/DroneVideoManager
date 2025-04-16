using System.Windows;

namespace DroneVideoManager.UI.Views
{
    public partial class ImportSettingsWindow : Window
    {
        public ImportSettings ImportSettings { get; private set; }

        public ImportSettingsWindow()
        {
            InitializeComponent();
            ImportSettings = new ImportSettings();
            DataContext = ImportSettings;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class ImportSettings
    {
        public bool ImportTelemetryData { get; set; } = true;
        public bool CalculateSpeedAndHeading { get; set; } = true;
        public bool ImportCameraMetadata { get; set; } = true;
    }
} 