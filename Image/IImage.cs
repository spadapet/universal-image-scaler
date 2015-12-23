using System.Collections.Generic;

namespace UniversalImageScaler.Image
{
    public interface IImage
    {
        ImageFileType FileType { get; }
        IReadOnlyList<IFrame> Frames { get; }
    }
}
