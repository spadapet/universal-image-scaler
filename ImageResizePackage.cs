using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace UniversalImageScaler
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(ImageResizePackage.PackageGuidString)]
    public sealed class ImageResizePackage : Package
    {
        public const string PackageGuidString = "9bd56ba2-ecfa-4ff8-8615-0475808ff596";
        private const string CommandSetGuidString = "d4e44266-2d61-4268-ac51-b3392512cbbf"; 
        private const int ImageResizeCommandId = 1;
#if DEBUG
        private const string AppInsightsGuidString = "b8bb3d3c-a649-4f10-87f2-dabac88e877f";
#else
        private const string AppInsightsGuidString = "4860a93a-8a6c-4af5-aa85-f744c147884e";
#endif
        public static ImageResizePackage Instance { get; private set; }
        public ThreadHelper MainThreadHelper { get; }

        private List<MenuCommand> menuCommands;
        private IMenuCommandService menuCommandService;
        private Microsoft.ApplicationInsights.TelemetryClient telemetryClient;

        public ImageResizePackage()
        {
            this.MainThreadHelper = ThreadHelper.Generic;
            this.menuCommands = new List<MenuCommand>();
        }

        public Microsoft.ApplicationInsights.TelemetryClient TelemetryClient
        {
            get { return this.telemetryClient; }
        }

        protected override void Initialize()
        {
            if (ImageResizePackage.Instance == null)
            {
                ImageResizePackage.Instance = this;
            }

            InitializeTelemetry();
            base.Initialize();
            AddMenuCommands();
        }

        private void InitializeTelemetry()
        {
            this.telemetryClient = new Microsoft.ApplicationInsights.TelemetryClient()
            {
                InstrumentationKey = ImageResizePackage.AppInsightsGuidString,
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ImageResizePackage.Instance == this)
                {
                    ImageResizePackage.Instance = null;
                }

                RemoveMenuCommands();
            }

            base.Dispose(disposing);
        }

        private void AddMenuCommands()
        {
            if (this.menuCommandService == null)
            {
                this.menuCommandService = this.GetService(typeof(IMenuCommandService)) as IMenuCommandService;

                if (this.menuCommandService != null)
                {
                    this.menuCommands.AddRange(this.CreateMenuCommands());

                    foreach (MenuCommand menuCommand in this.menuCommands)
                    {
                        this.menuCommandService.AddCommand(menuCommand);
                    }
                }
            }
        }

        private void RemoveMenuCommands()
        {
            if (this.menuCommandService != null)
            {
                foreach (MenuCommand menuCommand in this.menuCommands)
                {
                    this.menuCommandService.RemoveCommand(menuCommand);
                }

                this.menuCommands.Clear();
                this.menuCommandService = null;
            }
        }

        private IEnumerable<MenuCommand> CreateMenuCommands()
        {
            CommandID imageResizeCommandId = new CommandID(
                new Guid(ImageResizePackage.CommandSetGuidString),
                ImageResizePackage.ImageResizeCommandId);

            yield return new ImageResizeCommand(this, imageResizeCommandId);
        }
    }
}
