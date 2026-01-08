using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using tnt_wpf_children.ViewModels;

namespace tnt_wpf_children.Views
{
    /// <summary>
    /// Interaction logic for Children.xaml
    /// </summary>
    public partial class Children : UserControl
    {
        public Children()
        {
            InitializeComponent();
            DataContext = new ChildrenViewModel();
        }

    }
}
