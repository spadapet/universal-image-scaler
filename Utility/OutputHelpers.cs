using System.Collections.Generic;
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
            OutputFeature feature = new OutputFeature("Test feature") { AllowChangeScale = true };
            image.AddFeature(feature);

            OutputSet set = new OutputSet(image, "Test image", 8, 8);
            feature.AddSet(set);

            OutputImage output = new OutputImageScale(set, 2);
            set.AddImage(output);

            output = new OutputImageScale(set, 1);
            set.AddImage(output);

            output = new OutputImageTargetSize(set, 16, true);
            set.AddImage(output);

            output = new OutputImageTargetSize(set, 16, false);
            set.AddImage(output);

            set = new OutputSet(image, "Test image 2", 16, 16);
            feature.AddSet(set);

            // Another feature for fun
            feature = new OutputFeature("Another test feature");
            feature.AddSet(set);
            image.AddFeature(feature);
        }

        public static void PopulateFeatures(SourceImage source)
        {
            OutputFeature scaleFeature = OutputHelpers.InitScaleFeature(source);
            OutputFeature squareFeature = OutputHelpers.InitSquareManifestFeature(source);
            OutputFeature wideFeature = OutputHelpers.InitWideManifestFeature(source);
            OutputFeature bothFeature = null;

            if (squareFeature != null && wideFeature != null)
            {
                bothFeature = new OutputFeature("Square and wide manifest images (for app store)")
                {
                    Tooltip = "Recommended source image size: 1240px x 1240px",
                };

                foreach (OutputSet set in squareFeature.Sets)
                {
                    bothFeature.AddSet(set);
                }

                foreach (OutputSet set in wideFeature.Sets)
                {
                    bothFeature.AddSet(set);
                }
            }

            source.AddFeature(scaleFeature);
            source.AddFeature(squareFeature);
            source.AddFeature(wideFeature);
            source.AddFeature(bothFeature);
        }

        private static OutputFeature InitScaleFeature(SourceImage source)
        {
            OutputFeature feature = new OutputFeature("Smaller scales of the source image")
            {
                Tooltip = "The scale of the source image is determined by '.scale-XXX.' in the filename. Otherwise you can set it above.",
            };

            string setName = Path.GetFileName(source.UnscaledPath);
            OutputSet set = new OutputSet(source, setName);

            foreach (OutputImage image in OutputHelpers.CreateOutputImages(set, false))
            {
                set.AddImage(image);
            }

            feature.AddSet(set);
            return feature;
        }

        private static OutputFeature InitSquareManifestFeature(SourceImage source)
        {
            OutputFeature feature = new OutputFeature("Square manifest images (for app store)")
            {
                Tooltip = "Recommended source image size: 1240px x 1240px",
            };

            OutputSet[] sets = new OutputSet[]
            {
                new OutputSet(source, "Square 71x71 logo (small app tile)", 71, 71)
                {
                    Tooltip = "This is used for your small app tile on the start screen",
                    OutputFileType = ImageFileType.Png,
                },

                new OutputSet(source, "Square 150x150 logo (medium app tile)", 150, 150)
                {
                    Tooltip = "This is used for your medium app tile on the start screen",
                    OutputFileType = ImageFileType.Png,
                },

                new OutputSet(source, "Square 310x310 logo (large app tile)", 310, 310)
                {
                    Tooltip = "This is used for your large app tile on the start screen",
                    OutputFileType = ImageFileType.Png,
                },

                new OutputSet(source, "Square 44x44 logo (app list icon)", 44, 44)
                {
                    Tooltip = "This is used for your app on the task bar",
                    OutputFileType = ImageFileType.Png,
                },

                new OutputSet(source, "Store logo", 50, 50)
                {
                    Tooltip = "This will be your app's icon in the store",
                    OutputFileType = ImageFileType.Png,
                },

                new OutputSet(source, "Badge logo (white only)", 24, 24)
                {
                    Tooltip = "This will show on the lock screen when your app has notifications",
                    TransformType = ImageTransformType.WhiteOnly,
                    OutputFileType = ImageFileType.Png,
                },
            };

            foreach (OutputSet set in sets)
            {
                foreach (OutputImage image in OutputHelpers.CreateOutputImages(set, true))
                {
                    set.AddImage(image);
                }

                feature.AddSet(set);
            }

            return feature;
        }

        private static OutputFeature InitWideManifestFeature(SourceImage source)
        {
            OutputFeature feature = new OutputFeature("Wide manifest images (for app store)")
            {
                Tooltip = "Recommended source image size: 2480px x 1200px",
            };

            OutputSet[] sets = new OutputSet[]
            {
                new OutputSet(source, "Wide 310x150 Logo", 310, 150)
                {
                    Tooltip = "This is used for your wide app tile on the start screen",
                    OutputFileType = ImageFileType.Png,
                },

                new OutputSet(source, "Splash Screen", 620, 300)
                {
                    Tooltip = "While your app is loading, Windows will show this image on the splash screen",
                    OutputFileType = ImageFileType.Png,
                },
            };

            foreach (OutputSet set in sets)
            {
                foreach (OutputImage image in OutputHelpers.CreateOutputImages(set, true))
                {
                    set.AddImage(image);
                }

                feature.AddSet(set);
            }

            return feature;
        }

        private static IEnumerable<OutputImage> CreateOutputImages(OutputSet set, bool manifestImage)
        {
            yield return new OutputImageScale(set, 4);
            yield return new OutputImageScale(set, 2);
            yield return new OutputImageScaleOptional8(set, 1.8);
            yield return new OutputImageScaleOptional10(set, 1.5);
            yield return new OutputImageScaleOptional8(set, 1.4);
            yield return new OutputImageScaleOptional10(set, 1.25);
            yield return new OutputImageScale(set, 1);

            if (manifestImage && set.Width == 44 && set.Height == 44)
            {
                foreach (bool unplated in new bool[] { false, true })
                {
                    yield return new OutputImageTargetSize(set, 256, unplated);
                    yield return new OutputImageTargetSizeOptional10(set, 96, unplated);
                    yield return new OutputImageTargetSizeOptional10(set, 80, unplated);
                    yield return new OutputImageTargetSizeOptional10(set, 72, unplated);
                    yield return new OutputImageTargetSizeOptional10(set, 64, unplated);
                    yield return new OutputImageTargetSizeOptional10(set, 60, unplated);
                    yield return new OutputImageTargetSize(set, 48, unplated);
                    yield return new OutputImageTargetSizeOptional10(set, 40, unplated);
                    yield return new OutputImageTargetSizeOptional10(set, 36, unplated);
                    yield return new OutputImageTargetSize(set, 32, unplated);
                    yield return new OutputImageTargetSizeOptional10(set, 30, unplated);
                    yield return new OutputImageTargetSize(set, 24, unplated);
                    yield return new OutputImageTargetSizeOptional10(set, 20, unplated);
                    yield return new OutputImageTargetSize(set, 16, unplated);
                }
            }
        }
    }
}
