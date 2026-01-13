using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace tnt_wpf_children.Services
{
    public class CameraService
    {
        private static CameraService _instance;
        public static CameraService Instance => _instance ??= new CameraService();

        private VideoCapture _capture;
        private CancellationTokenSource _cts;
        private Task _cameraTask;

        public event Action<BitmapSource> FrameArrived;

        private CameraService()
        {
        }

        public List<string> GetCameraList()
        {
            // OpenCvSharp doesn't natively list devices with names easily on Windows without DirectShowLib.
            // We'll return a list of probable indices.
            var list = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                // Optimistically list indices. Ideally we'd probe them, but that's slow.
                list.Add($"Camera {i}");
            }
            return list;
        }

        public void StartCamera(string cameraName)
        {
            StopCamera();

            // Parse index from "Camera {i}"
            int index = 0;
            if (cameraName.StartsWith("Camera "))
            {
                int.TryParse(cameraName.Substring(7), out index);
            }

            _cts = new CancellationTokenSource();
            _cameraTask = Task.Run(() => CaptureLoop(index, _cts.Token));
        }

        private void CaptureLoop(int cameraIndex, CancellationToken token)
        {
            try
            {
                _capture = new VideoCapture(cameraIndex);
                if (!_capture.IsOpened())
                {
                    MessageBox.Show($"Cannot open camera {cameraIndex}");
                    return;
                }

                using (var frame = new Mat())
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (_capture.Read(frame) && !frame.Empty())
                        {
                            // Convert to BitmapSource
                            // Must freeze for UI thread access
                            var bitmapSource = frame.ToBitmapSource();
                            bitmapSource.Freeze();
                            FrameArrived?.Invoke(bitmapSource);
                        }
                        Thread.Sleep(30); // ~30 fps
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle
                System.Diagnostics.Debug.WriteLine($"Camera Error: {ex.Message}");
            }
            finally
            {
                _capture?.Release();
                _capture = null;
            }
        }

        public void StopCamera()
        {
            _cts?.Cancel();
            try
            {
                _cameraTask?.Wait(500);
            }
            catch { }
            _cts = null;
        }
    }
}
