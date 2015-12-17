using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

        private bool? Generate
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

        public IEnumerable<OutputImage> Images
        {
            get { return this.images; }
        }

        public void AddImage(OutputImage image)
        {
            if (image != null && !this.images.Contains(image))
            {
                this.images.Add(image);
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
    }
}
