using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using tnt_wpf_children.ViewModels;

namespace tnt_wpf_children
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                MaximizeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore;
            }
            else
            {
                WindowState = WindowState.Normal;
                MaximizeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize;
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MaxHeight = SystemParameters.WorkArea.Height;
            MaxWidth = SystemParameters.WorkArea.Width;
        }
        private bool _isExplicitClose = false;

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            bool isLoginOpen = false;
            foreach (Window win in Application.Current.Windows)
            {
                if (win.GetType().Name == "Login")
                {
                    isLoginOpen = true;
                    break;
                }
            }

            if (_isExplicitClose || isLoginOpen)
            {
                base.OnClosing(e);
                if (!isLoginOpen) 
                {
                     Services.CameraService.Instance.StopCamera();
                     Application.Current.Shutdown();
                }
                return;
            }

            e.Cancel = true;
            var vm = new ViewModels.ConfirmationViewModel("Bạn có chắc chắn muốn thoát phần mềm?", "Xác nhận thoát");
            var winConfirm = new Views.ConfirmationWindow { DataContext = vm, Owner = this };
            
            if (winConfirm.ShowDialog() == true)
            {
                _isExplicitClose = true;
                Services.CameraService.Instance.StopCamera();
                Application.Current.Shutdown();
            }
        }
    }

    public class RowNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
                return index + 1;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class NotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}