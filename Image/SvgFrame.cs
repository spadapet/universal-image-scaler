using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Svg;

namespace UniversalImageScaler.Image
{
    internal class SvgFrame : IFrame
    {
        private SvgDocument svg;
        private BitmapSource thumbnail;

        public SvgFrame(SvgDocument svg)
        {
            this.svg = svg;
        }

        public Size? PixelSize
        {
            get { return null; }
        }

        public double WidthOverHeight
        {
            get { return (double)this.svg.ViewBox.Width / (double)this.svg.ViewBox.Height; }
        }

        public BitmapSource Thumbnail
        {
            get
            {
                if (this.thumbnail == null)
                {
                    lock (this)
                    {
                        if (this.thumbnail == null)
                        {
                            this.thumbnail = this.Render(256, 256, ImageTransformType.None);
                        }
                    }
                }

                return this.thumbnail;
            }
        }

        public BitmapSource Render(double pixelWidth, double pixelHeight, ImageTransformType transform)
        {
            pixelWidth = Math.Ceiling(pixelWidth);
            pixelHeight = Math.Ceiling(pixelHeight);

            System.Drawing.Bitmap bitmap = this.svg.Draw((int)pixelWidth, (int)pixelHeight);
            if (bitmap != null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    return bitmapImage;
                }
            }

            return null;
        }
    }
}
