using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UniversalImageScaler.Utility
{
    public enum ImageTransformType
    {
        None,
        WhiteOnly,
    }

    public enum ImageFileType
    {
        None,
        Png,
        Jpeg,
        Pdf,
        Svg,
    }

    internal static class ImageHelpers
    {
        public static ImageFileType GetFileType(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    return ImageFileType.Png;
                }

                if (name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                {
                    return ImageFileType.Jpeg;
                }

                if (name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return ImageFileType.Pdf;
                }

                if (name.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                {
                    return ImageFileType.Svg;
                }
            }

            return ImageFileType.None;
        }

        public static bool IsSourceImageType(ImageFileType type)
        {
            switch (type)
            {
                case ImageFileType.Png:
                case ImageFileType.Jpeg:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsSourceImageFile(string path)
        {
            bool isImage = false;

            if (!string.IsNullOrEmpty(path))
            {
                string name = Path.GetFileName(path);
                ImageFileType type = ImageHelpers.GetFileType(name);
                isImage = ImageHelpers.IsSourceImageType(type);
            }

            return isImage;
        }

        public static BitmapImage LoadSourceImage(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

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

            // Center the scaled image
            if (sourceWidth != width || sourceHeight != height)
            {
                int pixelWidth = (int)width;
                int pixelHeight = (int)height;
                WriteableBitmap newSource = new WriteableBitmap(pixelWidth, pixelHeight, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
                byte[] bytes = new byte[pixelWidth * pixelHeight * 4];

                int destX = (int)(width / 2 - sourceWidth / 2);
                int destY = (int)(height / 2 - sourceHeight / 2);
                source.CopyPixels(bytes, pixelWidth * 4, destY * pixelWidth * 4 + destX * 4);
                newSource.WritePixels(new Int32Rect(0, 0, pixelWidth, pixelHeight), bytes, pixelWidth * 4, 0);

                source = newSource;
                sourceWidth = source.PixelWidth;
                sourceHeight = source.PixelHeight;
            }

            return source;
        }

        public static void Save(BitmapSource source, ImageFileType type, string path)
        {
            switch (type)
            {
                case ImageFileType.Png:
                    ImageHelpers.Save<PngBitmapEncoder>(source, path);
                    break;

                case ImageFileType.Jpeg:
                    ImageHelpers.Save<JpegBitmapEncoder>(source, path);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
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
