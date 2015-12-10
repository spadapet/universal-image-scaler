using System;
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
    }
}
