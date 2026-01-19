using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using tnt_wpf_children.Data;
using tnt_wpf_children.Models;

namespace tnt_wpf_children.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }
        private string _usernameHelperText;
        public string UsernameHelperText
        {
            get => _usernameHelperText;
            set { _usernameHelperText = value; OnPropertyChanged(); }
        }
        private string _passwordHelperText = "Ít nhất 8 kí tự (gồm chữ cái và số)";
        public string PasswordHelperText
        {
            get => _passwordHelperText;
            set { _passwordHelperText = value; OnPropertyChanged(); }
        }

        public enum IconState
        {
            None,
            Processing,
            Success
        }

        private IconState _icon = IconState.None;
        public IconState Icon
        {
            get => _icon;
            set
            {
                _icon = value; OnPropertyChanged();
                OnPropertyChanged(nameof(IsBusy));
                OnPropertyChanged(nameof(IsProcessing));
                OnPropertyChanged(nameof(IsSuccess));
            }
        }

        public bool IsBusy => Icon != IconState.None;
        public bool IsProcessing => Icon == IconState.Processing;
        public bool IsSuccess => Icon == IconState.Success;
        private string _tbStatus = "Đang xử lý...";
        public string TBStatus
        {
            get => _tbStatus;
            set { _tbStatus = value; OnPropertyChanged(); }
        }

        public ICommand LoginCmd { get; }

        

        public LoginViewModel()
        {
            LoginCmd = new RelayCommand<object>(p => true, p => Login(p));
        }


        private bool checkInput(object parameter)
        {
            var p = parameter as PasswordBox;
            string? passwd = p?.Password;

            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameHelperText = "Vui lòng nhập tên tài khoản!";
                return false;
            }
            if (string.IsNullOrWhiteSpace(passwd))
            {
                PasswordHelperText = "Vui lòng nhập mật khẩu!";
                return false;
            }

            // Kiểm tra mật khẩu (8 ký tự, ít nhất 1 chữ, 1 số)
            var checkPasswd = new Regex(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$");
            if (!checkPasswd.IsMatch(passwd))
            {
                PasswordHelperText = "Ít nhất 8 ký tự (gồm chữ cái và số)";
                return false;
            }
            return true;
        }

        private async void Login(object parameter)
        {
            if (!checkInput(parameter)) return;

            var p = parameter as PasswordBox;
            string? passwd = p?.Password;

            Icon = IconState.Processing;

            try
            {
                using (var db = new AppDbContext())
                {
                    var admin = db.Admins.FirstOrDefault(x => x.Username == Username);

                    if (admin == null)
                    {
                        Icon = IconState.None;
                        UsernameHelperText = "Tài khoản không tồn tại!";
                        return;
                    }

                    string hashPwd = Services.AuthService.Instance.HashPasswordShort(passwd);

                    if (admin.PasswordHash != hashPwd)
                    {
                        Icon = IconState.None;
                        PasswordHelperText = "Mật khẩu không đúng!";
                        return;
                    }

                    admin.Status = true;
                    db.SaveChanges();
                }


                await Task.Delay(2000);
                Icon = IconState.Success;
                TBStatus = "Đăng nhập thành công!";
                await Task.Delay(1500);
                Icon = IconState.None;
                var win = Window.GetWindow(p);
                if (win != null) { new MainWindow().Show(); win.Close(); }

            }
            catch (Exception ex)
            {
                Icon = IconState.None;
                Application.Current.Dispatcher.Invoke(() =>
                {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var vm = new ConfirmationViewModel($"Lỗi khi đăng nhập: {ex.Message}", "Lỗi", false);
                    new Views.ConfirmationWindow { DataContext = vm }.ShowDialog();
                });
                });
            }

        }
    }
}
