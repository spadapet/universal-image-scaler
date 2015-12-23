using System.Windows;
using System.Windows.Media.Imaging;

namespace UniversalImageScaler.Image
{
    internal class RasterFrame : IFrame
    {
        private BitmapSource source;

        public RasterFrame(BitmapSource source)
        {
            this.source = source;
        }

        public Size? PixelSize
        {
            get { return new Size(this.source.PixelWidth, this.source.PixelHeight); }
        }

        public BitmapSource Thumbnail
        {
            get { return this.source; }
        }

        public BitmapSource Render(double pixelWidth, double pixelHeight, ImageTransformType transform)
        {
            return ImageHelpers.ScaleBitmap(this.source, pixelWidth, pixelHeight, transform);
        }
    }
}
