using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace UniversalImageScaler
{
    internal partial class ImageResizeDialog : DialogWindow
    {
        private ImageResizeInfo item;

        public ImageResizeDialog()
            : this(null)
        {
        }

        public ImageResizeDialog(ImageResizeInfo item)
        {
            this.item = item;
            this.DataContext = item;

            InitializeComponent();
        }

        private void OnOk(object sender, RoutedEventArgs args)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void OnCancel(object sender, RoutedEventArgs args)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
