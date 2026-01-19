using System.Windows;
using System.Windows.Input;

namespace tnt_wpf_children.Views
{
    public partial class NoteEditWindow : Window
    {
        public NoteEditWindow()
        {
            InitializeComponent();
            Loaded += NoteEditWindow_Loaded;
        }

        private void NoteEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (NoteTextBox != null)
            {
                NoteTextBox.Focus();
                NoteTextBox.CaretIndex = NoteTextBox.Text.Length;
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
