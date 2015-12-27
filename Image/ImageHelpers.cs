using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Svg;

namespace UniversalImageScaler.Image
{
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

                if (name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                {
                    return ImageFileType.Bmp;
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

        public static bool IsBitmapType(ImageFileType type)
        {
            switch (type)
            {
                case ImageFileType.Png:
                case ImageFileType.Jpeg:
                case ImageFileType.Bmp:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsVectorType(ImageFileType type)
        {
            switch (type)
            {
                case ImageFileType.Svg:
                case ImageFileType.Pdf:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsSourceImageType(ImageFileType type)
        {
            return ImageHelpers.IsBitmapType(type) ||
                type == ImageFileType.Svg;
        }

        public static bool IsBitmapFile(string path)
        {
            return ImageHelpers.IsBitmapType(ImageHelpers.GetFileType(path));
        }

        public static bool IsSourceImageFile(string path)
        {
            return ImageHelpers.IsSourceImageType(ImageHelpers.GetFileType(path));
        }

        public static BitmapImage LoadBitmap(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        public static IImage LoadSourceImage(string path)
        {
            ImageFileType fileType = ImageHelpers.GetFileType(path);
            if (ImageHelpers.IsBitmapType(fileType))
            {
                BitmapSource source = ImageHelpers.LoadBitmap(path);
                if (source != null)
                {
                    return new RasterImage(source, fileType);
                }
            }
            else if (fileType == ImageFileType.Svg)
            {
                SvgDocument svg = SvgDocument.Open(path);
                return new SvgImage(svg);
            }

            throw new Exception("Couldn't load image file: " + path);
        }

        public static BitmapSource ScaleBitmap(BitmapSource source, double width, double height, ImageTransformType transform)
        {
            if (source.Format != PixelFormats.Bgra32)
            {
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            }

            width = Math.Ceiling(width);
            height = Math.Ceiling(height);

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

            return ImageHelpers.TransformImage(source, transform);
        }

        public static void SaveBitmap(BitmapSource source, ImageFileType type, string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    FileAttributes attribs = File.GetAttributes(path);
                    if ((attribs & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        attribs &= ~FileAttributes.ReadOnly;
                        File.SetAttributes(path, attribs);
                    }
                }
            }
            catch
            {
                Debug.Fail("Can't update file attributes: " + path);
            }

            switch (type)
            {
                case ImageFileType.Png:
                    ImageHelpers.SaveBitmap<PngBitmapEncoder>(source, path);
                    break;

                case ImageFileType.Jpeg:
                    ImageHelpers.SaveBitmap<JpegBitmapEncoder>(source, path);
                    break;

                case ImageFileType.Bmp:
                    ImageHelpers.SaveBitmap<BmpBitmapEncoder>(source, path);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private static void SaveBitmap<EncoderT>(BitmapSource source, string path) where EncoderT : BitmapEncoder, new()
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

        private static BitmapSource TransformImage(BitmapSource source, ImageTransformType type)
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
