using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace tnt_wpf_children.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(); }
        }

        public ICommand GoToRelativeCommand { get; }
        public ICommand GoToSessionCommand { get; }
        public ICommand GoToStatisticsCommand { get; }

        public MainViewModel()
        {
            GoToRelativeCommand = new RelayCommand<object>(p => true, p => 
            {
                try { CurrentViewModel = new RelativeViewModel(); }
                catch (Exception ex) 
                {
                    var vm = new ConfirmationViewModel($"Lỗi khi mở trang Người gửi: {ex.Message}", "Lỗi", false);
                    new Views.ConfirmationWindow { DataContext = vm }.ShowDialog();
                }
            });

            GoToSessionCommand = new RelayCommand<object>(p => true, p => 
            {
                try { CurrentViewModel = new SessionViewModel(); }
                catch (Exception ex) 
                {
                    var vm = new ConfirmationViewModel($"Lỗi khi mở trang Phiên gửi: {ex.Message}", "Lỗi", false);
                    new Views.ConfirmationWindow { DataContext = vm }.ShowDialog();
                }
            });

            GoToStatisticsCommand = new RelayCommand<object>(p => true, p => 
            {
                try { CurrentViewModel = new StatisticsViewModel(); }
                catch (Exception ex) 
                {
                    var vm = new ConfirmationViewModel($"Lỗi khi mở trang Thống kê: {ex.Message}", "Lỗi", false);
                    new Views.ConfirmationWindow { DataContext = vm }.ShowDialog();
                }
            });

            LogoutCommand = new RelayCommand<object>(p => true, p => Logout(p));

            try { CurrentViewModel = new SessionViewModel(); } catch { }
        }

        public ICommand LogoutCommand { get; }

        private void Logout(object parameter)
        {
            var confirmVm = new ConfirmationViewModel("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất");
            var confirmWin = new Views.ConfirmationWindow();
            confirmWin.DataContext = confirmVm;
            if (parameter is Window ownerWindow)
                confirmWin.Owner = ownerWindow;
            else if (Application.Current.MainWindow != null)
                 confirmWin.Owner = Application.Current.MainWindow;

            if (confirmWin.ShowDialog() != true) return;

            try
            {
                using (var db = new Data.AppDbContext())
                {
                    var admins = db.Admins.Where(a => a.Status == true).ToList();
                    foreach (var admin in admins)
                        admin.Status = false;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var vm = new ConfirmationViewModel($"Lỗi cập nhật trạng thái đăng xuất: {ex.Message}", "Lỗi", false);
                new Views.ConfirmationWindow { DataContext = vm }.ShowDialog();
            }

            var loginWindow = new Views.Login();
            loginWindow.Show();
            
            if (parameter is Window window)
                window.Close();
            else
            {
                foreach (Window win in System.Windows.Application.Current.Windows)
                {
                    if (win is MainWindow)
                    {
                        win.Close();
                        break;
                    }
                }
            }
        }
    }
}
