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

        public CreateSessionViewModel()
        {
            _context = new Data.AppDbContext();
            MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));

            SaveCommand = new RelayCommand<object>(p => true, p => Save());
            CloseCommand = new RelayCommand<Window>(p => p != null, p => p.Close());
            SearchRelativeCommand = new RelayCommand<object>(p => true, p => SearchRelative());
        }

        // Search Fields
        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { _searchKeyword = value; OnPropertyChanged(); }
        }

        private Relatives _selectedRelative;
        public Relatives SelectedRelative
        {
            get => _selectedRelative;
            set 
            { 
                _selectedRelative = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(RelativeInfo));
                OnPropertyChanged(nameof(IsRelativeSelected));
            }
        }

        public bool IsRelativeSelected => SelectedRelative != null;
        public string RelativeInfo => SelectedRelative != null ? $"{SelectedRelative.FullName} - {SelectedRelative.PhoneNumber}" : "Chưa chọn người gửi";

        // Session Fields
        private int _numberOfChildren = 1;
        public int NumberOfChildren
        {
            get => _numberOfChildren;
            set { _numberOfChildren = value; OnPropertyChanged(); }
        }

        private string _note;
        public string Note
        {
            get => _note;
            set { _note = value; OnPropertyChanged(); }
        }

        // Search Logic
        public ObservableCollection<Relatives> SearchResults { get; } = new ObservableCollection<Relatives>();

        private void SearchRelative()
        {
            SearchResults.Clear();
            if (string.IsNullOrWhiteSpace(SearchKeyword)) return;

            var results = _context.Relatives
                .Where(r => r.Status == true && (r.PhoneNumber.Contains(SearchKeyword) || r.FullName.Contains(SearchKeyword)))
                .Take(5)
                .ToList();

            foreach (var r in results) SearchResults.Add(r);
            
            if (SearchResults.Count == 0) MessageQueue.Enqueue("Không tìm thấy người gửi");
        }

        private void Save()
        {
            if (SelectedRelative == null)
            {
                MessageQueue.Enqueue("Vui lòng chọn người gửi");
                return;
            }

            try
            {
                var session = new Sessions
                {
                    RelativeId = SelectedRelative.Id,
                    NumberOfChildren = NumberOfChildren,
                    CheckinTime = DateTime.UtcNow,
                    Note = Note,
                    Status = true
                };

                _context.Sessions.Add(session);
                _context.SaveChanges();

                MessageQueue.Enqueue("Tạo phiên thành công!");

                // Close window after short delay
                // Note: In real MVVM, we might use an interaction service
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
