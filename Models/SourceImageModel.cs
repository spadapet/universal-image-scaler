using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Shell.Interop;
using UniversalImageScaler.Utility;

namespace UniversalImageScaler.Models
{
    internal class SourceImageModel : ModelBase
    {
        private VSITEMSELECTION item;
        private List<DestImageModel> images;

        private string sourcePath;
        private int sourcePathScaleStart;
        private int sourcePathScaleLength;
        private BitmapImage sourceImage;
        private ImageFileType sourceImageType;
        private double? sourceScale;

        public SourceImageModel(VSITEMSELECTION item)
        {
            this.item = item;

            InitSourcePathAndScale();
            InitSourceImage();
            InitDestImages();
            SetDestImageDefaults();
        }

        private void InitSourcePathAndScale()
        {
            object nameObj;
            if (item.pHier.GetCanonicalName(item.itemid, out this.sourcePath) >= 0 &&
                item.pHier.GetProperty(item.itemid, (int)__VSHPROPID.VSHPROPID_Name, out nameObj) >= 0 &&
                nameObj is string)
            {
                // Replace the lower-case name with the original case
                this.sourcePath = Path.Combine(Path.GetDirectoryName(this.sourcePath), (string)nameObj);

                Match match = Regex.Match(this.sourcePath, @"\.scale-(?<scale>\d{3})\.");
                if (match != null && match.Success && match.Index >= this.sourcePath.Length - Path.GetFileName(this.sourcePath).Length)
                {
                    this.sourcePathScaleStart = match.Index;
                    this.sourcePathScaleLength = match.Length;

                    int scale;
                    if (int.TryParse(match.Groups["scale"].Value, out scale) && scale > 0)
                    {
                        this.sourceScale = scale / 100.0;
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
            this.sourceImageType = ImageHelpers.GetFileType(this.sourcePath);
            this.sourceImage = ImageHelpers.LoadSourceImage(this.sourcePath);
        }

        private void InitDestImages()
        {
            this.images = new List<DestImageModel>();

            if (this.sourceScale.HasValue)
            {
                this.images.Add(new DestImageModel(this, "Square 71x71 Logo", 71, 71));
                this.images.Add(new DestImageModel(this, "Square 150x150 Logo", 150, 150));
                this.images.Add(new DestImageModel(this, "Wide 310x150 Logo", 310, 150));
                this.images.Add(new DestImageModel(this, "Square 310x310 Logo", 310, 310));
                this.images.Add(new DestImageModel(this, "Square 44x44 Logo", 44, 44, 256, 48, 24, 16));
                this.images.Add(new DestImageModel(this, "Store Logo", 50, 50));
                this.images.Add(new DestImageModel(this, "Badge Logo", 24, 24) { TransformType = ImageTransformType.WhiteOnly });
                this.images.Add(new DestImageModel(this, "Splash Screen", 620, 300));
            }
            else
            {
                this.images.Add(new DestImageModel(this, this.SourceFileName,
                    (int)(this.sourceImage.PixelWidth / this.sourceScale.Value),
                    (int)(this.sourceImage.PixelHeight / this.sourceScale.Value)));
            }
        }

        private void SetDestImageDefaults()
        {
            bool isSquare = this.sourceImage.PixelWidth == this.sourceImage.PixelHeight;

            foreach (DestImageModel info in this.images)
            {
                if (isSquare && info.IsSquare)
                {
                    info.Generate = true;
                }

                if (isSquare && info.IsWide)
                {
                    info.Generate = true;
                }
            }

            images.RemoveAll(info => !info.Enabled);
        }

        public ImageFileType SourceImageType
        {
            get { return this.sourceImageType; }
        }

        public BitmapSource SourceImage
        {
            get { return this.sourceImage; }
        }

        public string SourceFileName
        {
            get { return Path.GetFileName(this.sourcePath); }
        }

        public string SourceFileNameWithoutScale
        {
            get
            {
                string path = this.sourcePath.Remove(this.sourcePathScaleStart, this.sourcePathScaleLength - 1);
                return Path.GetFileName(path);
            }
        }

        public string FullSourceDir
        {
            get { return Path.GetDirectoryName(this.sourcePath); }
        }

        public string FullSourcePath
        {
            get { return this.sourcePath; }
        }

        public IEnumerable<DestImageModel> Images
        {
            get { return this.images; }
        }

        public string HeaderText
        {
            get
            {
                return this.IsManifestImage
                    ? "Select manifest images to generate:"
                    : "Select smaller scales to generate:";
            }
        }

        public bool IsManifestImage
        {
            get { return !this.sourceScale.HasValue; }
        }
    }
}
