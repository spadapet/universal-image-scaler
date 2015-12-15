using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
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

        public static ImageResizePackage Instance { get; private set; }
        public ThreadHelper MainThreadHelper { get; }

        private List<MenuCommand> menuCommands;
        private IMenuCommandService menuCommandService;

        public ImageResizePackage()
        {
            this.MainThreadHelper = ThreadHelper.Generic;
            this.menuCommands = new List<MenuCommand>();
        }

        protected override void Initialize()
        {
            ImageResizePackage.Instance = this;

            base.Initialize();

            AddMenuCommands();
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
