using System.Windows;
using System.Windows.Media.Imaging;

namespace UniversalImageScaler.Image
{
    public interface IFrame
    {
        Size? PixelSize { get; }
        double WidthOverHeight { get; }
        BitmapSource Thumbnail { get; }
        BitmapSource Render(double pixelWidth, double pixelHeight, ImageTransformType transform);
    }
}
