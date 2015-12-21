namespace UniversalImageScaler.Models
{
    internal class OutputImageTargetSizeOptional10 : OutputImageTargetSize
    {
        public OutputImageTargetSizeOptional10(OutputSet owner, double targetSize, bool unplated)
            : base(owner, targetSize, unplated)
        {
        }

        protected override bool ShouldEnable
        {
            get { return base.ShouldEnable && this.Owner.Owner.ShowOptionalScales10; }
        }

        protected override bool IsOptional
        {
            get { return true; }
        }
    }
}
