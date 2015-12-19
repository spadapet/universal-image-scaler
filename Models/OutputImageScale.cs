﻿using System;

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
                string text = $"Scale {(int)(this.scale * 100.0)}, {this.PixelWidth}x{this.PixelHeight}px";
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
                double width = this.scale * this.Owner.Width;
                double height = this.scale * this.Owner.Height;

                return base.ShouldEnable &&
                    this.Image.PixelWidth >= width &&
                    this.Image.PixelHeight >= height;
            }
        }

        protected virtual bool IsOptional
        {
            get { return false; }
        }
    }
}
