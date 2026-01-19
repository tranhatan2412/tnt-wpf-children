using System;
using System.Drawing;
using System.IO;
using System.Linq;
using FaceONNX;
using System.Windows.Media.Imaging;

namespace tnt_wpf_children.Services
{
    public class FaceRecognitionService
    {
        private static FaceRecognitionService _instance;
        public static FaceRecognitionService Instance => _instance ??= new FaceRecognitionService();

        private FaceDetector _faceDetector;
        private Face68LandmarksExtractor _faceLandmarksExtractor;
        private FaceEmbedder _faceEmbedder;

        private FaceRecognitionService()
        {
            _faceDetector = new FaceDetector();
            _faceLandmarksExtractor = new Face68LandmarksExtractor();
            _faceEmbedder = new FaceEmbedder();
        }

        public float[]? GetEmbedding(BitmapSource bitmapSource)
        {
            using var bitmap = BitmapSourceToBitmap(bitmapSource);
            
            if (bitmap.Width > 800 || bitmap.Height > 800)
            {
                 float scale = Math.Min(800f / bitmap.Width, 800f / bitmap.Height);
                 int newWidth = (int)(bitmap.Width * scale);
                 int newHeight = (int)(bitmap.Height * scale);
                 
                 using var resized = new Bitmap(bitmap, newWidth, newHeight);
                 return GetEmbedding(resized);
            }

            return GetEmbedding(bitmap);
        }

        private float[]? GetEmbedding(Bitmap bitmap)
        {
            try
            {
                var originalImage = bitmap;
                System.Diagnostics.Debug.WriteLine($"Processing Face: {originalImage.Width}x{originalImage.Height}");
                
                var faces = _faceDetector.Forward(originalImage);
                if (faces == null || faces.Length == 0) 
                {
                    System.Diagnostics.Debug.WriteLine("No faces detected");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"Faces detected: {faces.Length}");
                var face = faces.OrderByDescending(f => f.Box.Width * f.Box.Height).First();

                var landmarks = _faceLandmarksExtractor.Forward(originalImage, face.Box);

                var cropRect = face.Box;
                cropRect.Intersect(new Rectangle(0, 0, originalImage.Width, originalImage.Height));
                
                using var croppedFace = originalImage.Clone(cropRect, originalImage.PixelFormat);
                var embedding = _faceEmbedder.Forward(croppedFace);

                return embedding;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Face Recognition Error: {ex.Message}");
                return null;
            }
        }



        public byte[]? ComputeembeddingToBytes(float[]? embedding)
        {
            if (embedding == null) return null;
            var byteArray = new byte[embedding.Length * 4];
            Buffer.BlockCopy(embedding, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        public float[]? BytesToEmbedding(byte[]? bytes)
        {
            if (bytes == null) return null;
            var floatArray = new float[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, floatArray, 0, bytes.Length);
            return floatArray;
        }

        private Bitmap BitmapSourceToBitmap(BitmapSource source)
        {
            using var outStream = new MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(source));
            enc.Save(outStream);
            return new Bitmap(outStream);
        }

        public float CompareFaces(float[] embedding1, float[] embedding2)
        {
            if (embedding1 == null || embedding2 == null) return 0;
            if (embedding1.Length != embedding2.Length) return 0;

            float dotProduct = 0;
            float norm1 = 0;
            float norm2 = 0;

            for (int i = 0; i < embedding1.Length; i++)
            {
                dotProduct += embedding1[i] * embedding2[i];
                norm1 += embedding1[i] * embedding1[i];
                norm2 += embedding2[i] * embedding2[i];
            }

            if (norm1 == 0 || norm2 == 0) return 0;

            return dotProduct / (float)(Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }

        public const float MatchThreshold = 0.65f;
        public bool IsFaceMatch(float[] embedding1, float[] embedding2) =>
            CompareFaces(embedding1, embedding2) >= MatchThreshold;
    }
}
