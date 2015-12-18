using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace UniversalImageScaler.Models
{
    public class OutputSet : ModelBase
    {
        private SourceImage owner;
        private string name;
        private double width;
        private double height;
        private bool fixedSize;
        private bool? generate;
        private ObservableCollection<OutputImage> images;

        public OutputSet(SourceImage owner, string name, double width, double height, bool fixedSize)
        {
            this.owner = owner;
            this.name = name;
            this.width = width;
            this.height = height;
            this.fixedSize = fixedSize;
            this.images = new ObservableCollection<OutputImage>();

            this.UpdateGenerate();
        }

        public SourceImage Owner
        {
            get { return this.owner; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public double Width
        {
            get { return this.width; }
            set
            {
                if (this.width != value)
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
                if (this.height != value)
                {
                    this.height = value;
                    this.OnPropertyChanged(nameof(this.Height));
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

        public bool? Generate
        {
            get { return this.generate; }
            set { this.SetGenerate(value, true); }
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

        public void AddImage(OutputImage image)
        {
            if (image != null && !this.images.Contains(image))
            {
                this.images.Add(image);

                image.PropertyChanged += OnImagePropertyChanged;
            }

            this.UpdateGenerate();
        }

        public double GetScaledWidth(double scale)
        {
            return Math.Ceiling(this.width * scale);
        }

        public double GetScaledHeight(double scale)
        {
            return Math.Ceiling(this.height * scale);
        }

        private void OnImagePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(args.PropertyName) || args.PropertyName == nameof(OutputImage.Generate))
            {
                UpdateGenerate();
            }
        }

        private void UpdateGenerate()
        {
            int trueCount = 0;
            int falseCount = 0;

            foreach (OutputImage image in this.Images)
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

            bool? generate = null;

            if (trueCount > 0 && falseCount > 0)
            {
                // null
            }
            else if (trueCount > 0)
            {
                generate = true;
            }
            else if (falseCount > 0)
            {
                generate = false;
            }
            else
            {
                generate = false;
            }

            this.SetGenerate(generate, false);
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
    }
}
