using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UniversalImageScaler.Utility
{
    internal enum ImageTransformType
    {
        None,
        WhiteOnly,
    }

    internal static class ImageHelpers
    {
        public static bool IsSourceImageName(string name)
        {
            bool isImage = false;

            if (!string.IsNullOrEmpty(name))
            {
                isImage = name.EndsWith(".png");
            }

            return isImage;
        }

        public static bool IsSourceImageFile(string path)
        {
            bool isImage = false;

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                isImage = ImageHelpers.IsSourceImageName(Path.GetFileName(path));
            }

            return isImage;
        }

        public static BitmapSource LoadSourceImage(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return bitmap;
        }

        public static BitmapSource ScaleSourceImage(BitmapSource source, double width, double height)
        {
            if (source.Format != PixelFormats.Bgra32)
            {
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            }

            double sourceWidth = source.PixelWidth;
            double sourceHeight = source.PixelHeight;
            double scale = Math.Min(width / sourceWidth, height / sourceHeight);

            if (scale != 1.0)
            {
                source = new TransformedBitmap(source, new ScaleTransform(scale, scale));
                sourceWidth = source.PixelWidth;
                sourceHeight = source.PixelHeight;
            }

            if (sourceWidth != width || sourceHeight != height)
            {
                WriteableBitmap newSource = new WriteableBitmap((int)width, (int)height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
                // ...
            }

            return source;
        }

        public static void SavePng(BitmapSource source, string path)
        {
            ImageHelpers.Save<PngBitmapEncoder>(source, path);
        }

        private static void Save<EncoderT>(BitmapSource source, string path) where EncoderT : BitmapEncoder, new()
        {
            BitmapEncoder encoder = new EncoderT();
            encoder.Frames.Add(BitmapFrame.Create(source));

            using (MemoryStream streamOut = new MemoryStream())
            {
                encoder.Save(streamOut);
                byte[] bytes = streamOut.ToArray();
                File.WriteAllBytes(path, bytes);
            }
        }

        public static BitmapSource TransformImage(BitmapSource source, ImageTransformType type)
        {
            switch (type)
            {
                case ImageTransformType.None:
                    break;

                case ImageTransformType.WhiteOnly:
                    source = TransformImageWhiteOnly(source);
                    break;
            }

            return source;
        }

        private static BitmapSource TransformImageWhiteOnly(BitmapSource source)
        {
            FormatConvertedBitmap bgraBitmap = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            WriteableBitmap writeableBitmap = new WriteableBitmap(bgraBitmap);
            byte[] pixels = new byte[writeableBitmap.PixelWidth * writeableBitmap.PixelHeight * 4];
            writeableBitmap.CopyPixels(pixels, writeableBitmap.PixelWidth * 4, 0);

            for (int y = 0; y < writeableBitmap.PixelHeight; y++)
            {
                for (int x = 0; x < writeableBitmap.PixelWidth; x++)
                {
                    int start = y * writeableBitmap.PixelWidth * 4 + x * 4;
                    byte val = (pixels[start + 3] > 127) ? (byte)255 : (byte)0;
                    pixels[start + 0] = val;
                    pixels[start + 1] = val;
                    pixels[start + 2] = val;
                    pixels[start + 3] = val;
                }
            }

            Int32Rect sourceRect = new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight);
            writeableBitmap.WritePixels(sourceRect, pixels, writeableBitmap.PixelWidth * 4, 0);
            return writeableBitmap;
        }
    }
}
