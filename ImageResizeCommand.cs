using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using OLECMDF = Microsoft.VisualStudio.OLE.Interop.OLECMDF;
using Task = System.Threading.Tasks.Task;

namespace UniversalImageScaler
{
    internal sealed class ImageResizeCommand : MenuCommand
    {
        private IServiceProvider serviceProvider;
        private IVsMonitorSelection monitorSelection;
        private ThreadHelper mainThreadHelper;

        public ImageResizeCommand(IServiceProvider serviceProvider, CommandID commandId)
            : base(null, commandId)
        {
            this.serviceProvider = serviceProvider;
            this.monitorSelection = serviceProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            this.mainThreadHelper = ThreadHelper.Generic;
        }

        public override async void Invoke()
        {
            VSITEMSELECTION sel = this.SelectedItem;
            if (sel.pHier != null)
            {
                try
                {
                    ImageResizeInfo item = new ImageResizeInfo(sel);
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

                if (this.SelectedItem.pHier != null)
                {
                    status = OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED;
                }

                return (int)status;
            }
        }

        private VSITEMSELECTION SelectedItem
        {
            get
            {
                VSITEMSELECTION sel = new VSITEMSELECTION();

                if (this.monitorSelection != null)
                {
                    IntPtr hierarchyPtr = IntPtr.Zero;
                    IntPtr selectionContainerPtr = IntPtr.Zero;

                    try
                    {
                        IVsMultiItemSelect multiSelect;
                        if (this.monitorSelection.GetCurrentSelection(out hierarchyPtr, out sel.itemid, out multiSelect, out selectionContainerPtr) >= 0)
                        {
                            sel.pHier = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
                            if (sel.pHier != null && multiSelect != null)
                            {
                                uint items;
                                int singleHierarchy;
                                if (multiSelect.GetSelectionInfo(out items, out singleHierarchy) >= 0 && items != 1)
                                {
                                    // Must have only one item selected 
                                    sel.pHier = null;
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (hierarchyPtr != IntPtr.Zero)
                        {
                            Marshal.Release(hierarchyPtr);
                            hierarchyPtr = IntPtr.Zero;
                        }

                        if (selectionContainerPtr != IntPtr.Zero)
                        {
                            Marshal.Release(selectionContainerPtr);
                            selectionContainerPtr = IntPtr.Zero;
                        }
                    }
                }

                bool isImage = false;

                if (sel.pHier != null)
                {
                    object nameObj;
                    if (sel.pHier.GetProperty(sel.itemid, (int)__VSHPROPID.VSHPROPID_Name, out nameObj) >= 0 && nameObj is string)
                    {
                        string name = (string)nameObj;
                        if (!string.IsNullOrEmpty(name))
                        {
                            isImage = name.EndsWith(".png");
                        }
                    }
                }

                if (!isImage)
                {
                    sel = new VSITEMSELECTION();
                }

                return sel;
            }
        }

        private async Task GenerateImagesMainThread(VSITEMSELECTION sel, ImageResizeInfo item)
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

        private IEnumerable<string> GenerateImages(VSITEMSELECTION sel, ImageResizeInfo item, CancellationToken token)
        {
            byte[] fileBytes = File.ReadAllBytes(item.FullPath);

            foreach (ImageInfo image in item.Images)
            {
                OnBeforeGenerateImages(sel, item, image);

                if (image.Generate && image.Enabled)
                {
                    foreach (double scale in image.Scales)
                    {
                        if (token.IsCancellationRequested)
                        {
                            yield break;
                        }

                        BitmapImage bitmap = new BitmapImage();
                        BitmapSource bitmapSource = bitmap;
                        bitmap.BeginInit();
                        bitmap.DecodePixelWidth = image.GetScaledWidth(scale);
                        bitmap.DecodePixelHeight = image.GetScaledHeight(scale);
                        bitmap.StreamSource = new MemoryStream(fileBytes);
                        bitmap.EndInit();

                        if (image.Name.StartsWith("Badge"))
                        {
                            FormatConvertedBitmap cb = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
                            WriteableBitmap wb = new WriteableBitmap(bitmap);
                            byte[] pixels = new byte[wb.PixelWidth * wb.PixelHeight * 4];
                            wb.CopyPixels(pixels, wb.PixelWidth * 4, 0);

                            for (int y = 0; y < wb.PixelHeight; y++)
                            {
                                for (int x = 0; x < wb.PixelWidth; x++)
                                {
                                    int start = y * wb.PixelWidth * 4 + x * 4;
                                    byte val = (pixels[start + 3] > 127) ? (byte)255 : (byte)0;
                                    pixels[start + 0] = val;
                                    pixels[start + 1] = val;
                                    pixels[start + 2] = val;
                                    pixels[start + 3] = val;
                                }
                            }

                            wb.WritePixels(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight), pixels, wb.PixelWidth * 4, 0);
                            bitmapSource = wb;
                        }

                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                        MemoryStream streamOut = new MemoryStream();
                        encoder.Save(streamOut);

                        byte[] newFileBytes = streamOut.ToArray();
                        string destPath = Path.Combine(item.FullDir, image.GetScaledFileName(scale));
                        File.WriteAllBytes(destPath, newFileBytes);
                        yield return destPath;
                    }

                    foreach (double targetSize in image.TargetSizes)
                    {
                        if (token.IsCancellationRequested)
                        {
                            yield break;
                        }

                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.DecodePixelWidth = (int)targetSize;
                        bitmap.DecodePixelHeight = (int)targetSize;
                        bitmap.StreamSource = new MemoryStream(fileBytes);
                        bitmap.EndInit();

                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));

                        MemoryStream streamOut = new MemoryStream();
                        encoder.Save(streamOut);

                        byte[] newFileBytes = streamOut.ToArray();
                        string destPath = Path.Combine(item.FullDir, image.GetTargetSizeFileName(targetSize));
                        File.WriteAllBytes(destPath, newFileBytes);
                        yield return destPath;

                        string unplatedPath = Path.Combine(item.FullDir, image.GetUnplatedTargetSizeFileName(targetSize));
                        File.Copy(destPath, unplatedPath, true);
                        yield return unplatedPath;
                    }
                }
            }
        }

        private void OnBeforeGenerateImages(VSITEMSELECTION sel, ImageResizeInfo item, ImageInfo image)
        {
            // Remove existing images

            this.mainThreadHelper.Invoke(() =>
            {
                string file = Path.Combine(item.FullDir, image.FileNameAndExtension);
                EnvDTE.Project dteProject = this.GetProject(sel.pHier);
                EnvDTE.ProjectItem dteItem = this.FindProjectItem(file);
                if (dteItem != null)
                {
                    dteItem.Delete();
                }
            });
        }

        private void OnGeneratedImage(VSITEMSELECTION sel, string file)
        {
            // Add the new image to the project

            this.mainThreadHelper.Invoke(() =>
            {
                EnvDTE.Project dteProject = this.GetProject(sel.pHier);
                if (this.FindProjectItem(file) == null)
                {
                    dteProject.ProjectItems.AddFromFile(file);
                }
            });
        }

        private EnvDTE.ProjectItem FindProjectItem(string file)
        {
            EnvDTE.DTE dte = this.serviceProvider.GetService(typeof(SDTE)) as EnvDTE.DTE;
            return dte.Solution.FindProjectItem(file);
        }

        private EnvDTE.Project GetProject(IVsHierarchy hierarchy)
        {
            object extObj;
            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObj);
            return extObj as EnvDTE.Project;
        }
    }
}
