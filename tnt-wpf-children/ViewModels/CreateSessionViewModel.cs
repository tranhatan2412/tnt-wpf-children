using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using MaterialDesignThemes.Wpf;
using tnt_wpf_children.Models;

namespace tnt_wpf_children.ViewModels
{
    public class CreateSessionViewModel : BaseViewModel
    {
        private Data.AppDbContext _context;
        public ISnackbarMessageQueue MessageQueue { get; }

        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand SearchRelativeCommand { get; }

        public CreateSessionViewModel() : this(null) { }

        public CreateSessionViewModel(Relatives preSelectedRelative)
        {
            _context = new Data.AppDbContext();
            MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));

            SaveCommand = new RelayCommand<object>(p => true, p => Save());
            CloseCommand = new RelayCommand<Window>(p => p != null, p => p.Close());
            SearchRelativeCommand = new RelayCommand<object>(p => true, p => SearchRelative());

            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(300);
            _timer.Tick += (s, e) => 
            {
                _timer.Stop(); 
                SearchRelative();
            };
            
            if (preSelectedRelative != null)
            {
                var relativeFromContext = _context.Relatives.Find(preSelectedRelative.Id);
                if (relativeFromContext != null)
                {
                    SelectedRelative = relativeFromContext;
                    SearchKeyword = relativeFromContext.ToString();
                    SearchResults.Add(relativeFromContext);
                }
            }
            else
                SearchRelative();
        }

        private System.Windows.Threading.DispatcherTimer _timer;

        private void RestartTimer()
        {
             _timer.Stop();
             _timer.Start();
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set 
            { 
                _searchKeyword = value; 
                OnPropertyChanged(); 
                RestartTimer();
            }
        }

        private Relatives _selectedRelative;
        public Relatives SelectedRelative
        {
            get => _selectedRelative;
            set 
            { 
                if (_selectedRelative != value)
                {
                    _selectedRelative = value; 
                    OnPropertyChanged(); 
                    OnPropertyChanged(nameof(RelativeInfo));
                    OnPropertyChanged(nameof(IsRelativeSelected));
                    if (_selectedRelative != null)
                        IsSearchDropDownOpen = false;
                }
            }
        }

        private bool _isSearchDropDownOpen;
        public bool IsSearchDropDownOpen
        {
            get => _isSearchDropDownOpen;
            set { _isSearchDropDownOpen = value; OnPropertyChanged(); }
        }

        public bool IsRelativeSelected => SelectedRelative != null;
        public string RelativeInfo => SelectedRelative != null ? $"{SelectedRelative.FullName} - {SelectedRelative.PhoneNumber}" : "Chưa chọn người gửi";

        private string _numberOfChildrenText = "1";
        public string NumberOfChildrenText
        {
            get => _numberOfChildrenText;
            set 
            { 
                _numberOfChildrenText = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(NumberOfChildrenHelperText));
            }
        }

        public string NumberOfChildrenHelperText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_numberOfChildrenText))
                    return "Số lượng trẻ không được để trống";
                if (!int.TryParse(_numberOfChildrenText, out int val))
                    return "Chỉ nhập số";
                if (val < 1)
                    return "Số lượng trẻ phải lớn hơn 0";
                if (val > 100)
                    return "Số lượng trẻ quá lớn";
                return string.Empty;
            }
        }

        private string _note;
        public string Note
        {
            get => _note;
            set { _note = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Relatives> SearchResults { get; } = new ObservableCollection<Relatives>();

        private void SearchRelative()
        {
            if (SelectedRelative != null && SearchKeyword == SelectedRelative.ToString())
                return;

            SearchResults.Clear();
            
            IQueryable<Relatives> query = _context.Relatives.Where(r => r.Status == true);

            if (!string.IsNullOrWhiteSpace(SearchKeyword))
                query = query.Where(r => r.PhoneNumber.Contains(SearchKeyword) || r.FullName.Contains(SearchKeyword));
            else
                query = query.OrderByDescending(r => r.CreatedAt);

            var results = query.Take(5).ToList();

            foreach (var r in results) SearchResults.Add(r);
            
            if (!string.IsNullOrWhiteSpace(SearchKeyword) && SearchResults.Count > 0)
                IsSearchDropDownOpen = true;
        }

        private void Save()
        {
            if (SelectedRelative == null)
            {
                MessageQueue.Enqueue("Vui lòng chọn người gửi trẻ");
                return;
            }

            if (!string.IsNullOrEmpty(NumberOfChildrenHelperText))
            {
                MessageQueue.Enqueue(NumberOfChildrenHelperText);
                return;
            }

            if (!string.IsNullOrEmpty(Note) && Note.Length > 500)
            {
                MessageQueue.Enqueue("Ghi chú quá dài (tối đa 500 ký tự)");
                return;
            }

            int numberOfChildren = int.Parse(_numberOfChildrenText);

            try
            {
                var session = new Sessions
                {
                    RelativeId = SelectedRelative.Id,
                    NumberOfChildren = numberOfChildren,
                    CheckinTime = DateTime.Now,
                    Note = Note,
                    Status = true
                };

                _context.Sessions.Add(session);
                _context.SaveChanges();

                MessageQueue.Enqueue("Tạo phiên thành công!");
                
                SelectedRelative = null;
                SearchKeyword = string.Empty;
                NumberOfChildrenText = "1";
                Note = string.Empty;

                Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(1000);
                     foreach (Window win in Application.Current.Windows)
                    {
                        if (win.DataContext == this)
                        {
                            win.Close();
                            break;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageQueue.Enqueue($"Lỗi: {ex.Message}");
            }
        }
    }
}
