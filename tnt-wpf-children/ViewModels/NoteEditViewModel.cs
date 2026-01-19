using System.Windows;
using System.Windows.Input;

namespace tnt_wpf_children.ViewModels
{
    public class NoteEditViewModel : BaseViewModel
    {
        private string _noteContent;
        public string NoteContent
        {
            get => _noteContent;
            set { _noteContent = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public NoteEditViewModel(string note)
        {
            _noteContent = note ?? string.Empty;

            SaveCommand = new RelayCommand<Window>(p => p != null, win => 
            {
                win.DialogResult = true;
                win.Close();
            });

            CancelCommand = new RelayCommand<Window>(p => p != null, win => 
            {
                win.DialogResult = false;
                win.Close();
            });
        }
    }
}
