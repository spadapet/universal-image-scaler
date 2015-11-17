using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using OLECMDF = Microsoft.VisualStudio.OLE.Interop.OLECMDF;

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

        public override void Invoke()
        {
            VsShellUtilities.ShowMessageBox(
                this.serviceProvider,
                "Hello1",
                "Hello2",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        public override int OleStatus
        {
            get
            {
                OLECMDF status = 0;

                if (SelectedItem.pHier != null)
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
                            isImage = name.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
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
    }
}
