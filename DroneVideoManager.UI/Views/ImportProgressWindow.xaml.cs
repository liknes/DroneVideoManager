using System.ComponentModel;
using System.Windows;

namespace DroneVideoManager.UI.Views
{
    public partial class ImportProgressWindow : Window, INotifyPropertyChanged
    {
        private double _progress;
        private string _status = "";
        private string _detailedStatus = "";

        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public string ProgressText => $"{Progress:F0}%";

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public string DetailedStatus
        {
            get => _detailedStatus;
            set
            {
                _detailedStatus = value;
                OnPropertyChanged(nameof(DetailedStatus));
            }
        }

        public ImportProgressWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 