using Microsoft.VisualStudio.Shell.Interop;

namespace UniversalImageScaler
{
    internal class ImageResizeItem
    {
        private VSITEMSELECTION item;

        public ImageResizeItem(VSITEMSELECTION item)
        {
            this.item = item;
        }
    }
}
