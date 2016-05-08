using System.Windows;

namespace UniversalImageScaler.Models
{
    internal class OutputImagePromoSize : OutputImage
    {
        private double width;
        private double height;

        public OutputImagePromoSize(OutputSet owner, double width, double height)
            : base(owner)
        {
            this.width = width;
            this.height = height;
        }

        public override double PixelWidth
        {
            get { return this.width; }
        }

        public override double PixelHeight
        {
            get { return this.height; }
        }

        public override string Path
        {
            get { return this.Owner.GetCustomOutputPath($"Promo{this.PixelWidth}x{this.PixelHeight}.png"); }
        }

        public override string DisplayText
        {
            get { return $"Store promo art {this.PixelWidth}x{this.PixelHeight}"; }
        }

        protected override bool ShouldEnable
        {
            get
            {
                if (!base.ShouldEnable)
                {
                    return false;
                }

                if (!this.Frame.PixelSize.HasValue)
                {
                    return true;
                }

                Size frameSize = this.Frame.PixelSize.Value;
                return frameSize.Width >= this.PixelWidth || frameSize.Height >= this.PixelHeight;
            }
        }
    }
}
