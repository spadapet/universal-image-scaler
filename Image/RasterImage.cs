using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace UniversalImageScaler.Image
{
    internal class RasterImage : IImage
    {
        private BitmapSource source;
        private ImageFileType fileType;
        private List<IFrame> frames;

        public RasterImage(BitmapSource source, ImageFileType fileType)
        {
            this.source = source;
            this.fileType = fileType;
            this.frames = new List<IFrame>(1)
            {
                new RasterFrame(source)
            };
        }

        public ImageFileType FileType
        {
            get { return this.fileType; }
        }

        public IReadOnlyList<IFrame> Frames
        {
            get { return this.frames; }
        }
    }
}
