using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UniversalImageScaler.Models
{
    internal class OutputSet
    {
        private string name;
        private double width;
        private double height;
        private ObservableCollection<OutputImage> images;

        public OutputSet(string name, double width, double height)
        {
            this.name = name;
            this.width = width;
            this.height = height;
            this.images = new ObservableCollection<OutputImage>();
        }

        public string Name
        {
            get { return this.name; }
        }

        public double Width
        {
            get { return this.width; }
        }

        public double Height
        {
            get { return this.height; }
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
    }
}
