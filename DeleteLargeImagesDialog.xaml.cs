using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;
using UniversalImageScaler.Models;
using UniversalImageScaler.Utility;
using System.Collections.ObjectModel;

namespace UniversalImageScaler
{
    internal partial class DeleteLargeImagesDialog : DialogWindow
    {
        private ObservableCollection<OutputImage> largeImages;

        public DeleteLargeImagesDialog()
            : this(new OutputImage[0])
        {
            Debug.Assert(WpfHelpers.IsDesignMode);
        }

        public DeleteLargeImagesDialog(IEnumerable<OutputImage> largeImages)
        {
            this.largeImages = new ObservableCollection<OutputImage>(largeImages);
            this.DataContext = this.largeImages;

            this.InitializeComponent();
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
