using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;

namespace UniversalImageScaler
{
    public class ImageResizeInfo
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
    }
}
