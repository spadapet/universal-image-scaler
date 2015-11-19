using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;

namespace UniversalImageScaler
{
    internal class ImageResizeInfo // : INotifyPropertyChanged
    {
        private VSITEMSELECTION item;
        private string path;
        private string dir;

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

        public IEnumerable<ManifestImageInfo> ManifestImages
        {
            get
            {
                yield return new ManifestImageInfo(71, 71);
                yield return new ManifestImageInfo(150, 150);
                yield return new ManifestImageInfo(310, 150);
                yield return new ManifestImageInfo(310, 310);
                yield return new ManifestImageInfo(44, 44, 256, 48, 24, 16);
                yield return new ManifestImageInfo(50, 50);
                yield return new ManifestImageInfo(24, 24);
                yield return new ManifestImageInfo(620, 300);
            }
        }
    }
}
