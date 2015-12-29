using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UniversalImageScaler.Models;

namespace UniversalImageScaler.Utility
{
    internal static class TelemetryHelpers
    {
        public static void TrackDialogOpen(SourceImage source)
        {
            if (ImageResizePackage.Instance.TelemetryClient == null)
            {
                return;
            }

            Dictionary<string, string> eventProps = new Dictionary<string, string>()
            {
                [nameof(source.Image.FileType)] = source.Image.FileType.ToString(),
                [nameof(source.CustomPixelHeight)] = source.CustomPixelHeight.ToString(),
                [nameof(source.CustomPixelWidth)] = source.CustomPixelWidth.ToString(),
                [nameof(source.FrameHasPixelSize)] = source.FrameHasPixelSize.ToString(),
                [nameof(source.FramePixelWidth)] = source.FramePixelWidth.ToString(),
                [nameof(source.FramePixelHeight)] = source.FramePixelHeight.ToString(),
                [nameof(source.HasCustomSize)] = source.HasCustomSize.ToString(),
                [nameof(source.Scale)] = source.Scale.ToString(),
                [nameof(source.ScaleReadOnly)] = source.ScaleReadOnly.ToString(),
            };

            ImageResizePackage.Instance.TelemetryClient.TrackEvent("ShowImageResizeDialog", eventProps);
        }

        public static void TrackDialogOk(SourceImage source)
        {
            if (ImageResizePackage.Instance.TelemetryClient == null)
            {
                return;
            }

            Dictionary<string, string> eventProps = new Dictionary<string, string>()
            {
                [nameof(source.Image.FileType)] = source.Image.FileType.ToString(),
                [nameof(source.Feature)] = source.Feature.Name,
                ["SetCount"] = source.SetsToGenerate.Count().ToString(),
                ["ImageCount"] = source.ImagesToGenerate.Count().ToString(),
                ["OptionalScales10"] = source.ShowOptionalScales10.ToString(),
            };

            ImageResizePackage.Instance.TelemetryClient.TrackEvent("ImageResizeDialogOk", eventProps);
        }

        public static void TrackDialogCancel(SourceImage source)
        {
            if (ImageResizePackage.Instance.TelemetryClient == null)
            {
                return;
            }

            Dictionary<string, string> eventProps = new Dictionary<string, string>()
            {
                [nameof(source.Image.FileType)] = source.Image.FileType.ToString(),
                [nameof(source.Feature)] = source.Feature.Name,
                ["SetCount"] = source.SetsToGenerate.Count().ToString(),
                ["ImageCount"] = source.ImagesToGenerate.Count().ToString(),
                ["OptionalScales10"] = source.ShowOptionalScales10.ToString(),
            };

            ImageResizePackage.Instance.TelemetryClient.TrackEvent("ImageResizeDialogCancel", eventProps);
        }

        public static void TrackGenerateSuccess(SourceImage source)
        {
            if (ImageResizePackage.Instance.TelemetryClient == null)
            {
                return;
            }

            Dictionary<string, string> eventProps = new Dictionary<string, string>()
            {
                [nameof(source.Image.FileType)] = source.Image.FileType.ToString(),
                [nameof(source.Feature)] = source.Feature.Name,
                ["SetCount"] = source.SetsToGenerate.Count().ToString(),
                ["ImageCount"] = source.ImagesToGenerate.Count().ToString(),
                ["OptionalScales10"] = source.ShowOptionalScales10.ToString(),
            };

            ImageResizePackage.Instance.TelemetryClient.TrackEvent("ImageResizeDialogSuccess", eventProps);
        }

        public static void TrackException(Exception exception)
        {
            Debug.Fail(exception.Message);
            ImageResizePackage.Instance.TelemetryClient.TrackException(exception);
        }

        [Conditional("DEBUG")]
        public static void Flush()
        {
            if (ImageResizePackage.Instance.TelemetryClient != null)
            {
                ImageResizePackage.Instance.TelemetryClient.Flush();
            }
        }
    }
}
