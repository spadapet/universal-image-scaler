using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UniversalImageScaler.Models;

namespace UniversalImageScaler.Utility
{
    internal static class OutputHelpers
    {
        public static BitmapSource CreateDesignTimeSourceImage()
        {
            byte[] bytes = new byte[32 * 32 * 4];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 255;
            }

            WriteableBitmap image = new WriteableBitmap(32, 32, 96, 96, PixelFormats.Bgra32, null);
            image.WritePixels(new Int32Rect(0, 0, 32, 32), bytes, 32 * 4, 0);
            image.Freeze();
            return image;
        }

        public static void PopulateDesignTimeFeatures(SourceImage image)
        {
            OutputFeature feature = new OutputFeature("Test feature");
            image.AddFeature(feature);

            OutputSet set = new OutputSet(this, "Test image", 8, 8, true);
            feature.AddSet(set);

            OutputImage output = new OutputImageScale(set, 200);
            set.AddImage(output);

            output = new OutputImageScale(set, 100);
            set.AddImage(output);

            output = new OutputImageTargetSize(set, 16, true);
            set.AddImage(output);

            output = new OutputImageTargetSize(set, 16, false);
            set.AddImage(output);

            // Another feature for fun
            feature = new OutputFeature("Another test feature");
            feature.AddSet(set);
            image.AddFeature(feature);
        }

        public static void PopulateFeatures(SourceImage image)
        {
            OutputFeature scaleFeature = OutputHelpers.InitScaleFeature();
            OutputFeature squareFeature = OutputHelpers.InitSquareManifestFeature();
            OutputFeature wideFeature = OutputHelpers.InitWideManifestFeature();

            OutputFeature bothFeature = new OutputFeature("Square and Wide Manifest Images");
            foreach (OutputSet set in squareFeature.Sets)
            {
                bothFeature.AddSet(set);
            }

            foreach (OutputSet set in wideFeature.Sets)
            {
                bothFeature.AddSet(set);
            }

            image.AddFeature(scaleFeature);
            image.AddFeature(squareFeature);
            image.AddFeature(wideFeature);
            image.AddFeature(bothFeature);
        }

        private static OutputFeature InitScaleFeature(SourceImage image)
        {
            OutputFeature feature = new OutputFeature("Smaller Scales of Selected Image");
            string setName = Path.GetFileName(image.UnscaledPath);
            OutputSet set = null;

            if (image.Scale.HasValue)
            {
                double width = image.ImagePixelWidth / image.Scale.Value;
                double height = image.ImagePixelHeight / image.Scale.Value;
                set = new OutputSet(image, setName, width, height, true);

                // TODO: Add images
            }
            else
            {
                double width = image.ImagePixelWidth / 4.0;
                double height = image.ImagePixelHeight / 4.0;
                set = new OutputSet(image, setName, width, height, false);

                // TODO: Add images
            }

            feature.AddSet(set);
            return feature;
        }

        private static OutputFeature InitSquareManifestFeature()
        {
            OutputFeature feature = new OutputFeature("Square Manifest Images");

            return feature;

            //if (this.sourceScale.HasValue)
            //{
            //    this.images.Add(new OutputImage(this, "Square 71x71 Logo", 71, 71));
            //    this.images.Add(new OutputImage(this, "Square 150x150 Logo", 150, 150));
            //    this.images.Add(new OutputImage(this, "Wide 310x150 Logo", 310, 150));
            //    this.images.Add(new OutputImage(this, "Square 310x310 Logo", 310, 310));
            //    this.images.Add(new OutputImage(this, "Square 44x44 Logo", 44, 44, 256, 48, 24, 16));
            //    this.images.Add(new OutputImage(this, "Store Logo", 50, 50));
            //    this.images.Add(new OutputImage(this, "Badge Logo", 24, 24) { TransformType = ImageTransformType.WhiteOnly });
            //    this.images.Add(new OutputImage(this, "Splash Screen", 620, 300));
            //}
            //else
            //{
            //    this.images.Add(new OutputImage(this, this.FileName,
            //        (int)(this.sourceImage.PixelWidth / this.sourceScale.Value),
            //        (int)(this.sourceImage.PixelHeight / this.sourceScale.Value)));
            //}

        }

        private static OutputFeature InitWideManifestFeature()
        {
            OutputFeature feature = new OutputFeature("Wide Manifest Images");

            return feature;
        }
    }
}
