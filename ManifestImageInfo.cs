using System.Collections.Generic;

namespace UniversalImageScaler
{
    internal class ManifestImageInfo
    {
        private int width;
        private int height;
        private int[] targetSizes;

        public ManifestImageInfo(int width, int height, params int[] targetSizes)
        {
            this.width = width;
            this.height = height;
            this.targetSizes = targetSizes ?? new int[0];
        }

        public int Width
        {
            get
            {
                return this.width;
            }
        }

        public int Height
        {
            get
            {
                return this.height;
            }
        }

        public IEnumerable<double> Scales
        {
            get
            {
                yield return 1.0;
                yield return 1.25;
                yield return 1.5;
                yield return 2.0;
                yield return 4.0;
            }
        }

        public IEnumerable<int> TargetSizes
        {
            get
            {
                return this.targetSizes;
            }
        }

        public int GetScaledWidth(double scale)
        {
            return (int)(this.width * scale);
        }

        public int GetScaledHeight(double scale)
        {
            return (int)(this.height * scale);
        }

        public bool IsSquare
        {
            get
            {
                return this.width == this.height;
            }
        }

        public bool IsWide
        {
            get
            {
                return this.width != this.height;
            }
        }
    }
}
