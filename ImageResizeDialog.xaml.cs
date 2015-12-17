using System.Diagnostics;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;
using UniversalImageScaler.Models;
using UniversalImageScaler.Utility;

namespace UniversalImageScaler
{
    internal partial class ImageResizeDialog : DialogWindow
    {
        private SourceImage item;

        public ImageResizeDialog()
            : this(new SourceImage())
        {
            Debug.Assert(WpfHelpers.IsDesignMode);
        }

        public ImageResizeDialog(SourceImage item)
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
