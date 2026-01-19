using System.Windows;
using System.Windows.Input;

namespace tnt_wpf_children.Views
{
    public partial class CreateSession : Window
    {
        public CreateSession()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
