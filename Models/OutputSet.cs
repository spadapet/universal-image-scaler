using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UniversalImageScaler.Image;

namespace UniversalImageScaler.Models
{
    public class OutputSet : ModelBase
    {
        private SourceImage owner;
        private string name;
        private string description;
        private string fileNameOverride;
        private double width;
        private double height;
        private bool enabled;
        private bool expanded;
        private bool fixedSize;
        private bool? generate;
        private ImageTransformType transformType;
        private ImageFileType outputType;
        private ObservableCollection<OutputImage> images;

        public OutputSet(SourceImage owner, string name, double width, double height)
        {
            this.owner = owner;
            this.name = name;
            this.description = string.Empty;
            this.width = width;
            this.height = height;
            this.expanded = true;
            this.fixedSize = true;
            this.transformType = ImageTransformType.None;
            this.outputType = owner.Image.FileType;
            this.images = new ObservableCollection<OutputImage>();

            owner.PropertyChanged += this.OnOwnerPropertyChanged;
        }

        public OutputSet(SourceImage owner, string name)
        {
            this.owner = owner;
            this.name = name;
            this.description = string.Empty;
            this.expanded = true;
            this.transformType = ImageTransformType.None;
            this.outputType = owner.Image.FileType;
            this.images = new ObservableCollection<OutputImage>();

            this.UpdateSize();

            owner.PropertyChanged += this.OnOwnerPropertyChanged;
        }

        public void Initialize()
        {
            foreach (OutputImage image in this.images)
            {
                image.Initialize();
            }

            this.Generate = true;
            this.Enabled = true;

            // Must be checked after Generate is set to true
            if (!this.ImagesToGenerate.Any())
            {
                this.Generate = false;
                this.Enabled = false;
            }
        }

