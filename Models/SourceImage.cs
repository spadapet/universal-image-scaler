using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using UniversalImageScaler.Utility;

namespace UniversalImageScaler.Models
{
    public class SourceImage : ModelBase
    {
        private VSITEMSELECTION item;

        private string path;
        private int pathScaleStart;
        private int pathScaleLength;
        private BitmapSource image;
        private ImageFileType imageType;
        private double? scale;
        private ObservableCollection<OutputFeature> features;
        private OutputFeature feature;

        public SourceImage()
        {
            if (!WpfHelpers.IsDesignMode)
            {
                throw new InvalidOperationException("Must create SourceImage with a selected image");
            }

            this.path = @"x:\test\image.scale-400.png";
            this.pathScaleStart = 13;
            this.pathScaleLength = 11;
            this.image = OutputHelpers.CreateDesignTimeSourceImage();
            this.imageType = ImageFileType.Png;
            this.scale = 400;
            this.features = new ObservableCollection<OutputFeature>();

            OutputHelpers.PopulateDesignTimeFeatures(this);
        }

        public SourceImage(VSITEMSELECTION item)
        {
            this.item = item;
            this.features = new ObservableCollection<OutputFeature>();

            InitSourcePathAndScale();
            InitSourceImage();

            OutputHelpers.PopulateFeatures(this);
        }

        private void InitSourcePathAndScale()
        {
            object nameObj;
            if (this.item.pHier.GetCanonicalName(this.item.itemid, out this.path) >= 0 &&
                this.item.pHier.GetProperty(this.item.itemid, (int)__VSHPROPID.VSHPROPID_Name, out nameObj) >= 0 &&
                nameObj is string)
            {
                // Replace the lower-case name with the original case
                this.path = Path.Combine(Path.GetDirectoryName(this.path), (string)nameObj);

                Match match = Regex.Match(this.path, @"\.scale-(?<scale>\d{3})\.");
                if (match != null && match.Success && match.Index >= this.path.Length - Path.GetFileName(this.path).Length)
                {
                    this.pathScaleStart = match.Index;
                    this.pathScaleLength = match.Length;

                    int scale;
                    if (int.TryParse(match.Groups["scale"].Value, out scale) && scale > 0)
                    {
                        this.scale = scale / 100.0;
                    }
                }
                else
                {
                    string extension = Path.GetExtension(this.path);
                    this.pathScaleStart = this.path.Length - extension.Length;
                    this.pathScaleLength = 0;
                }
            }
            else
            {
                throw new ArgumentException("item must be an image");
            }
        }

        private void InitSourceImage()
        {
            this.imageType = ImageHelpers.GetFileType(this.path);
            this.image = ImageHelpers.LoadSourceImage(this.path);
        }

        public IEnumerable<OutputFeature> Features
        {
            get { return this.features; }
        }

        public OutputFeature Feature
        {
            get { return this.feature; }
            set
            {
                if (this.feature != value)
                {
                    this.feature = value;
                    this.OnPropertyChanged(nameof(this.Feature));
                }
            }
        }

        public void AddFeature(OutputFeature feature)
        {
            if (feature != null && !this.features.Contains(feature))
            {
                this.features.Add(feature);

                if (this.Feature == null)
                {
                    this.Feature = feature;
                }
            }
        }

        public ImageFileType ImageType
        {
            get { return this.imageType; }
        }

        public BitmapSource Image
        {
            get { return this.image; }
        }

        public double? Scale
        {
            get { return this.scale; }
        }

        public double ImagePixelWidth
        {
            get { return this.image != null ? this.image.PixelWidth : 0; }
        }

        public double ImagePixelHeight
        {
            get { return this.image != null ? this.image.PixelHeight : 0; }
        }

        public string FileName
        {
            get { return Path.GetFileName(this.path); }
        }

        public string UnscaledPath
        {
            get { return this.GetScaledPath(0); }
        }

        public string GetScaledPath(double scale)
        {
            string path = this.pathScaleLength > 0
                ? this.path.Remove(this.pathScaleStart, this.pathScaleLength - 1)
                : this.path;

            if (scale > 0)
            {
                path = path.Insert(this.pathScaleStart, $".scale-{(int)(scale * 100.0)}");
            }

            return path;
        }

        public string GetTargetSizePath(double targetSize, bool unplated)
        {
            string path = this.pathScaleLength > 0
                ? this.path.Remove(this.pathScaleStart, this.pathScaleLength - 1)
                : this.path;

            if (targetSize > 0)
            {
                path = unplated
                    ? path.Insert(this.pathScaleStart, $".targetsize-{(int)targetSize}_altform-unplated")
                    : path.Insert(this.pathScaleStart, $".targetsize-{(int)targetSize}");
            }

            return path;
        }

        public string FullDir
        {
            get { return Path.GetDirectoryName(this.path); }
        }

        public string FullPath
        {
            get { return this.path; }
        }
    }
}
