using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Printing;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using tnt_wpf_children.Data;
using tnt_wpf_children.Models;

namespace tnt_wpf_children.ViewModels
{
    public class CheckoutReceiptViewModel : BaseViewModel
    {
        private readonly Sessions _session;
        private readonly AppDbContext _context;
        private readonly Action _onCheckoutComplete;

        public ISnackbarMessageQueue MessageQueue { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand PrintCommand { get; }

        public CheckoutReceiptViewModel(Sessions session, Action onCheckoutComplete)
        {
            _context = new AppDbContext();
            _session = _context.Sessions
                .Include(s => s.Relative)
                .FirstOrDefault(s => s.Id == session.Id);

            _onCheckoutComplete = onCheckoutComplete;
            
            MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));
            
            CheckoutTime = DateTime.Now;

            ConfirmCommand = new RelayCommand<Window>(p => p != null, Confirm);
            CloseCommand = new RelayCommand<Window>(p => p != null, CloseWithConfirmation);
            PrintCommand = new RelayCommand<Window>(p => p != null, PrintReceipt);
        }

        public string CustomerName => _session?.Relative?.FullName ?? "Không xác định";
        public string PhoneNumber => _session?.Relative?.PhoneNumber ?? "";
        public DateTime CheckinTime => _session?.CheckinTime ?? DateTime.MinValue;
        public DateTime CheckoutTime { get; private set; }
        public int NumberOfChildren => _session?.NumberOfChildren ?? 0;
        public string Note => _session?.Note ?? "";
        



        private void Confirm(Window window)
        {
            if (_session != null)
            {
                _session.CheckoutTime = CheckoutTime;
                _session.Status = false;
                _context.SaveChanges();
            }

            _onCheckoutComplete?.Invoke();

            window?.Close();
        }

        private void PrintReceipt(Window window)
        {
            try
            {
                var printArea = FindPrintArea(window);
                if (printArea == null)
                {
                    var vm = new ConfirmationViewModel("Không tìm thấy nội dung để in.", "Lỗi", false);
                    new Views.ConfirmationWindow { DataContext = vm, Owner = window }.ShowDialog();
                    return;
                }

                PrintDialog printDialog = new();
                if (printDialog.ShowDialog() == true)
                    printDialog.PrintVisual(printArea, "Phiếu nhận trẻ - " + CustomerName);
            }
            catch (Exception ex)
            {
                var vm = new ConfirmationViewModel($"Lỗi khi in: {ex.Message}", "Lỗi", false);
                new Views.ConfirmationWindow { DataContext = vm, Owner = window }.ShowDialog();
            }
        }

        private Visual FindPrintArea(Window window)
        {
            if (window != null)
            {
                var printArea = window.FindName("PrintArea") as Visual;
                return printArea;
            }
            return null;
        }

        private void CloseWithConfirmation(Window window)
        {
            window?.Close();
        }
    }
}
