using System.Windows;
using System.Windows.Media;

namespace DroneVideoManager.UI.Services
{
    public static class ThemeManager
    {
        private static ResourceDictionary _currentTheme;

        public static void ApplyDarkTheme()
        {
            var darkTheme = new ResourceDictionary
            {
                Source = new System.Uri("/DroneVideoManager.UI;component/Themes/DarkTheme.xaml", System.UriKind.Relative)
            };

            ApplyTheme(darkTheme);
        }

        public static void ApplyLightTheme()
        {
            // Clear any existing theme
            if (_currentTheme != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(_currentTheme);
                _currentTheme = null;
            }

            // Force update of all windows to use system colors
            foreach (Window window in Application.Current.Windows)
            {
                if (window != null)
                {
                    window.Background = SystemColors.WindowBrush;
                    window.Foreground = SystemColors.WindowTextBrush;
                    UpdateControlTheme(window);
                }
            }
        }

        private static void ApplyTheme(ResourceDictionary theme)
        {
            // Remove current theme if exists
            if (_currentTheme != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(_currentTheme);
            }

            // Add new theme
            Application.Current.Resources.MergedDictionaries.Add(theme);
            _currentTheme = theme;

            // Update all windows
            foreach (Window window in Application.Current.Windows)
            {
                UpdateWindowTheme(window);
            }
        }

        private static void UpdateWindowTheme(Window window)
        {
            if (window == null) return;

            // Update window background and foreground
            if (_currentTheme != null)
            {
                window.Background = (SolidColorBrush)Application.Current.Resources["WindowBackgroundBrush"];
                window.Foreground = (SolidColorBrush)Application.Current.Resources["WindowForegroundBrush"];
            }
            else
            {
                // Reset to default system colors
                window.Background = SystemColors.WindowBrush;
                window.Foreground = SystemColors.WindowTextBrush;
            }

            // Update all child controls
            UpdateControlTheme(window);
        }

        private static void UpdateControlTheme(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement element)
                {
                    // Force style update
                    var style = element.Style;
                    element.Style = null;
                    element.Style = style;
                }
                UpdateControlTheme(child);
            }
        }
    }
} 