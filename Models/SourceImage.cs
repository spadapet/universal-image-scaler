using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Shell.Interop;
using UniversalImageScaler.Utility;

namespace UniversalImageScaler.Models
{
    internal class SourceImage : ModelBase
    {
        private VSITEMSELECTION item;

        private string path;
        private int pathScaleStart;
        private int pathScaleLength;
        private BitmapImage image;
        private ImageFileType imageType;
        private double? scale;

        public SourceImage(VSITEMSELECTION item)
        {
            this.item = item;

            InitSourcePathAndScale();
            InitSourceImage();
            InitOutputSets();
        }

        private void InitSourcePathAndScale()
        {
            object nameObj;
            if (item.pHier.GetCanonicalName(item.itemid, out this.path) >= 0 &&
                item.pHier.GetProperty(item.itemid, (int)__VSHPROPID.VSHPROPID_Name, out nameObj) >= 0 &&
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

        public string FileNameWithoutScale
        {
            get
            {
                string path = this.pathScaleLength > 0
                    ? this.path.Remove(this.pathScaleStart, this.pathScaleLength - 1)
                    : this.path;

                return Path.GetFileName(path);
            }
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
