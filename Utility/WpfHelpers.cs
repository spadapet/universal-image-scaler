using System.ComponentModel;
using System.Windows;

namespace UniversalImageScaler.Utility
{
    internal static class WpfHelpers
    {
        private static readonly DependencyObject dependencyObject = new DependencyObject();

        public static bool IsDesignMode
        {
            get { return DesignerProperties.GetIsInDesignMode(WpfHelpers.dependencyObject); }
        }
    }
}
