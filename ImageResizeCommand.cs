using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using UniversalImageScaler.Image;
using UniversalImageScaler.Models;
using UniversalImageScaler.Utility;
using OLECMDF = Microsoft.VisualStudio.OLE.Interop.OLECMDF;
using Task = System.Threading.Tasks.Task;

namespace UniversalImageScaler
{
    internal sealed class ImageResizeCommand : MenuCommand
    {
        private IServiceProvider serviceProvider;
        private IVsMonitorSelection monitorSelection;

        public ImageResizeCommand(IServiceProvider serviceProvider, CommandID commandId)
            : base(null, commandId)
        {
            this.serviceProvider = serviceProvider;
            this.monitorSelection = serviceProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
        }

        public override async void Invoke()
        {
            VSITEMSELECTION sel = this.monitorSelection.GetSelectedSourceImage();
            if (sel.pHier != null)
            {
                try
                {
                    using (SourceImage source = new SourceImage(sel))
                    {
                        ImageResizeDialog dialog = new ImageResizeDialog(source);
                        bool? result = dialog.ShowModal();

                        if (result == true)
                        {
                            await GenerateImagesMainThread(source);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.Message);
                    ImageResizePackage.Instance.TelemetryClient.TrackException(ex);
                }
            }
        }

        public override int OleStatus
        {
            get
            {
                OLECMDF status = 0;
                VSITEMSELECTION sel = this.monitorSelection.GetSelectedSourceImage();

                if (sel.pHier != null)
                {
                    status = OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED;
                }

                return (int)status;
            }
        }

        private async Task GenerateImagesMainThread(SourceImage source)
        {
            CommonMessagePump pump = new CommonMessagePump();
            pump.AllowCancel = true;
            pump.WaitTitle = "Creating scaled images";
            pump.WaitText = "Adding to the project takes time...";

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            Task task = Task.Run(() => GenerateImagesBackgroundThread(source, token, pump), token);

            CommonMessagePumpExitCode code = pump.ModalWaitForHandles(((IAsyncResult)task).AsyncWaitHandle);
            tokenSource.Cancel();
            await task;
        }

        private void GenerateImagesBackgroundThread(SourceImage source, CancellationToken token, CommonMessagePump mainThreadPump)
        {
            int totalSets = source.SetsToGenerate.Count();
            int curSet = 0;

            foreach (OutputSet set in source.SetsToGenerate)
            {
                if (!token.IsCancellationRequested)
                {
                    curSet++;
                    mainThreadPump.WaitText = $"Checking existing images ({curSet} of {totalSets}):\r\n{set.UnscaledPath}";
                    this.OnGeneratingSet(set);
                }
            }

            int totalImages = source.ImagesToGenerate.Count();
            int curImage = 0;

            foreach (OutputImage image in source.ImagesToGenerate)
            {
                if (!token.IsCancellationRequested)
                {
                    curImage++;
                    mainThreadPump.WaitText = $"Adding to the project ({curImage} of {totalImages}):\r\n{image.Path}";
                    this.GenerateImage(image);
                    this.OnGeneratedImage(image);
                }
            }
        }

        private void GenerateImage(OutputImage image)
        {
            SourceImage sourceImage = image.Owner.Owner;
            BitmapSource source = sourceImage.Frame.Render(image.PixelWidth, image.PixelHeight, image.TransformType);
            ImageHelpers.SaveBitmap(source, image.OutputFileType, image.Path);
        }

        private void OnGeneratingSet(OutputSet set)
        {
            // Remove existing images

            ImageResizePackage.Instance.MainThreadHelper.Invoke(() =>
            {
                IVsHierarchy hierarchy = set.Owner.Item.pHier;
                uint itemId = hierarchy.FindItemId(set.UnscaledPath);
                EnvDTE.ProjectItem item = hierarchy.GetProjectItem(itemId);
                if (item != null)
                {
                    EnvDTE.Property deployProp = item.GetProperty("DeploymentContent");
                    EnvDTE.Property typeProp = item.GetProperty("ItemType");

                    if (deployProp != null)
                    {
                        // C++ project item
                        if (deployProp.Value is bool && (bool)deployProp.Value)
                        {
                            deployProp.Value = false;
                        }
                    }
                    else if (typeProp != null)
                    {
                        // C# project item
                        if (typeProp.Value is string && string.Equals((string)typeProp.Value, "Content", StringComparison.OrdinalIgnoreCase))
                        {
                            typeProp.Value = "None";
                        }
                    }
                }
            });
        }

        private void OnGeneratedImage(OutputImage outputImage)
        {
            // Add the new image to the project

            ImageResizePackage.Instance.MainThreadHelper.Invoke(() =>
            {
                IVsHierarchy hierarchy = outputImage.Owner.Owner.Item.pHier;
                uint itemId = outputImage.Owner.Owner.Item.itemid;
                uint parentId = hierarchy.FindParentId(itemId);
                hierarchy.AddItem(parentId, outputImage.Path);
            });
        }
    }
}
