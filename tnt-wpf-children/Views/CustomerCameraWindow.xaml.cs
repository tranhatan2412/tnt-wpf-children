using System.Windows;
using tnt_wpf_children.ViewModels;

namespace tnt_wpf_children.Views
{
    public partial class CustomerCameraWindow : Window
    {
        public CustomerCameraWindow(CameraViewModel viewModel)
        {
            InitializeComponent();
            CameraPreview.DataContext = viewModel;
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                DragMove();
        }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    }
}
