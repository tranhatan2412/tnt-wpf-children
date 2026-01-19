using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore; // Fixed missing using
using System.Windows;
using tnt_wpf_children.Models;

namespace tnt_wpf_children.ViewModels
{
    public class RelativeViewModel : BaseViewModel
    {
        public ICommand AddRelativeCommand { get; }
        public ICommand RemoveFilterCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteSelectedCommand { get; }
        public ICommand CreateSessionCommand { get; }

        private Data.AppDbContext _context;
        public MaterialDesignThemes.Wpf.ISnackbarMessageQueue MessageQueue { get; }
        private System.Windows.Threading.DispatcherTimer _timer;

        public RelativeViewModel()
        {
            MessageQueue = new MaterialDesignThemes.Wpf.SnackbarMessageQueue(TimeSpan.FromSeconds(3));
            _context = new Data.AppDbContext();
            try { _context.Database.Migrate(); } catch { }

            // Debounce Timer
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(300);
            _timer.Tick += (s, e) => 
            {
                _timer.Stop();
                LoadData();
            };

            AddRelativeCommand = new RelayCommand<object>(p => true, p => OpenAddRelative());
            RemoveFilterCommand = new RelayCommand<object>(p => true, p => 
            {
               _nameFilter = string.Empty;
               _phoneFilter = string.Empty;
               OnPropertyChanged(nameof(NameFilter));
               OnPropertyChanged(nameof(PhoneFilter));
               LoadData();
            });

            DeleteCommand = new RelayCommand<SelectableRelativeViewModel>(p => p != null, DeleteRelative);
            SaveCommand = new RelayCommand<object>(p => true, p => SaveChanges());
            DeleteSelectedCommand = new RelayCommand<object>(p => true, p => DeleteSelectedRelatives());
            CreateSessionCommand = new RelayCommand<SelectableRelativeViewModel>(p => p != null, OpenCreateSession);
            EditNoteCommand = new RelayCommand<SelectableRelativeViewModel>(p => p != null, EditNote);

            Items1 = new ObservableCollection<SelectableRelativeViewModel>();
            LoadData(); 
        }

        public ICommand EditNoteCommand { get; }

        private void EditNote(SelectableRelativeViewModel vm)
        {
            if (vm == null) return;

            var noteVm = new NoteEditViewModel(vm.Note);
            var noteWin = new Views.NoteEditWindow { DataContext = noteVm, Owner = Application.Current.MainWindow };

            if (noteWin.ShowDialog() == true)
                vm.Note = noteVm.NoteContent;
        }

        private void OpenCreateSession(SelectableRelativeViewModel vm)
        {
            if (vm == null) return;
            
            var createSessionVm = new CreateSessionViewModel(vm.Model);
            var createSessionWindow = new Views.CreateSession();
            createSessionWindow.DataContext = createSessionVm;
            createSessionWindow.Show();
        }

        private void OpenAddRelative()
        {
            var vm = new CreateRelativeViewModel();
            var employeeWindow = new Views.CreateRelative();
            employeeWindow.DataContext = vm;
            employeeWindow.Closed += (s, e) => LoadData();
            employeeWindow.Show();
        }

        private void LoadData()
        {
            Items1.Clear();
            ExecuteLoadData();
        }

        private void ExecuteLoadData()
        {
            try 
            {
                var query = _context.Relatives.Where(r => r.Status == true);

                if (!string.IsNullOrEmpty(NameFilter))
                   query = query.Where(r => r.FullName.Contains(NameFilter));
                if (!string.IsNullOrEmpty(PhoneFilter))
                   query = query.Where(r => r.PhoneNumber.Contains(PhoneFilter));

                var list = query.OrderByDescending(r => r.CreatedAt).ToList();

                foreach (var item in list)
                {
                    var vm = new SelectableRelativeViewModel(item);
                    vm.PropertyChanged += Item_PropertyChanged;
                    Items1.Add(vm);
                }
            }
            catch (Exception ex)
            {
                MessageQueue.Enqueue($"Lỗi tải dữ liệu: {ex.Message}");
            }
        }

        private void RestartTimer()
        {
             _timer.Stop();
             _timer.Start();
        }

        private string _nameFilter;
        public string NameFilter
        {
            get => _nameFilter;
            set 
            { 
                _nameFilter = value; 
                OnPropertyChanged();
                RestartTimer();
            }
        }

        private string _phoneFilter;
        public string PhoneFilter
        {
            get => _phoneFilter;
            set 
            { 
                _phoneFilter = value; 
                OnPropertyChanged();
                RestartTimer();
            }
        }

        private void DeleteRelative(SelectableRelativeViewModel vm)
        {
            if (Items1.Any(x => x.IsSelected))
            {
                DeleteSelectedRelatives();
                return;
            }

            if (vm == null) return;
            
            var confirmVm = new PasswordConfirmationViewModel($"Bạn có chắc chắn muốn xóa '{vm.FullName}'?");
            var confirmWin = new Views.PasswordConfirmationWindow();
            confirmWin.DataContext = confirmVm;
            confirmWin.Owner = Application.Current.MainWindow;
            
            if (confirmWin.ShowDialog() != true) return;
            
            var item = _context.Relatives.Find(vm.Model.Id);
            if (item != null)
            {
                item.Status = false;
                _context.SaveChanges();
                Items1.Remove(vm);

                MessageQueue.Enqueue(
                    $"Đã xóa '{vm.FullName}'", 
                    "HOÀN TÁC", 
                    param => UndoDelete(param as SelectableRelativeViewModel), 
                    vm);
            }
        }

