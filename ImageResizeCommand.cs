using System;
using System.ComponentModel.Design;
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
                OLECMDF status = OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED;
                return (int)status;
            }
        }
    }
}
