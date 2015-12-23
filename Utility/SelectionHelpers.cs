using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using UniversalImageScaler.Image;

namespace UniversalImageScaler.Utility
{
    internal static class SelectionHelpers
    {
        public static VSITEMSELECTION GetSelectedSourceImage(this IVsMonitorSelection monitorSelection)
        {
            VSITEMSELECTION sel = new VSITEMSELECTION();

            if (monitorSelection != null)
            {
                IntPtr hierarchyPtr = IntPtr.Zero;
                IntPtr selectionContainerPtr = IntPtr.Zero;

                try
                {
                    IVsMultiItemSelect multiSelect;
                    if (monitorSelection.GetCurrentSelection(out hierarchyPtr, out sel.itemid, out multiSelect, out selectionContainerPtr) >= 0)
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
                if (sel.pHier.GetProperty(sel.itemid, (int)__VSHPROPID.VSHPROPID_Name, out nameObj) >= 0)
                {
                    isImage = ImageHelpers.IsSourceImageFile(nameObj as string);
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
