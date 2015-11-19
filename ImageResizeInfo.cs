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

            this.manifestImages = new List<ManifestImageInfo>()
            {
                new ManifestImageInfo("SmallTileLogo", 71, 71),
                new ManifestImageInfo("MediumTileLogo", 150, 150),
                new ManifestImageInfo("WideTileLogo", 310, 150),
                new ManifestImageInfo("LargeTileLogo", 310, 310),
                new ManifestImageInfo("AppListLogo", 44, 44, 256, 48, 24, 16),
                new ManifestImageInfo("StoreLogo", 50, 50),
                new ManifestImageInfo("BadgeLogo", 24, 24),
                new ManifestImageInfo("SplashScreen", 620, 300),
            };

            this.bitmap = new BitmapImage(new Uri(this.path));

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