        public SourceImage Owner
        {
            get { return this.owner; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public string FileNameOverride
        {
            get { return this.fileNameOverride ?? string.Empty; }
            set
            {
                if (this.fileNameOverride != value)
                {
                    this.fileNameOverride = value;
                    this.OnPropertyChanged(nameof(this.FileNameOverride));
                }
            }
        }

        public string RawTooltip
        {
            get { return this.description ?? string.Empty; }
        }

        public string Tooltip
        {
            get
            {
                string text = this.RawTooltip;
                int count = 0;
                const int maxShow = 5;

                foreach (OutputImage image in this.ImagesToGenerate)
                {
                    if (++count <= maxShow)
                    {
                        if (!string.IsNullOrEmpty(text))
                        {
                            text += "\r\n";
                        }

                        text += $"> {Path.GetFileName(image.Path)}";
                    }
                }

                if (count > maxShow)
                {
                    text += $"\r\n(plus {count - 6} more)";
                }

                return !string.IsNullOrEmpty(text) ? text : null;
            }

            set
            {
                if (this.description != value)
                {
                    this.description = value;
                    this.OnPropertyChanged(nameof(this.Tooltip));
                    this.OnPropertyChanged(nameof(this.RawTooltip));
                }
            }
        }

        public double Width
        {
            get { return this.width; }
            set
            {
                if (!this.fixedSize && this.width != value)
                {
                    this.width = value;
                    this.OnPropertyChanged(nameof(this.Width));
                }
            }
        }

        public double Height
        {
            get { return this.height; }
            set
            {
                if (!this.fixedSize && this.height != value)
                {
                    this.height = value;
                    this.OnPropertyChanged(nameof(this.Height));
                }
            }
        }

        public bool Enabled
        {
            get { return this.enabled; }
            set
            {
                if (this.enabled != value)
                {
                    this.enabled = value;
                    this.OnPropertyChanged(nameof(this.Enabled));
                }
            }
        }

        public bool ShowExpander
        {
            get { return false; }
        }

        public bool Expanded
        {
            get { return this.expanded; }
            set
            {
                if (this.expanded != value)
                {
                    this.expanded = value;
                    this.OnPropertyChanged(nameof(this.Expanded));
                }
            }
        }

        public bool FixedSize
        {
            get { return this.fixedSize; }
            set
            {
                if (this.fixedSize != value)
                {
                    this.fixedSize = value;
                    this.OnPropertyChanged(nameof(this.FixedSize));
                }
            }
        }

        public ImageTransformType TransformType
        {
            get { return this.transformType; }
            set
            {
                if (this.transformType != value)
                {
                    this.transformType = value;
                    this.OnPropertyChanged(nameof(this.TransformType));
                }
            }
        }

        public ImageFileType OutputFileType
        {
            get { return this.outputType; }
            set
            {
                if (this.outputType != value)
                {
                    this.outputType = value;
                    this.OnPropertyChanged(nameof(this.OutputFileType));
                }
            }
        }

        public bool? Generate
        {
            get { return this.generate; }
            set { this.SetGenerate(value, true); }
        }

        public string UnscaledPath
        {
            get { return this.GetScaledPath(0); }
        }

        public string GetScaledPath(double scale)
        {
            return this.Owner.GetScaledPath(scale, this.FileNameOverride);
        }

        public string GetTargetSizePath(double targetSize, bool unplated)
        {
            return this.Owner.GetTargetSizePath(targetSize, unplated, this.FileNameOverride);
        }

        private void SetGenerate(bool? value, bool updateImages)
        {
            if (this.generate != value)
            {
                this.generate = value;
                this.OnPropertyChanged(nameof(this.Generate));

                if (updateImages)
                {
                    this.UpdateImageGenerate();
                }
            }
        }

        public IEnumerable<OutputImage> Images
        {
            get { return this.images; }
        }

        public IEnumerable<OutputImage> ImagesToGenerate
        {
            get
            {
                foreach (OutputImage image in this.Images)
                {
                    if (image.Generate && image.Enabled)
                    {
                        yield return image;
                    }
                }
            }
        }

        public void AddImage(OutputImage image)
        {
            if (image != null && !this.images.Contains(image))
            {
                this.images.Add(image);

                image.PropertyChanged += OnImagePropertyChanged;
            }
        }

        public double GetScaledWidth(double scale)
        {
            return Math.Ceiling(this.width * scale);
        }

        public double GetScaledHeight(double scale)
        {
            return Math.Ceiling(this.height * scale);
        }

        private void UpdateGenerate()
        {
            int trueCount = 0;
            int falseCount = 0;

            foreach (OutputImage image in this.Images)
            {
                if (image.Enabled)
                {
                    if (image.Generate)
                    {
                        trueCount++;
                    }
                    else
                    {
                        falseCount++;
                    }
                }
            }

            bool enabled = trueCount > 0 || falseCount > 0;
            bool? generate = null;

            if (trueCount == 0 || falseCount == 0)
            {
                generate = (trueCount > 0);
            }

            this.Enabled = enabled;
            this.SetGenerate(generate, false);
            this.OnPropertyChanged(nameof(this.Tooltip));
            this.OnPropertyChanged(nameof(this.RawTooltip));
        }

        private void UpdateImageGenerate()
        {
            if (this.Generate.HasValue)
            {
                foreach (OutputImage image in this.Images)
                {
                    image.PropertyChanged -= this.OnImagePropertyChanged;
                    image.Generate = this.Generate.Value;
                    image.PropertyChanged += this.OnImagePropertyChanged;
                }
            }
        }

        private void OnImagePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(args.PropertyName) ||
                args.PropertyName == nameof(OutputImage.Generate) ||
                args.PropertyName == nameof(OutputImage.Enabled))
            {
                this.UpdateGenerate();
            }
        }

        private void OnOwnerPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (!this.FixedSize)
            {
                this.UpdateSize();
            }

            this.UpdateGenerate();
        }

        private void UpdateSize()
        {
            if (this.Owner.HasCustomSize)
            {
                this.Width = this.Owner.CustomPixelWidth;
                this.Height = this.Owner.CustomPixelHeight;
            }
            else if (this.Owner.Frame.PixelSize.HasValue)
            {
                this.Width = this.Owner.Frame.PixelSize.Value.Width / this.Owner.Scale;
                this.Height = this.Owner.Frame.PixelSize.Value.Height / this.Owner.Scale;
            }
            else
            {
                Debug.Fail("Frame has no custom size and no actual size");
            }
        }
    }
}
