using System.ComponentModel;

namespace UniversalImageScaler.Models
{
    internal class OutputImageScaleOptional8 : OutputImageScale
    {
        public OutputImageScaleOptional8(OutputSet owner, double scale)
            : base(owner, scale)
        {
        }

        protected override bool ShouldEnable
        {
            get { return base.ShouldEnable && this.Owner.Owner.ShowOptionalScales8; }
        }

        protected override bool IsOptional
        {
            get { return true; }
        }
    }
}
