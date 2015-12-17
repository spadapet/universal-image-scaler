using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Shell.Interop;
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
                Debug.Fail("Only use this constructor in design mode");
                return;
            }

            this.path = @"x:\test\image.scale-400.png";
            this.pathScaleStart = 13;
            this.pathScaleLength = 11;
            this.imageType = ImageFileType.Png;
            this.scale = 400;
            this.features = new ObservableCollection<OutputFeature>();

            byte[] bytes = new byte[32 * 32 * 4];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 255;
            }

            WriteableBitmap image = new WriteableBitmap(32, 32, 96, 96, PixelFormats.Bgra32, null);
            image.WritePixels(new Int32Rect(0, 0, 32, 32), bytes, 32 * 4, 0);
            image.Freeze();
            this.image = image;

            OutputFeature feature = new OutputFeature("Test feature");
            this.AddFeature(feature);

            OutputSet set = new OutputSet(this, "Test image", 8, 8, true);
            feature.AddSet(set);

            OutputImage output = new OutputImageScale(set, 200);
            set.AddImage(output);

            output = new OutputImageScale(set, 100);
            set.AddImage(output);

            output = new OutputImageTargetSize(set, 16, true);
            set.AddImage(output);

            output = new OutputImageTargetSize(set, 16, false);
            set.AddImage(output);

            // Another feature for fun
            feature = new OutputFeature("Another test feature");
            feature.AddSet(set);
            this.AddFeature(feature);
        }

        public SourceImage(VSITEMSELECTION item)
        {
            this.item = item;
            this.features = new ObservableCollection<OutputFeature>();

            InitSourcePathAndScale();
            InitSourceImage();
            InitOutputSets();
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

        private void InitOutputSets()
        {
            //this.images = new List<OutputImage>();

            //if (this.sourceScale.HasValue)
            //{
            //    this.images.Add(new OutputImage(this, "Square 71x71 Logo", 71, 71));
            //    this.images.Add(new OutputImage(this, "Square 150x150 Logo", 150, 150));
            //    this.images.Add(new OutputImage(this, "Wide 310x150 Logo", 310, 150));
            //    this.images.Add(new OutputImage(this, "Square 310x310 Logo", 310, 310));
            //    this.images.Add(new OutputImage(this, "Square 44x44 Logo", 44, 44, 256, 48, 24, 16));
            //    this.images.Add(new OutputImage(this, "Store Logo", 50, 50));
            //    this.images.Add(new OutputImage(this, "Badge Logo", 24, 24) { TransformType = ImageTransformType.WhiteOnly });
            //    this.images.Add(new OutputImage(this, "Splash Screen", 620, 300));
            //}
            //else
            //{
            //    this.images.Add(new OutputImage(this, this.FileName,
            //        (int)(this.sourceImage.PixelWidth / this.sourceScale.Value),
            //        (int)(this.sourceImage.PixelHeight / this.sourceScale.Value)));
            //}
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
