using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

        public enum IconState
        {
            None,
            Processing,
            Success
        }

        private IconState _icon = IconState.None;
        public IconState Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBusy));
                OnPropertyChanged(nameof(IsProcessing));
                OnPropertyChanged(nameof(IsSuccess));
            }
        }

        public bool IsBusy => Icon != IconState.None;
        public bool IsProcessing => Icon == IconState.Processing;
        public bool IsSuccess => Icon == IconState.Success;

        private string _tbStatus = "Đang xử lý...";
        public string TBStatus
        {
            get => _tbStatus;
            set { _tbStatus = value; OnPropertyChanged(); }
        }

        public ICommand SaveCmd { get; }
        public ICommand CaptureFaceCmd { get; }

        public CreateRelativeViewModel()
        {
            SaveCmd = new RelayCommand<object>(p => true, p => Save());
            CaptureFaceCmd = new RelayCommand<object>(p => true, p => CaptureFace());
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
                MessageBox.Show("Vui lòng chụp khuôn mặt", "Thiếu dữ liệu",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private async void Save()
        {
            if (!Validate()) return;

            Icon = IconState.Processing;

            try
            {
                using (var db = new AppDbContext())
                {
                    if (db.Relatives.Any(r => r.PhoneNumber == PhoneNumber))
                    {
                        Icon = IconState.None;
                        PhoneHelper = "Số điện thoại đã tồn tại";
                        return;
                    }

                    db.Relatives.Add(new Relatives
                    {
                        FullName = FullName,
                        PhoneNumber = PhoneNumber,
                        Face = Face,
                        Note = Note,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });

                    db.SaveChanges();
                }

                await Task.Delay(1200);
                Icon = IconState.Success;
                TBStatus = "Tạo người gửi thành công";
            }
            catch (Exception ex)
            {
                Icon = IconState.None;
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CaptureFace()
        {
            // TODO: gọi CameraService → lấy byte[] Face
            // Face = camera.Capture();
        }
    }
}
