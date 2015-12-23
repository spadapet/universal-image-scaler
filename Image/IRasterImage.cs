using System.Windows.Media.Imaging;

namespace UniversalImageScaler.Image
{
    internal interface IRasterImage : IImage
    {
        BitmapSource Image { get; }
        double PixelWidth { get; }
        double PixelHeight { get; }
    }
}
