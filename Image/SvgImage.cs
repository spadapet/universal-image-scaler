using System;
using System.Collections.Generic;
using Svg;

namespace UniversalImageScaler.Image
{
    internal class SvgImage : IImage
    {
        private SvgDocument svg;
        private List<IFrame> frames;

        public SvgImage(SvgDocument svg)
        {
            this.svg = svg;
            this.frames = new List<IFrame>(1)
            {
                new SvgFrame(svg)
            };
        }

        public ImageFileType FileType
        {
            get { return ImageFileType.Svg; }
        }

        public IReadOnlyList<IFrame> Frames
        {
            get { return this.frames; }
        }
    }
}
