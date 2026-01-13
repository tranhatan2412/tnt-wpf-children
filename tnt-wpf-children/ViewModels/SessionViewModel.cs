using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using tnt_wpf_children.Models;

namespace tnt_wpf_children.ViewModels
{
    public class SessionViewModel : BaseViewModel
    {
        public ICommand AddSessionCommand { get; }
        public ICommand RemoveFilterCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteSelectedCommand { get; }

        private Data.AppDbContext _context;
        
        public MaterialDesignThemes.Wpf.ISnackbarMessageQueue MessageQueue { get; }

        public SessionViewModel()
        {
            MessageQueue = new MaterialDesignThemes.Wpf.SnackbarMessageQueue(TimeSpan.FromSeconds(3));
            _context = new Data.AppDbContext();
            // Apply Migrations if needed
            try { _context.Database.Migrate(); } catch { }

            AddSessionCommand = new RelayCommand<object>(p => true, p => OpenAddSession());
            RemoveFilterCommand = new RelayCommand<object>(p => true, p => 
            {
               NameFilter = string.Empty;
               PhoneFilter = string.Empty;
               LoadData();
            });

            DeleteCommand = new RelayCommand<SelectableSessionViewModel>(p => p != null, DeleteSession);
            SaveCommand = new RelayCommand<object>(p => true, p => SaveChanges());
            DeleteSelectedCommand = new RelayCommand<object>(p => true, p => DeleteSelectedSessions());

            Items = new ObservableCollection<SelectableSessionViewModel>();
            LoadData(); 
        }

        private void OpenAddSession()
        {
           var vm = new CreateSessionViewModel();
           var win = new Views.CreateSession { DataContext = vm };
           win.Closed += (s, e) => LoadData();
           win.ShowDialog();
        }

        private void LoadData()
        {
            Items.Clear();
            ExecuteLoadData();
        }

        private void ExecuteLoadData()
        {
            try 
            {
                // Include Relative to get Name/Phone
                var query = _context.Sessions.Include(s => s.Relative).Where(s => s.Status == true);

                if (!string.IsNullOrEmpty(NameFilter))
                {
                   query = query.Where(s => s.Relative.FullName.Contains(NameFilter));
                }
                if (!string.IsNullOrEmpty(PhoneFilter))
                {
                   query = query.Where(s => s.Relative.PhoneNumber.Contains(PhoneFilter));
                }

                var list = query.OrderByDescending(s => s.CheckinTime).ToList();

                foreach (var item in list)
                {
                    var vm = new SelectableSessionViewModel(item);
                    vm.PropertyChanged += Item_PropertyChanged;
                    Items.Add(vm);
                }
            }
            catch (Exception ex)
            {
                MessageQueue.Enqueue($"Lỗi tải dữ liệu: {ex.Message}");
            }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(NameFilter) || propertyName == nameof(PhoneFilter))
            {
                LoadData();
            }
        }

        private void DeleteSession(SelectableSessionViewModel vm)
        {
             // Smart Delete Logic
            if (Items.Any(x => x.IsSelected))
            {
                DeleteSelectedSessions();
                return;
            }

            if (vm == null) return;
            
            var item = _context.Sessions.Find(vm.Model.Id);
            if (item != null)
            {
                item.Status = false;
                _context.SaveChanges();
                Items.Remove(vm);

                MessageQueue.Enqueue(
                    $"Đã xóa phiên của '{vm.RelativeName}'", 
                    "HOÀN TÁC", 
                    param => UndoDelete(param as SelectableSessionViewModel), 
                    vm);
            }
        }

        private void UndoDelete(SelectableSessionViewModel vm)
        {
            if (vm == null) return;
            var item = _context.Sessions.Find(vm.Model.Id);
            if (item != null)
            {
                item.Status = true;
                _context.SaveChanges();
                LoadData(); 
            }
        }

        private void DeleteSelectedSessions()
        {
            var selected = Items.Where(x => x.IsSelected).ToList();
            if (selected.Count == 0) return;

            var deletedIds = new List<string>();

            foreach(var item in selected)
            {
                var dbItem = _context.Sessions.Find(item.Model.Id);
                if (dbItem != null)
                {
                    dbItem.Status = false;
                    deletedIds.Add(dbItem.Id);
                }
                Items.Remove(item);
            }
            _context.SaveChanges();

            MessageQueue.Enqueue(
                $"Đã xóa {selected.Count} mục",
                "HOÀN TÁC",
                param => UndoDeleteMultiple(param as List<string>),
                deletedIds);
        }

        private void UndoDeleteMultiple(List<string> ids)
        {
            if (ids == null || ids.Count == 0) return;

            foreach(var id in ids)
            {
                var item = _context.Sessions.Find(id);
                if (item != null) item.Status = true;
            }
            _context.SaveChanges();
            LoadData();
        }

        private void SaveChanges()
        {
            try
            {
                _context.SaveChanges();
                MessageQueue.Enqueue("Đã lưu thay đổi!");
            }
            catch (Exception ex)
            {
                MessageQueue.Enqueue($"Lỗi khi lưu: {ex.Message}");
            }
        }

        private string _nameFilter;
        public string NameFilter
        {
            get => _nameFilter;
            set { _nameFilter = value; OnPropertyChanged(); }
        }

        private string _phoneFilter;
        public string PhoneFilter
        {
            get => _phoneFilter;
            set { _phoneFilter = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SelectableSessionViewModel> Items { get; }

        #region Select All Logic 

        public bool? IsAllItemsSelected
        {
            get
            {
                var states = Items.Select(x => x.IsSelected).Distinct().ToList();
                return states.Count == 1 ? states[0] : (bool?)null;
            }
            set
            {
                if (value.HasValue)
                {
                    foreach (var item in Items)
                        item.IsSelected = value.Value;

                    OnPropertyChanged();
                }
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectableSessionViewModel.IsSelected))
                OnPropertyChanged(nameof(IsAllItemsSelected));
        }

        #endregion
    }

    public class SelectableSessionViewModel : BaseViewModel
    {
        private bool _isSelected;

        public SelectableSessionViewModel(Sessions model)
        {
            Model = model;
        }

        public Sessions Model { get; }

        public string RelativeName => Model.Relative?.FullName ?? "Unknown";
        public string RelativePhone => Model.Relative?.PhoneNumber ?? "";

        public int? NumberOfChildren
        {
            get => Model.NumberOfChildren;
            set
            {
                if (Model.NumberOfChildren == value) return;
                Model.NumberOfChildren = value;
                OnPropertyChanged();
            }
        }

        public DateTime CheckinTime => Model.CheckinTime;
        
        public DateTime? CheckoutTime
        {
            get => Model.CheckoutTime;
            set
            {
                if (Model.CheckoutTime == value) return;
                Model.CheckoutTime = value;
                OnPropertyChanged();
            }
        }

        public string? Note
        {
            get => Model.Note;
            set
            {
                if (Model.Note == value) return;
                Model.Note = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
}
