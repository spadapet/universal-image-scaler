using System;
using System.Diagnostics;
using UniversalImageScaler.Models;

namespace UniversalImageScaler.Utility
{
    internal static class TelemetryHelpers
    {
        public static void TrackDialogOpen(SourceImage source)
        {
        }

        public static void TrackDialogOk(SourceImage source)
        {
        }

        public static void TrackDialogCancel(SourceImage source)
        {
        }

        public static void TrackGenerateSuccess(SourceImage source)
        {
        }

        public static void TrackException(Exception exception)
        {
            Debug.Fail(exception.Message);
        }

        [Conditional("DEBUG")]
        public static void Flush()
        {
        }
    }
}
