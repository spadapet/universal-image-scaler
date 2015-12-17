using System;

namespace UniversalImageScaler.Models
{
    internal class OutputImageScale : OutputImage
    {
        private double scale;

        public OutputImageScale(OutputSet owner, double scale)
            : base(owner)
        {
            this.scale = scale;
        }

        public double Scale
        {
            get { return this.scale; }
        }

        public override double PixelWidth
        {
            get { return this.Owner.GetScaledWidth(this.scale); }
        }

        public override double PixelHeight
        {
            get { return this.Owner.GetScaledHeight(this.scale); }
        }

        public override string Path
        {
            get { return this.Owner.Owner.GetScaledPath(this.scale); }
        }

        public override string DisplayText
        {
            get
            {
                return "Foo";
            }
        }
    }
}
