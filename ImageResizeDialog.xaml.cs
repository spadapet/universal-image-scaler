using System.Windows;

namespace UniversalImageScaler
{
    internal partial class ImageResizeDialog : Window
    {
        private ImageResizeItem item;

        public ImageResizeDialog()
            : this(null)
        {
        }

        public ImageResizeDialog(ImageResizeItem item)
        {
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
