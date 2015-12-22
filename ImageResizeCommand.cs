using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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

            Task task = Task.Run(() =>
            {
                foreach (OutputSet set in source.SetsToGenerate)
                {
                    if (!token.IsCancellationRequested)
                    {
                        pump.WaitText = $"Checking existing images:\r\n{set.UnscaledPath}";
                        this.OnGeneratingSet(set);
                    }
                }

                foreach (OutputImage image in source.ImagesToGenerate)
                {
                    if (!token.IsCancellationRequested)
                    {
                        pump.WaitText = $"Adding to the project:\r\n{image.Path}";
                        this.GenerateImage(image);
                        this.OnGeneratedImage(image);
                    }
                }
            }, token);

            CommonMessagePumpExitCode code = pump.ModalWaitForHandles(((IAsyncResult)task).AsyncWaitHandle);
            tokenSource.Cancel();
            await task;
        }

        private void GenerateImage(OutputImage image)
        {
            BitmapSource source = ImageHelpers.ScaleSourceImage(image.Owner.Owner.Image, image.PixelWidth, image.PixelHeight);
            source = ImageHelpers.TransformImage(source, image.TransformType);
            ImageHelpers.Save(source, image.OutputFileType, image.Path);
        }

        private void OnGeneratingSet(OutputSet set)
        {
            // Remove existing images

            ImageResizePackage.Instance.MainThreadHelper.Invoke(() =>
            {
                string file = set.UnscaledPath;
                EnvDTE.Project dteProject = set.Owner.Item.pHier.GetProject();
                EnvDTE.ProjectItem dteItem = this.serviceProvider.FindProjectItem(file);
                if (dteItem != null)
                {
                    try
                    {
                        dteItem.Remove();
                    }
                    catch
                    {
                    }
                }
            });
        }

        private void OnGeneratedImage(OutputImage outputImage)
        {
            // Add the new image to the project

            ImageResizePackage.Instance.MainThreadHelper.Invoke(() =>
            {
                EnvDTE.Project dteProject = outputImage.Owner.Owner.Item.pHier.GetProject();
                string file = outputImage.Path;

                if (this.serviceProvider.FindProjectItem(file) == null)
                {
                    try
                    {
                        dteProject.ProjectItems.AddFromFile(file);
                    }
                    catch
                    {
                    }
                }
            });
        }
    }
}
