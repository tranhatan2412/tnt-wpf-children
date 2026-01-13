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
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Lỗi khởi tạo Database: {ex.Message}");
                }
            }

            // Open Customer Camera Window Always
            // We need a CameraViewModel. 
            // Note: CameraViewModel starts camera in constructor.
            // Ideally we share one CameraService.
            var cameraVM = new ViewModels.CameraViewModel();
            var customerWindow = new Views.CustomerCameraWindow(cameraVM);
            customerWindow.Show();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            Services.CameraService.Instance.StopCamera();
            base.OnExit(e);
        }
    }

}
