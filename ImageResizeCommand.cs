﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
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
                    SourceImageModel item = new SourceImageModel(sel);
                    ImageResizeDialog dialog = new ImageResizeDialog(item);
                    bool? result = dialog.ShowModal();

                    if (result.HasValue && result.Value)
                    {
                        await GenerateImagesMainThread(sel, item);
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

        private async Task GenerateImagesMainThread(VSITEMSELECTION sel, SourceImageModel item)
        {
            CommonMessagePump pump = new CommonMessagePump();
            pump.AllowCancel = true;
            pump.WaitTitle = "Creating scaled images";
            pump.WaitText = "Adding to the project takes time...";

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            Task task = Task.Run(() =>
            {
                foreach (string file in this.GenerateImages(sel, item, token))
                {
                    pump.WaitText = $"Adding to the project:\r\n{file}";
                    OnGeneratedImage(sel, file);
                }
            }, token);

            CommonMessagePumpExitCode code = pump.ModalWaitForHandles(((IAsyncResult)task).AsyncWaitHandle);
            tokenSource.Cancel();
            await task;
        }

        private IEnumerable<string> GenerateImages(VSITEMSELECTION sel, SourceImageModel item, CancellationToken token)
        {
            byte[] fileBytes = File.ReadAllBytes(item.FullSourcePath);

            foreach (DestImageModel image in item.Images)
            {
                OnBeforeGenerateImages(sel, item, image);

                if (image.Generate && image.Enabled)
                {
                    foreach (double scale in image.Scales)
                    {
                        if (!token.IsCancellationRequested)
                        {
                            string destPath = Path.Combine(item.FullSourceDir, image.GetScaledFileName(scale));
                            if (!string.Equals(item.FullSourcePath, destPath, StringComparison.OrdinalIgnoreCase))
                            {
                                BitmapSource source = ImageHelpers.ScaleSourceImage(
                                    item.SourceImage, image.GetScaledWidth(scale), image.GetScaledHeight(scale));
                                source = ImageHelpers.TransformImage(source, image.TransformType);
                                ImageHelpers.Save(source, item.SourceImageType, destPath);
                                yield return destPath;
                            }
                        }
                    }

                    foreach (double targetSize in image.TargetSizes)
                    {
                        if (!token.IsCancellationRequested)
                        {
                            string destPath = Path.Combine(item.FullSourceDir, image.GetTargetSizeFileName(targetSize));
                            if (!string.Equals(item.FullSourcePath, destPath, StringComparison.OrdinalIgnoreCase))
                            {
                                BitmapSource source = ImageHelpers.ScaleSourceImage(item.SourceImage, (int)targetSize, (int)targetSize);
                                ImageHelpers.Save(source, item.SourceImageType, destPath);
                                yield return destPath;

                                string unplatedPath = Path.Combine(item.FullSourceDir, image.GetUnplatedTargetSizeFileName(targetSize));
                                if (!string.Equals(item.FullSourcePath, unplatedPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    File.Copy(destPath, unplatedPath, true);
                                    yield return unplatedPath;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnBeforeGenerateImages(VSITEMSELECTION sel, SourceImageModel item, DestImageModel image)
        {
            // Remove existing images

            ImageResizePackage.Instance.MainThreadHelper.Invoke(() =>
            {
                string file = Path.Combine(item.FullSourceDir, image.FileNameAndExtension);
                EnvDTE.Project dteProject = sel.pHier.GetProject();
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

        private void OnGeneratedImage(VSITEMSELECTION sel, string file)
        {
            // Add the new image to the project

            ImageResizePackage.Instance.MainThreadHelper.Invoke(() =>
            {
                EnvDTE.Project dteProject = sel.pHier.GetProject();
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
