using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace UniversalImageScaler.Utility
{
    internal static class DteHelpers
    {
        public static EnvDTE.ProjectItem FindProjectItem(this IServiceProvider serviceProvider, string file)
        {
            EnvDTE.DTE dte = serviceProvider.GetService(typeof(SDTE)) as EnvDTE.DTE;
            return dte.Solution.FindProjectItem(file);
        }

        public static EnvDTE.Project GetProject(this IVsHierarchy hierarchy)
        {
            object extObj;
            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObj);
            return extObj as EnvDTE.Project;
        }

        public static EnvDTE.ProjectItem GetProjectItem(this IVsHierarchy hierarchy, uint itemId)
        {
            object itemObj;
            if (itemId != 0 && hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out itemObj) == 0)
            {
                return itemObj as EnvDTE.ProjectItem;
            }

            return null;
        }

        public static EnvDTE.Property GetProperty(this EnvDTE.ProjectItem item, string name)
        {
            try
            {
                return item.Properties.Item(name);
            }
            catch
            {
                return null;
            }
        }

        public static string GetFileName(this IVsHierarchy hierarchy, uint itemId)
        {
            string path;
            if (hierarchy.GetCanonicalName(itemId, out path) == 0)
            {
                return path;
            }

            return string.Empty;
        }

        public static uint FindItemId(this IVsHierarchy hierarchy, string file)
        {
            int found;
            uint foundItemId;
            VSDOCUMENTPRIORITY[] pri = new VSDOCUMENTPRIORITY[1];
            IVsProject project = hierarchy as IVsProject;

            if (project != null &&
                project.IsDocumentInProject(file, out found, pri, out foundItemId) == 0 &&
                found != 0 &&
                foundItemId != 0)
            {
                return foundItemId;
            }

            return 0;
        }

        public static void RemoveItemId(this IVsHierarchy hierarchy, uint itemId)
        {
            if (itemId != 0)
            {
                IVsProject2 project = hierarchy as IVsProject2;

                int result;
                int hr = project.RemoveItem(0, itemId, out result);
                Debug.Assert(hr == 0 && result != 0);
            }
        }

        public static uint FindParentId(this IVsHierarchy hierarchy, uint itemId)
        {
            object parentObj;
            if (itemId != 0 && hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_Parent, out parentObj) == 0)
            {
                uint parentId = DteHelpers.GetItemId(parentObj);
                if (parentId != VSConstants.VSITEMID_NIL)
                {
                    return parentId;
                }
            }

            return 0;
        }

        public static uint AddItem(this IVsHierarchy hierarchy, uint parentId, string file)
        {
            IVsProject project = hierarchy as IVsProject;
            if (project != null && parentId != 0)
            {
                uint itemId = hierarchy.FindItemId(file);
                if (itemId != 0)
                {
                    return itemId;
                }

                string[] files = new string[] { file };
                VSADDRESULT[] result = new VSADDRESULT[1];
                if (project.AddItem(parentId, VSADDITEMOPERATION.VSADDITEMOP_OPENFILE, file, 1, files, IntPtr.Zero, result) == 0 &&
                    result[0] == VSADDRESULT.ADDRESULT_Success)
                {
                    return hierarchy.FindItemId(file);
                }
            }

            return 0;
        }

        private static uint GetItemId(object obj)
        {
            uint id = 0;

            if (obj is int)
            {
                id = (uint)(int)obj;
            }
            else if (obj is uint)
            {
                id = (uint)obj;
            }

            return id;
        }
    }
}
