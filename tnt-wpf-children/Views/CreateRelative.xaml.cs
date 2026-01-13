using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using tnt_wpf_children.ViewModels;

namespace tnt_wpf_children.Views
{
    /// <summary>
    /// Interaction logic for CreateRelative.xaml
    /// </summary>
    public partial class CreateRelative : Window
    {
        public CreateRelative()
        {
            InitializeComponent();
            DataContext = new CreateRelativeViewModel();
        }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    }
}
