﻿using System.ComponentModel;
using System.Windows;

namespace UniversalImageScaler.Models
{
    internal class OutputImageScale : OutputImage
    {
        private double scale;

        public OutputImageScale(OutputSet owner, double scale)
            : base(owner)
        {
            this.scale = scale;
            owner.PropertyChanged += OnOwnerPropertyChanged;
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
            get { return this.Owner.GetScaledPath(this.scale); }
        }

        public override string DisplayText
        {
            get
            {
                string text = $"Scale {(int)(this.scale * 100.0)}, {this.PixelWidth}px x {this.PixelHeight}px";
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

                double width = this.scale * this.Owner.Width;
                double height = this.scale * this.Owner.Height;
                Size frameSize = this.Frame.PixelSize.Value;

                return frameSize.Width >= width || frameSize.Height >= height;
            }
        }

        private void OnOwnerPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            this.OnPropertyChanged(nameof(this.Path));
            this.OnPropertyChanged(nameof(this.PixelWidth));
            this.OnPropertyChanged(nameof(this.PixelHeight));
            this.OnPropertyChanged(nameof(this.DisplayText));
            this.OnPropertyChanged(nameof(this.TooLargeDisplayText));
        }
    }
}