        private void UndoDelete(SelectableRelativeViewModel vm)
        {
            if (vm == null) return;
            var item = _context.Relatives.Find(vm.Model.Id);
            if (item != null)
            {
                item.Status = true;
                _context.SaveChanges();
                LoadData(); 
            }
        }

        private void DeleteSelectedRelatives()
        {
            var selected = Items1.Where(x => x.IsSelected).ToList();
            if (selected.Count == 0) return;

            var confirmVm = new PasswordConfirmationViewModel($"Bạn có chắc chắn muốn xóa {selected.Count} dòng đã chọn?");
            var confirmWin = new Views.PasswordConfirmationWindow();
            confirmWin.DataContext = confirmVm;
            confirmWin.Owner = Application.Current.MainWindow;
            
            if (confirmWin.ShowDialog() != true) return;

            var deletedIds = new List<string>();

            foreach(var item in selected)
            {
                var dbItem = _context.Relatives.Find(item.Model.Id);
                if (dbItem != null)
                {
                    dbItem.Status = false;
                    deletedIds.Add(dbItem.Id);
                }
                Items1.Remove(item);
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
                var item = _context.Relatives.Find(id);
                if (item != null) item.Status = true;
            }
            _context.SaveChanges();
            LoadData();
        }

        private void SaveChanges()
        {
            var emptyFields = new List<string>();
            var invalidFields = new List<string>();
            
            foreach (var item in Items1)
            {
                if (string.IsNullOrWhiteSpace(item.FullName))
                    emptyFields.Add("Họ và tên");
                
                if (string.IsNullOrWhiteSpace(item.PhoneNumber))
                    emptyFields.Add("Số điện thoại");
                
                var nameError = (item as IDataErrorInfo)?["FullName"];
                if (!string.IsNullOrEmpty(nameError) && !string.IsNullOrWhiteSpace(item.FullName))
                    invalidFields.Add("Họ và tên");
                
                var phoneError = (item as IDataErrorInfo)?["PhoneNumber"];
                if (!string.IsNullOrEmpty(phoneError) && !string.IsNullOrWhiteSpace(item.PhoneNumber))
                    invalidFields.Add("Số điện thoại");
            }
            
            if (emptyFields.Count > 0)
            {
                var uniqueFields = string.Join(", ", emptyFields.Distinct());
                MessageQueue.Enqueue($"{uniqueFields} không được để trống");
                return;
            }
            
            if (invalidFields.Count > 0)
            {
                MessageQueue.Enqueue("Không thể lưu vì dữ liệu nhập vào không đúng");
                return;
            }
            
            try
            {
                var modifiedEntities = _context.ChangeTracker.Entries<Relatives>()
                    .Where(x => x.State == EntityState.Modified)
                    .ToList();

                foreach (var entry in modifiedEntities)
                    entry.Entity.UpdatedAt = DateTime.Now;

                _context.SaveChanges();
                MessageQueue.Enqueue("Đã lưu thay đổi!");
            }
            catch (Exception ex)
            {
               MessageQueue.Enqueue($"Lỗi khi lưu: {ex.Message}");
            }
        }

        public ObservableCollection<SelectableRelativeViewModel> Items1 { get; }

        public bool? IsAllItems1Selected
        {
            get
            {
                var states = Items1.Select(x => x.IsSelected).Distinct().ToList();
                return states.Count == 1 ? states[0] : (bool?)null;
            }
            set
            {
                if (value.HasValue)
                {
                    foreach (var item in Items1)
                        item.IsSelected = value.Value;
                    OnPropertyChanged();
                }
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectableRelativeViewModel.IsSelected))
                OnPropertyChanged(nameof(IsAllItems1Selected));
        }
    }

    public class SelectableRelativeViewModel : BaseViewModel, IDataErrorInfo
    {
        private bool _isSelected;
        public SelectableRelativeViewModel(Relatives model) => Model = model;
        public Relatives Model { get; }

        public string FullName
        {
            get => Model.FullName;
            set { if (Model.FullName != value) { Model.FullName = value; OnPropertyChanged(); } }
        }

        public string PhoneNumber
        {
            get => Model.PhoneNumber;
            set { if (Model.PhoneNumber != value) { Model.PhoneNumber = value; OnPropertyChanged(); } }
        }
        public DateTime CreatedAt => Model.CreatedAt;
        public DateTime UpdatedAt => Model.UpdatedAt;
        public string? Note
        {
            get => Model.Note;
            set { if (Model.Note != value) { Model.Note = value; OnPropertyChanged(); } }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(); } }
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(PhoneNumber))
                {
                    if (!string.IsNullOrWhiteSpace(PhoneNumber) && !System.Text.RegularExpressions.Regex.IsMatch(PhoneNumber, @"^\d+$"))
                    {
                        return "Số điện thoại chỉ được chứa chữ số";
                    }
                    if (!string.IsNullOrWhiteSpace(PhoneNumber) && PhoneNumber.Length > 15)
                    {
                        return "Số điện thoại quá dài";
                    }
                }
                return null;
            }
        }
    }
}
