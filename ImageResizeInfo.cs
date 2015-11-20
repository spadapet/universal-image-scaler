using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Shell.Interop;

namespace UniversalImageScaler
{
    internal class ImageResizeInfo : ModelBase
    {
        private VSITEMSELECTION item;
        private string path;
        private string dir;
        private List<ManifestImageInfo> manifestImages;
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
                this.dir = Path.GetDirectoryName(this.path);
                this.path = Path.Combine(this.dir, (string)nameObj);
            }
            else
            {
                throw new ArgumentException("item must be an image");
            }

            this.bitmap = new BitmapImage(new Uri(this.path));

            this.manifestImages = new List<ManifestImageInfo>()
            {
                new ManifestImageInfo("Small Tile Logo", 71, 71),
                new ManifestImageInfo("Medium Tile Logo", 150, 150),
                new ManifestImageInfo("Wide Tile Logo", 310, 150),
                new ManifestImageInfo("Large Tile Logo", 310, 310),
                new ManifestImageInfo("App List Logo", 44, 44, 256, 48, 24, 16),
                new ManifestImageInfo("Store Logo", 50, 50),
                new ManifestImageInfo("Badge Logo", 24, 24),
                new ManifestImageInfo("Splash Screen", 620, 300),
            };

            foreach (ManifestImageInfo info in this.manifestImages)
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
        }

        public string FullPath
        {
            get
            {
                return this.path;
            }
        }

        public string FileName
        {
            get
            {
                return Path.GetFileName(this.path);
            }
        }

        public string FullDir
        {
            get
            {
                return Path.GetDirectoryName(this.path);
            }
        }

        public IEnumerable<ManifestImageInfo> ManifestImages
        {
            get
            {
                return this.manifestImages;
            }
        }

        public bool IsSquare
        {
            get
            {
                return this.bitmap != null && this.bitmap.Width == this.bitmap.Height;
            }
        }

        public bool IsWide
        {
            get
            {
                return this.bitmap != null && this.bitmap.Width > this.bitmap.Height;
            }
        }
    }
}
