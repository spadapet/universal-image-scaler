using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Shell.Interop;

namespace UniversalImageScaler
{
    internal class ImageResizeInfo : ModelBase
    {
        private VSITEMSELECTION item;
        private string name;
        private string dir;
        private string path;
        private double scale;
        private List<ImageInfo> images;
        private BitmapImage bitmap;

        public ImageResizeInfo(VSITEMSELECTION item)
        {
            this.item = item;

            if (item.pHier == null)
            {
                throw new ArgumentNullException("item.pHier");
            }

            object nameObj;
            if (item.pHier.GetCanonicalName(item.itemid, out this.path) >= 0 &&
                item.pHier.GetProperty(item.itemid, (int)__VSHPROPID.VSHPROPID_Name, out nameObj) >= 0)
            {
                this.name = (string)nameObj;
                this.dir = Path.GetDirectoryName(this.path);
                this.path = Path.Combine(this.dir, this.name);

                Match match = Regex.Match(this.name, @".*\.scale-(?<scale>\d{3})\.(?:png|jpg)");
                if (match != null && match.Success)
                {
                    int scale;
                    if (int.TryParse(match.Groups["scale"].Value, out scale))
                    {
                        this.scale = scale / 100.0;
                    }
                }
            }
            else
            {
                throw new ArgumentException("item must be an image");
            }

            this.bitmap = new BitmapImage(new Uri(this.path));
            this.images = new List<ImageInfo>();

            if (this.scale == 0.0)
            {
                this.images.Add(new ImageInfo(this, "Square 71x71 Logo", 71, 71));
                this.images.Add(new ImageInfo(this, "Square 150x150 Logo", 150, 150));
                this.images.Add(new ImageInfo(this, "Wide 310x150 Logo", 310, 150));
                this.images.Add(new ImageInfo(this, "Square 310x310 Logo", 310, 310));
                this.images.Add(new ImageInfo(this, "Square 44x44 Logo", 44, 44, 256, 48, 24, 16));
                this.images.Add(new ImageInfo(this, "Store Logo", 50, 50));
                this.images.Add(new ImageInfo(this, "Badge Logo", 24, 24));
                this.images.Add(new ImageInfo(this, "Splash Screen", 620, 300));
            }
            else
            {
                this.images.Add(new ImageInfo(this, this.name,
                    (int)(this.bitmap.PixelWidth / this.scale),
                    (int)(this.bitmap.PixelHeight / this.scale)));
            }

            foreach (ImageInfo info in this.images)
            {
                if (this.IsSquare && info.IsSquare)
                {
                    info.Generate = true;
                }

                if (this.IsWide && info.IsWide)
                {
                    info.Generate = true;
                }

                if (this.IsSquare != info.IsSquare &&
                    this.IsWide != info.IsWide)
                {
                    info.Enabled = false;
                }
            }

            this.images.RemoveAll(info => !info.Enabled);
        }

        public BitmapImage Bitmap
        {
            get
            {
                return this.bitmap;
            }
        }

        public double Scale
        {
            get
            {
                return this.scale;
            }
        }

        public string FileName
        {
            get
            {
                return this.name;
            }
        }

        public string FullDir
        {
            get
            {
                return this.dir;
            }
        }

        public string FullPath
        {
            get
            {
                return this.path;
            }
        }

        public IEnumerable<ImageInfo> Images
        {
            get
            {
                return this.images;
            }
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

        public bool IsSquare
        {
            get
            {
                return this.bitmap != null && this.bitmap.PixelWidth == this.bitmap.PixelHeight;
            }
        }

        public bool IsWide
        {
            get
            {
                return this.bitmap != null && this.bitmap.PixelWidth > this.bitmap.PixelHeight;
            }
        }

        public bool IsManifestImage
        {
            get
            {
                return this.scale == 0.0;
            }
        }
    }
}
