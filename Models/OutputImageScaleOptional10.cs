using System.ComponentModel;

namespace UniversalImageScaler.Models
{
    internal class OutputImageScaleOptional10 : OutputImageScale
    {
        public OutputImageScaleOptional10(OutputSet owner, double scale)
            : base(owner, scale)
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
