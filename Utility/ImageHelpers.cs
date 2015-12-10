using System.IO;

namespace UniversalImageScaler.Utility
{
    internal static class ImageHelpers
    {
        public static bool IsSourceImageName(string name)
        {
            bool isImage = false;

            if (!string.IsNullOrEmpty(name))
            {
                isImage = name.EndsWith(".png");
            }

            return isImage;

        }
        public static bool IsSourceImageFile(string path)
        {
            bool isImage = false;

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                isImage = ImageHelpers.IsSourceImageName(Path.GetFileName(path));
            }

            return isImage;
        }
    }
}
