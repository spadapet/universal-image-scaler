using System.Windows.Media.Imaging;

namespace UniversalImageScaler.Image
{
    public interface IVectorImagePage
    {
        double Width { get; }
        double Height { get; }
        BitmapSource Render(double width, double height);
    }
}
