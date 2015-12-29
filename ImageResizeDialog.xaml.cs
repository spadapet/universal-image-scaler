using System;
using System.Collections.Generic;
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

            Dictionary<string, string> eventProps = new Dictionary<string, string>()
            {
                [nameof(item.Image.FileType)] = item.Image.FileType.ToString(),
                [nameof(item.CustomPixelHeight)] = item.CustomPixelHeight.ToString(),
                [nameof(item.CustomPixelWidth)] = item.CustomPixelWidth.ToString(),
                [nameof(item.FrameHasPixelSize)] = item.FrameHasPixelSize.ToString(),
                [nameof(item.FramePixelWidth)] = item.FramePixelWidth.ToString(),
                [nameof(item.FramePixelHeight)] = item.FramePixelHeight.ToString(),
                [nameof(item.HasCustomSize)] = item.HasCustomSize.ToString(),
                [nameof(item.Scale)] = item.Scale.ToString(),
            };

            ImageResizePackage.Instance.TelemetryClient.TrackEvent("ShowImageResizeDialog", eventProps);
        }

        protected override void InvokeDialogHelp()
        {
            try
            {
                Process.Start("https://visualstudiogallery.msdn.microsoft.com/824f5375-b0c7-4d79-b9bf-04653876ba53");
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
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
