using System;
using System.Windows;
using System.Windows.Input;

namespace tnt_wpf_children.ViewModels
{
    public class ConfirmationViewModel : BaseViewModel
    {
        private string _message;
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        private bool _isConfirmation;
        public bool IsConfirmation
        {
            get => _isConfirmation;
            set { _isConfirmation = value; OnPropertyChanged(); }
        }

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public ConfirmationViewModel(string message, string title = "Thông báo", bool isConfirmation = true)
        {
            Message = message;
            Title = title;
            IsConfirmation = isConfirmation;

            ConfirmCommand = new RelayCommand<Window>(p => p != null, w => 
            {
                w.DialogResult = true;
                w.Close();
            });

            CancelCommand = new RelayCommand<Window>(p => p != null, w => 
            {
                w.DialogResult = false;
                w.Close();
            });
        }
    }
}
