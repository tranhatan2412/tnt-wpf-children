using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        public MainViewModel()
        {
            GoToRelativeCommand = new RelayCommand<object>(p => true, p => CurrentViewModel = new RelativeViewModel());
            GoToSessionCommand = new RelayCommand<object>(p => true, p => CurrentViewModel = new SessionViewModel());

            CurrentViewModel = new RelativeViewModel();
        }
    }
}
