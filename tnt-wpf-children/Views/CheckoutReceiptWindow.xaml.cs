using System.Windows;
using System.Windows.Input;

namespace tnt_wpf_children.Views
{
    public partial class CheckoutReceiptWindow : Window
    {
        public CheckoutReceiptWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
