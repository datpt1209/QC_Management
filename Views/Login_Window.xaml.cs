using System;
using System.Windows;
using System.Windows.Controls;
using QC_Management.ViewModels;

namespace QC_Management
{
    /// <summary>
    /// Interaction logic for Login_Window.xaml
    /// </summary>
    public partial class Login_Window : Window
    {
        public Login_Window()
        {
            InitializeComponent();
            Loaded += Login_Window_Loaded;
        }

        private void Login_Window_Loaded(object? sender, RoutedEventArgs e)
        {
            try
            {
                // Temporarily measure content width by allowing the window to size to content
                var previousSizeToContent = this.SizeToContent;
                this.SizeToContent = SizeToContent.WidthAndHeight;

                // Force layout so ActualWidth reflects content
                this.UpdateLayout();

                double measuredWidth = this.ActualWidth;

                // Choose sensible bounds so the window doesn't become too small or too large
                const double minWidth = 380;                                 // minimum usable width
                double maxWidth = Math.Min(800, SystemParameters.WorkArea.Width * 0.6); // cap to 60% of work area or 800px

                double targetWidth = Math.Max(minWidth, Math.Min(measuredWidth, maxWidth));

                // Restore previous sizing behavior and apply the calculated width
                this.SizeToContent = previousSizeToContent;
                this.Width = targetWidth;

                // Re-center on screen (in case width changed)
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                // If DataContext is LoginViewModel and it has a loaded Password, sync it to the PasswordBox so UI shows saved password.
                if (this.DataContext is LoginViewModel vm)
                {
                    if (!string.IsNullOrEmpty(vm.Password) && FloatingPasswordBox != null)
                    {
                        // set the PasswordBox value so the user sees the restored password
                        FloatingPasswordBox.Password = vm.Password;
                    }
                }
            }
            catch
            {
                // Fall back silently if anything goes wrong; do not block login flow.
            }
        }
    }
}
