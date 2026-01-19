using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using tnt_wpf_children.Services;

namespace tnt_wpf_children.ViewModels
{
    public class CameraViewModel : BaseViewModel
    {
        private ImageSource _cameraFrame;
        public ImageSource CameraFrame
        {
            get => _cameraFrame;
            set { _cameraFrame = value; OnPropertyChanged(); }
        }

        private string _cameraStatus = "Chưa chọn camera";
        public string CameraStatus
        {
            get => _cameraStatus;
            set { _cameraStatus = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> CameraList { get; set; }

        private string _selectedCamera;
        public string SelectedCamera
        {
            get => _selectedCamera;
            set
            {
                _selectedCamera = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(_selectedCamera))
                {
                    StartCamera(_selectedCamera);
                }
            }
        }


        public CameraViewModel()
        {
            CameraList = new ObservableCollection<string>();
            LoadCameras();

            CameraService.Instance.FrameArrived += OnFrameArrived;
        }

        private void OnFrameArrived(System.Windows.Media.Imaging.BitmapSource frame)
        {
             var app = Application.Current;
             if (app != null)
             {
                 try
                 {
                     app.Dispatcher.Invoke(() =>
                     {
                         CameraFrame = frame;
                         CameraStatus = "Đang quét khuôn mặt";
                     });
                 }
                 catch (TaskCanceledException) { }
                 catch (Exception) { }
             }
        }

        public void Cleanup()
        {
            CameraService.Instance.StopCamera();
            CameraService.Instance.FrameArrived -= OnFrameArrived;
        }

        private void LoadCameras()
        {
            var cameras = CameraService.Instance.GetCameraList();
            CameraList.Clear();
            foreach (var cam in cameras)
                CameraList.Add(cam);

            if (CameraList.Count > 0)
                SelectedCamera = CameraList[0];
        }

        private void StartCamera(string cameraName) => CameraService.Instance.StartCamera(cameraName);
        public event Action<float[]>? OnFaceRecognized;

        public void CaptureAndRecognize()
        {
            if (CameraFrame == null) return;
            
            try
            {
                var bitmapSource = CameraFrame as System.Windows.Media.Imaging.BitmapSource;
                if (bitmapSource == null) return;
                
                var embedding = FaceRecognitionService.Instance.GetEmbedding(bitmapSource);
                if (embedding != null)
                {
                    OnFaceRecognized?.Invoke(embedding);
                    CameraStatus = "Đã nhận diện khuôn mặt!";
                }
                else
                    CameraStatus = "Không tìm thấy khuôn mặt";
            }
            catch (Exception ex)
            {
                CameraStatus = $"Lỗi: {ex.Message}";
            }
            }
        }
    }

