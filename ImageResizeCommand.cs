using System;
using System.ComponentModel.Design;
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
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                            await this.GenerateImagesMainThread(source);
                        }
                    }
                }
                catch
                {
                    // Don't let exceptions escape and take down VS
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

            Task task = Task.Run(() => this.GenerateImagesBackgroundThread(source, token, pump), token);

            CommonMessagePumpExitCode code = pump.ModalWaitForHandles(((IAsyncResult)task).AsyncWaitHandle);
            tokenSource.Cancel();
            await task;
            await this.DeleteLargeImages(source);
        }

        private async Task DeleteLargeImages(SourceImage source)
        {
            List<OutputImage> largeImages = new List<OutputImage>();
            foreach (OutputImage image in source.ImagesToGenerate)
            {
                if (image.FileSizeTooLarge)
                {
                    image.MarkedForDeletion = true;
                    largeImages.Add(image);
                }
            }

            if (largeImages.Count > 0)
            {
                DeleteLargeImagesDialog dialog = new DeleteLargeImagesDialog(largeImages);
                bool? result = dialog.ShowModal();

                if (result == true)
                {
                    await this.DeleteImagesMainThread(largeImages.Where(i => i.MarkedForDeletion));
                }
            }
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

        private async Task DeleteImagesMainThread(IEnumerable<OutputImage> images)
        {
            CommonMessagePump pump = new CommonMessagePump();
            pump.AllowCancel = true;
            pump.WaitTitle = "Deleting large images";
            pump.WaitText = "Deleting files takes time...";

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            Task task = Task.Run(() => this.DeleteImagesBackgroundThread(images, token, pump), token);

            CommonMessagePumpExitCode code = pump.ModalWaitForHandles(((IAsyncResult)task).AsyncWaitHandle);
            tokenSource.Cancel();
            await task;
        }

        private void DeleteImagesBackgroundThread(IEnumerable<OutputImage> images, CancellationToken token, CommonMessagePump mainThreadPump)
        {
            int totalImages = images.Count();
            int curImage = 0;

            foreach (OutputImage image in images)
            {
                if (!token.IsCancellationRequested)
                {
                    curImage++;
                    mainThreadPump.WaitText = $"Deleting {curImage} of {totalImages}:\r\n{image.Path}";

                    IVsHierarchy hierarchy = image.Owner.Owner.Item.pHier;
                    uint itemId = hierarchy.FindItemId(image.Path);
                    if (itemId != 0)
                    {
                        DteHelpers.RemoveItemId(hierarchy, itemId);

                        if (File.Exists(image.Path))
                        {
                            try
                            {
                                File.Delete(image.Path);
                            }
                            catch
                            {
                                // don't care
                            }
                        }
                    }
                }
            }
        }

        private void GenerateImage(OutputImage image)
        {
            if (File.Exists(image.Path))
            {
                FileInfo info = new FileInfo(image.Path);
                image.OldFileSize = info.Length;
            }

            SourceImage sourceImage = image.Owner.Owner;
            BitmapSource source = sourceImage.Frame.Render(image.PixelWidth, image.PixelHeight, image.TransformType);
            ImageHelpers.SaveBitmap(source, image.OutputFileType, image.Path);

            if (File.Exists(image.Path))
            {
                FileInfo info = new FileInfo(image.Path);
                image.NewFileSize = info.Length;
            }
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
