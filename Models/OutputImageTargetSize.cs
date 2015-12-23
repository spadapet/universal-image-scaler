using System.Windows;

namespace UniversalImageScaler.Models
{
    internal class OutputImageTargetSize : OutputImage
    {
        private double targetSize;
        private bool unplated;

        public OutputImageTargetSize(OutputSet owner, double targetSize, bool unplated)
            : base(owner)
        {
            this.targetSize = targetSize;
            this.unplated = unplated;
        }

        public double TargetSize
        {
            get { return this.targetSize; }
        }

        public bool Unplated
        {
            get { return this.unplated; }
        }

        public override double PixelWidth
        {
            get { return this.targetSize; }
        }

        public override double PixelHeight
        {
            get { return this.targetSize; }
        }

        public override string Path
        {
            get { return this.Owner.GetTargetSizePath(this.targetSize, this.unplated); }
        }

        public override string DisplayText
        {
            get
            {
                string text = $"Target size {this.PixelWidth}px x {this.PixelHeight}px";

                if (this.unplated)
                {
                    text += ", unplated";
                }

                if (this.IsOptional)
                {
                    text += " (optional)";
                }

                return text;
            }
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
                return frameSize.Width >= this.targetSize || frameSize.Height >= this.targetSize;
            }
        }
    }
}
