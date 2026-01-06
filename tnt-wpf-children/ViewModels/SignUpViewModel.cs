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
    public class SignUpViewModel : BaseViewModel
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
        private string _passwordHelperText = "Ít nhất 8 ký tự (gồm chữ cái và số)";
        public string PasswordHelperText
        {
            get => _passwordHelperText;
            set { _passwordHelperText = value; OnPropertyChanged(); }
        }

        private string _rePasswordHelperText;
        public string RePasswordHelperText
        {
            get => _rePasswordHelperText;
            set { _rePasswordHelperText = value; OnPropertyChanged(); }
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
            set { _icon = value; OnPropertyChanged();
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

        public ICommand SignUpCmd { get; }

        

        public SignUpViewModel()
        {
            SignUpCmd = new RelayCommand<object>(p => true, p => SignUp(p));
        }
        

        private bool checkInput(object parameter)
        {
            
            var container = parameter as StackPanel;
            var p1 = container.Children.OfType<PasswordBox>().FirstOrDefault(x => x.Name == "PasswdBox");
            var p2 = container.Children.OfType<PasswordBox>().FirstOrDefault(x => x.Name == "Re_PasswdBox");
            string? passwd = p1?.Password;
            string? re_passwd = p2?.Password;

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
            if (string.IsNullOrWhiteSpace(re_passwd))
            {
                RePasswordHelperText = "Vui lòng nhập lại mật khẩu!";
                return false;
            }

            // Kiểm tra mật khẩu (8 ký tự, ít nhất 1 chữ, 1 số)
            var checkPasswd = new Regex(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$");
            if (!checkPasswd.IsMatch(passwd))
            {
                PasswordHelperText = "Ít nhất 8 ký tự (gồm chữ cái và số)";
                return false;
            }
            if (passwd != re_passwd)
            {
                RePasswordHelperText = "Mật khẩu nhập lại không khớp!";
                return false;
            }
            return true;
        }

        private async void SignUp(object parameter)
        {
            if (!checkInput(parameter)) return;

           
            var container = parameter as StackPanel;
            var p1 = container.Children.OfType<PasswordBox>().FirstOrDefault(x => x.Name == "PasswdBox");
            string passwd = p1?.Password ?? string.Empty;

            Icon = IconState.Processing;

            try
            {
                using (var db = new AppDbContext())
                {
                    bool isExisted = db.Admins.Any(x => x.Username == Username);
                    if (isExisted)
                    {
                        UsernameHelperText = "Tài khoản đã tồn tại";
                        Icon = IconState.None;
                        return;
                    }

                    var admin = new Admin
                    {
                        Username = Username,
                        PasswordHash = HashPasswordShort(passwd)
                    };

                    db.Admins.Add(admin);
                    await db.SaveChangesAsync();
                }

                
                await Task.Delay(2000);
                Icon = IconState.Success;
                TBStatus = "Đăng ký thành công!";
                await Task.Delay(1500);
                Icon = IconState.None;
                var win = Window.GetWindow(container);
                if (win != null){new MainWindow().Show(); win.Close();}
                
            }
            catch (Exception ex)
            {
                Icon = IconState.None;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Lỗi khi đăng ký: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            
        }

        private string HashPasswordShort(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hex = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            
            return hex.Substring(0, Math.Min(20, hex.Length));
        }
    }
}
