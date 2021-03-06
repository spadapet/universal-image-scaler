﻿using System.ComponentModel;
using UniversalImageScaler.Image;

namespace UniversalImageScaler.Models
{
    public abstract class OutputImage : ModelBase
    {
        private OutputSet owner;
        private bool generate;
        private bool enabled;
        private bool markedForDeletion;
        private long oldFileSize;
        private long newFileSize;

        public OutputImage(OutputSet owner)
        {
            this.owner = owner;
            this.owner.PropertyChanged += OnImagePropertyChanged;
            this.owner.Owner.PropertyChanged += OnImagePropertyChanged;
        }

        public abstract double PixelWidth { get; }
        public abstract double PixelHeight { get; }
        public abstract string Path { get; }
        public abstract string DisplayText { get; }

        public virtual string TooLargeDisplayText
        {
            get
            {
                string name = System.IO.Path.GetFileName(this.Path);
                return $"{name} ({this.NewFileSize} bytes, for {this.Owner.Name})";
            }
        }

        public virtual void Initialize()
        {
            this.UpdateEnabled();
        }

        public virtual string Tooltip
        {
            get
            {
                string text = System.IO.Path.GetFileName(this.Path);
                string ownerTip = this.Owner.RawTooltip;

                return !string.IsNullOrEmpty(ownerTip)
                    ? $"{ownerTip}\r\n> {text}"
                    : text;
            }
        }

        public bool Generate
        {
            get { return this.generate; }
            set
            {
                if (this.generate != value)
                {
                    this.generate = value;
                    this.OnPropertyChanged(nameof(this.Generate));
                }
            }
        }

        public bool FileSizeTooLarge
        {
            get
            {
                OutputFeature feature = this.SourceImage.Feature;

                return feature != null &&
                    feature.HasMaxFileSize &&
                    this.oldFileSize <= feature.MaxFileSize &&
                    this.NewFileSize > feature.MaxFileSize;
            }
        }

        public long OldFileSize
        {
            get { return this.oldFileSize; }
            set
            {
                if (this.oldFileSize != value)
                {
                    this.oldFileSize = value;
                    this.OnPropertyChanged(nameof(this.OldFileSize));
                    this.OnPropertyChanged(nameof(this.FileSizeTooLarge));
                }
            }
        }

        public long NewFileSize
        {
            get { return this.newFileSize; }
            set
            {
                if (this.newFileSize != value)
                {
                    this.newFileSize = value;
                    this.OnPropertyChanged(nameof(this.NewFileSize));
                    this.OnPropertyChanged(nameof(this.FileSizeTooLarge));
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

        public bool MarkedForDeletion
        {
            get { return this.markedForDeletion; }
            set
            {
                if (this.markedForDeletion != value)
                {
                    this.markedForDeletion = value;
                    this.OnPropertyChanged(nameof(this.MarkedForDeletion));
                }
            }
        }

        public ImageFileType OutputFileType
        {
            get { return this.Owner.OutputFileType; }
        }

        public ImageTransformType TransformType
        {
            get { return this.Owner.TransformType; }
        }

        public OutputSet Owner
        {
            get { return this.owner; }
        }

        protected SourceImage SourceImage
        {
            get { return this.Owner.Owner; }
        }

        protected IImage Image
        {
            get { return this.SourceImage.Image; }
        }

        protected IFrame Frame
        {
            get { return this.SourceImage.Frame; }
        }

        protected void UpdateEnabled()
        {
            this.Enabled = this.ShouldEnable;
        }

        protected virtual bool ShouldEnable
        {
            get { return true; }
        }

        protected virtual bool IsOptional
        {
            get { return false; }
        }

        private void OnImagePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            this.UpdateEnabled();
            this.OnPropertyChanged(nameof(this.Tooltip));
        }
    }
}
