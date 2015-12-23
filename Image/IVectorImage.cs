using System.Collections.Generic;

namespace UniversalImageScaler.Image
{
    internal interface IVectorImage : IImage
    {
        ICollection<IVectorImagePage> Pages { get; }
    }
}
