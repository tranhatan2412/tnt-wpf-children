using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using tnt_wpf_children.Data;
using tnt_wpf_children.Models;

namespace tnt_wpf_children.ViewModels
{
    public class CreateRelativeViewModel : BaseViewModel
    {
        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(); }
        }

        private string _note;
        public string Note
        {
            get => _note;
            set { _note = value; OnPropertyChanged(); }
        }

        public byte[] Face { get; private set; }

        private string _fullNameHelper;
        public string FullNameHelper
        {
            get => _fullNameHelper;
            set { _fullNameHelper = value; OnPropertyChanged(); }
        }

        private string _phoneHelper;
        public string PhoneHelper
        {
            get => _phoneHelper;
            set { _phoneHelper = value; OnPropertyChanged(); }
        }

        public ISnackbarMessageQueue MessageQueue { get; }

        public ICommand SaveCmd { get; }
        public ICommand CaptureFaceCmd { get; }
        public ICommand OpenCustomerWindowCmd { get; }

        public CameraViewModel CameraVM { get; }

        public CreateRelativeViewModel()
        {
            MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));
            CameraVM = new CameraViewModel();
            SaveCmd = new RelayCommand<object>(p => true, p => Save());
            CaptureFaceCmd = new RelayCommand<object>(p => true, p => CaptureFace());
            OpenCustomerWindowCmd = new RelayCommand<object>(p => true, p => OpenCustomerWindow());
        }

        private void OpenCustomerWindow()
        {
            foreach (Window win in Application.Current.Windows)
            {
                if (win is Views.CustomerCameraWindow)
                {
                    win.WindowState = WindowState.Normal;
                    win.Activate();
                    return;
                }
            }

            var customerWindow = new Views.CustomerCameraWindow(CameraVM);
            customerWindow.Show();
        }

        private bool Validate()
        {
            FullNameHelper = PhoneHelper = string.Empty;

            if (string.IsNullOrWhiteSpace(FullName))
            {
                FullNameHelper = "Vui lòng nhập họ tên";
                return false;
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                PhoneHelper = "Vui lòng nhập số điện thoại";
                return false;
            }

            if (!Regex.IsMatch(PhoneNumber, @"^\d{9,11}$"))
            {
                PhoneHelper = "Số điện thoại không hợp lệ";
                return false;
            }

            if (Face == null)
            {
                if (Face == null)
                {
                    var vm = new ConfirmationViewModel("Vui lòng chụp khuôn mặt", "Thiếu dữ liệu", false);
                    new Views.ConfirmationWindow { DataContext = vm }.ShowDialog();
                    return false;
                }
            }

            return true;
        }

        private float[]? _currentEmbedding;

        private async void Save()
        {
            if (!Validate()) return;

            MessageQueue.Enqueue("Đang xử lý...");

            try
            {
                using (var db = new AppDbContext())
                {
                    if (db.Relatives.Any(r => r.PhoneNumber == PhoneNumber))
                    {
                        PhoneHelper = "Số điện thoại đã tồn tại";
                        MessageQueue.Enqueue("Số điện thoại đã tồn tại!");
                        return;
                    }
                    
                    var embeddingBytes = Services.FaceRecognitionService.Instance.ComputeembeddingToBytes(_currentEmbedding);

                    db.Relatives.Add(new Relatives
                    {
                        FullName = FullName,
                        PhoneNumber = PhoneNumber,
                        FaceEmbedding = embeddingBytes,
                        Note = Note,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });

                    db.SaveChanges();
                }

                

                MessageQueue.Enqueue("Tạo người gửi thành công!");
                await Task.Delay(1000); 
                
                foreach (Window win in Application.Current.Windows)
                {
                    if (win is Views.CustomerCameraWindow)
                        win.Close();
                    if (win.DataContext == this && win is Views.CreateRelative)
                        win.Close();
                }
            }
            catch (Exception ex)
            {
                MessageQueue.Enqueue($"Lỗi: {ex.Message}");
                var vm = new ConfirmationViewModel(ex.Message, "Lỗi", false);
                new Views.ConfirmationWindow { DataContext = vm }.ShowDialog();
            }
        }

        private async void CaptureFace()
        {
            if (CameraVM.CameraFrame is BitmapSource bitmapSource)
            {
                Face = GetJpgFromImageControl(bitmapSource);
                
                await Task.Run(() => 
                {
                   _currentEmbedding = Services.FaceRecognitionService.Instance.GetEmbedding(bitmapSource);
                });

                if (_currentEmbedding != null)
                     MessageQueue.Enqueue("Đã chụp ảnh");
                else
                     MessageQueue.Enqueue("Không tìm thấy khuôn mặt!");
            }
        }

        private byte[] GetJpgFromImageControl(BitmapSource imageC)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageC));
            using (System.IO.MemoryStream ms = new())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
        }
    }
}
