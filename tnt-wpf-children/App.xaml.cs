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
        }
    }

}
