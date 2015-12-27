using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Svg;
using UniversalImageScaler.Utility;

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
                System.Drawing.SizeF size = this.svg.GetDimensions();
                return size.Height != 0.0 ? size.Width / size.Height : 0.0;
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
                            double width = this.WidthOverHeight >= 1.0 ? 256.0 : 256.0 * this.WidthOverHeight;
                            double height = this.WidthOverHeight >= 1.0 ? 256.0 / this.WidthOverHeight : 256.0;

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

            System.Drawing.SizeF size = this.svg.GetDimensions();
            double scale = Math.Min(pixelWidth / size.Width, pixelHeight / size.Height);
            double bitmapWidth = Math.Ceiling(size.Width * scale);
            double bitmapHeight = Math.Ceiling(size.Height * scale);

            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)bitmapWidth, (int)bitmapHeight))
            using (ISvgRenderer render = SvgRenderer.FromImage(bitmap))
            using (MemoryStream stream = new MemoryStream())
            {
                render.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                render.ScaleTransform((float)scale, (float)scale);
                this.svg.Draw(render);

                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return ImageHelpers.ScaleBitmap(bitmapImage, (int)pixelWidth, (int)pixelHeight, transform);
            }
        }
    }
}
