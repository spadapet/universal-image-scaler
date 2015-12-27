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
            get
            {
                if (this.svg.ViewBox.Width == 0 || this.svg.ViewBox.Height == 0)
                {
                    if (this.svg.Height == 0)
                    {
                        return 1.0;
                    }
                    else
                    {
                        return this.svg.Width / this.svg.Height;
                    }
                }
                else
                {
                    return this.svg.ViewBox.Width / this.svg.ViewBox.Height;
                }
            }
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
                            double width = this.WidthOverHeight >= 1.0 ? 256.0 : 256.0 / this.WidthOverHeight;
                            double height = this.WidthOverHeight >= 1.0 ? 256.0 * this.WidthOverHeight : 256.0;

                            this.thumbnail = this.Render(width, height, ImageTransformType.None);
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
