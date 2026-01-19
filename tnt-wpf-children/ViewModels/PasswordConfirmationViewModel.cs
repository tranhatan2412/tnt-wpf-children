using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using tnt_wpf_children.Services;

namespace tnt_wpf_children.ViewModels
{
    public class PasswordConfirmationViewModel : BaseViewModel
    {
        public MaterialDesignThemes.Wpf.ISnackbarMessageQueue MessageQueue { get; }

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public PasswordConfirmationViewModel(string message = "")
        {
            MessageQueue = new MaterialDesignThemes.Wpf.SnackbarMessageQueue(TimeSpan.FromSeconds(2));
            
            ConfirmCommand = new RelayCommand<object>(p => true, Confirm);
            CancelCommand = new RelayCommand<Window>(p => p != null, w => 
            {
                w.DialogResult = false;
                w.Close();
            });
        }

        private void Confirm(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            var window = Window.GetWindow(passwordBox);
            
            if (passwordBox == null || string.IsNullOrEmpty(passwordBox.Password))
            {
                MessageQueue.Enqueue("Vui lòng nhập mật khẩu!");
                return;
            }

            if (AuthService.Instance.VerifyAnyAdminPassword(passwordBox.Password))
            {
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
            {
                MessageQueue.Enqueue("Mật khẩu không đúng!");
                passwordBox.Password = "";
                passwordBox.Focus();
            }
        }
    }
}
