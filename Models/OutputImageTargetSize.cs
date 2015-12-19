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
            get { return this.Owner.Owner.GetTargetSizePath(this.targetSize, this.unplated); }
        }

        public override string DisplayText
        {
            get
            {
                string text = $"Target size {this.PixelWidth}x{this.PixelHeight}px";
                if (this.unplated)
                {
                    text += ", unplated";
                }

                return text;
            }
        }

        protected override bool ShouldEnable
        {
            get
            {
                return base.ShouldEnable &&
                    this.Image.PixelWidth >= this.targetSize &&
                    this.Image.PixelHeight >= targetSize;
            }
        }
    }
}
