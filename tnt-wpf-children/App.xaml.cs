using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Data;
using System.Windows;
using tnt_wpf_children.Data;
namespace tnt_wpf_children
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            
            using (var dbContext = new AppDbContext())
            {
                try
                {
                    dbContext.Database.Migrate();
                    
                    // Kiểm tra phiên đăng nhập của Admin
                    bool isLoggedIn = false;
                    try
                    {
                        isLoggedIn = System.Linq.Enumerable.Any(dbContext.Admins, a => a.Status == true);
                    }
                    catch (System.Exception) { }

                    if (isLoggedIn)
                         new MainWindow().Show();
                    else
                         new Views.Login().Show();
                }
                catch (System.Exception ex)
                {
                    var vm = new ViewModels.ConfirmationViewModel($"Lỗi khởi tạo Database: {ex.Message}", "Lỗi", false);
                    new Views.ConfirmationWindow { DataContext = vm }.ShowDialog();
                    new Views.Login().Show();
                }
            }


        }
        protected override void OnExit(ExitEventArgs e)
        {
            Services.CameraService.Instance.StopCamera();
            base.OnExit(e);
        }
    }

}
