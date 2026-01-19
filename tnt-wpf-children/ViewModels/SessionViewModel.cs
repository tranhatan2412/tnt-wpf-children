using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using System.Windows;
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
        public ICommand CheckoutCommand { get; }

        private Data.AppDbContext _context;
        public MaterialDesignThemes.Wpf.ISnackbarMessageQueue MessageQueue { get; }
        private System.Windows.Threading.DispatcherTimer _timer;

        public SessionViewModel()
        {
            MessageQueue = new MaterialDesignThemes.Wpf.SnackbarMessageQueue(TimeSpan.FromSeconds(3));
            _context = new Data.AppDbContext();
            try { _context.Database.Migrate(); } catch { }

            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(300);
            _timer.Tick += (s, e) => 
            {
                _timer.Stop();
                LoadData();
            };

            AddSessionCommand = new RelayCommand<object>(p => true, p => OpenAddSession());
            RemoveFilterCommand = new RelayCommand<object>(p => true, p => 
            {
               _nameFilter = string.Empty;
               _phoneFilter = string.Empty;
               OnPropertyChanged(nameof(NameFilter));
               OnPropertyChanged(nameof(PhoneFilter));
               LoadData();
            });

            DeleteCommand = new RelayCommand<SelectableSessionViewModel>(p => p != null, DeleteSession);
            SaveCommand = new RelayCommand<object>(p => true, p => SaveChanges());
            DeleteSelectedCommand = new RelayCommand<object>(p => true, p => DeleteSelectedSessions());
            CheckoutCommand = new RelayCommand<object>(p => true, p => OpenCheckoutCamera());
            PrintReceiptCommand = new RelayCommand<SelectableSessionViewModel>(p => p != null, OpenPrintReceipt);
            EditNoteCommand = new RelayCommand<SelectableSessionViewModel>(p => p != null, EditNote);

            Items = new ObservableCollection<SelectableSessionViewModel>();
            LoadData(); 
        }

        public ICommand PrintReceiptCommand { get; }
        public ICommand EditNoteCommand { get; }

        private void EditNote(SelectableSessionViewModel vm)
        {
            if (vm == null) return;
            
            var noteVm = new NoteEditViewModel(vm.Note);
            var noteWin = new Views.NoteEditWindow { DataContext = noteVm, Owner = Application.Current.MainWindow };
            
            if (noteWin.ShowDialog() == true)
                vm.Note = noteVm.NoteContent;
        }

        private void OpenPrintReceipt(SelectableSessionViewModel vm)
        {
            if (vm == null) return;
            
            var checkoutVm = new CheckoutReceiptViewModel(vm.Model, () => { });
            
            var receiptWindow = new Views.CheckoutReceiptWindow();
            receiptWindow.Owner = Application.Current.MainWindow;
            receiptWindow.DataContext = checkoutVm;
            receiptWindow.ShowDialog();
        }

        private void OpenCheckoutCamera()
        {
            var cameraVm = new CameraViewModel();
            
            var employeeWindow = new Views.CustomerCameraWindow(cameraVm);
            employeeWindow.Owner = Application.Current.MainWindow;
            employeeWindow.Title = "Camera (Nhân viên)";

            var customerWindow = new Views.CustomerCameraWindow(cameraVm);
            customerWindow.Title = "Camera (Khách hàng)";
            customerWindow.Show();
            
            SelectableSessionViewModel? matchedSession = null;
            
            cameraVm.OnFaceRecognized += (capturedEmbedding) =>
            {
                var faceService = Services.FaceRecognitionService.Instance;
                
                var activeSessions = _context.Sessions
                    .Include(s => s.Relative)
                    .Where(s => s.Status == true)
                    .ToList();

                foreach (var sessionModel in activeSessions)
                {
                    var storedBytes = sessionModel.Relative?.FaceEmbedding;
                    if (storedBytes == null) continue;
                    
                    var storedEmbedding = faceService.BytesToEmbedding(storedBytes);
                    if (storedEmbedding == null) continue;
                    
                    if (faceService.IsFaceMatch(capturedEmbedding, storedEmbedding))
                    {
                        var existingVm = Items.FirstOrDefault(x => x.Model.Id == sessionModel.Id);
                        matchedSession = existingVm ?? new SelectableSessionViewModel(sessionModel);
                        
                        MessageQueue.Enqueue($"Nhận diện thành công khách hàng {sessionModel.Relative.FullName}");
                        employeeWindow.Close(); 
                        customerWindow.Close();
                        cameraVm.Cleanup();
                        return;
                    }
                }
                
                MessageQueue.Enqueue("Không tìm thấy khách hàng");
            };
            
            employeeWindow.ShowDialog();
            
            customerWindow.Close();
            
            cameraVm.Cleanup();
            
            if (matchedSession != null)
                ShowCheckoutReceipt(matchedSession);
        }

        private void ShowCheckoutReceipt(SelectableSessionViewModel vm)
        {
            if (vm == null) return;
            
            var checkoutVm = new CheckoutReceiptViewModel(vm.Model, () => 
            {
                Items.Remove(vm);
                MessageQueue.Enqueue("Đã xác nhận nhận trẻ thành công!");
            });
            
            var receiptWindow = new Views.CheckoutReceiptWindow();
            receiptWindow.Owner = Application.Current.MainWindow;
            receiptWindow.DataContext = checkoutVm;
            receiptWindow.ShowDialog();
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
                var query = _context.Sessions.Include(s => s.Relative).Where(s => s.Status == true);

                if (!string.IsNullOrEmpty(NameFilter))
                   query = query.Where(s => s.Relative.FullName.Contains(NameFilter));
                if (!string.IsNullOrEmpty(PhoneFilter))
                   query = query.Where(s => s.Relative.PhoneNumber.Contains(PhoneFilter));

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

        private void DeleteSession(SelectableSessionViewModel vm)
        {
            if (Items.Any(x => x.IsSelected))
            {
                DeleteSelectedSessions();
                return;
            }

            if (vm == null) return;
            
            var confirmVm = new PasswordConfirmationViewModel($"Bạn có chắc chắn muốn xóa phiên gửi của khách hàng '{vm.RelativeName}'?");
            var confirmWin = new Views.PasswordConfirmationWindow();
            confirmWin.DataContext = confirmVm;
            confirmWin.Owner = Application.Current.MainWindow;
            
            if (confirmWin.ShowDialog() != true) return;
            
            var item = _context.Sessions.Find(vm.Model.Id);
            if (item != null)
            {
                item.Status = false;
                _context.SaveChanges();
                Items.Remove(vm);

                MessageQueue.Enqueue(
                    $"Đã xóa phiên gửi của khách hàng '{vm.RelativeName}'", 
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

            var confirmVm = new PasswordConfirmationViewModel($"Bạn có chắc chắn muốn xóa {selected.Count} phiên gửi đã chọn?");
            var confirmWin = new Views.PasswordConfirmationWindow();
            confirmWin.DataContext = confirmVm;
            confirmWin.Owner = Application.Current.MainWindow;
            
            if (confirmWin.ShowDialog() != true) return;

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
            var emptyFields = new List<string>();
            var invalidFields = new List<string>();
            
            foreach (var item in Items)
            {
                var numberError = (item as IDataErrorInfo)?["NumberOfChildrenText"];
                if (!string.IsNullOrEmpty(numberError))
                    invalidFields.Add("Số lượng trẻ");
                if (item.Model.NumberOfChildren == null)
                    invalidFields.Add("Số lượng trẻ");
                var noteError = (item as IDataErrorInfo)?["Note"];
                if (!string.IsNullOrEmpty(noteError))
                    invalidFields.Add("Ghi chú");
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
                _context.SaveChanges();
                MessageQueue.Enqueue("Đã lưu thay đổi!");
            }
            catch (Exception ex)
            {
                MessageQueue.Enqueue($"Lỗi khi lưu: {ex.Message}");
            }
        }

        public ObservableCollection<SelectableSessionViewModel> Items { get; }

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
    }

    public class SelectableSessionViewModel : BaseViewModel, IDataErrorInfo
    {
        private bool _isSelected;
        private string _numberOfChildrenText;

        public SelectableSessionViewModel(Sessions model)
        {
            Model = model;
            _numberOfChildrenText = model.NumberOfChildren?.ToString() ?? "1";
        }

        public Sessions Model { get; }

        public string RelativeName => Model.Relative?.FullName ?? "Unknown";
        public string RelativePhone => Model.Relative?.PhoneNumber ?? "";

        public string NumberOfChildrenText
        {
            get => _numberOfChildrenText;
            set 
            { 
                if (_numberOfChildrenText != value)
                {
                    _numberOfChildrenText = value;
                    OnPropertyChanged();
                    
                    if (int.TryParse(value, out int num) && num >= 1)
                        Model.NumberOfChildren = num;
                    else
                        Model.NumberOfChildren = null;
                }
            }
        }

        public DateTime CheckinTime => Model.CheckinTime;
        
        public DateTime? CheckoutTime
        {
            get => Model.CheckoutTime;
            set { if (Model.CheckoutTime != value) { Model.CheckoutTime = value; OnPropertyChanged(); } }
        }

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
                if (columnName == nameof(NumberOfChildrenText))
                {
                    if (string.IsNullOrWhiteSpace(_numberOfChildrenText))
                        return "Không được để trống";
                    if (!int.TryParse(_numberOfChildrenText, out int val))
                        return "Chỉ nhập số";
                    if (val < 1) 
                        return "Số lượng trẻ phải lớn hơn 0";
                    if (val > 100) 
                        return "Số lượng trẻ quá lớn";
                }
                if (columnName == nameof(Note))
                {
                    if (Note != null && Note.Length > 500) return "Ghi chú quá dài";
                }
                return null;
            }
        }
    }
}
