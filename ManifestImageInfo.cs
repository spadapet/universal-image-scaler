using System.Collections.Generic;

namespace UniversalImageScaler
{
    internal class ManifestImageInfo : ModelBase
    {
        private int width;
        private int height;
        private int[] targetSizes;
        private string name;
        private bool enabled;
        private bool generate;

        public ManifestImageInfo(string name, int width, int height, params int[] targetSizes)
        {
            this.name = name;
            this.width = width;
            this.height = height;
            this.targetSizes = targetSizes ?? new int[0];
            this.enabled = true;
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

        public string GetScaledFileName(double scale)
        {
            return $"{this.name}.scale-{(int)(scale * 100.0)}.png";
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

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public bool Enabled
        {
            get
            {
                return this.enabled;
            }

            set
            {
                if (this.enabled != value)
                {
                    this.enabled = value;
                    OnPropertyChanged(nameof(this.Enabled));
                }
            }
        }

        public bool Generate
        {
            get
            {
                return this.generate;
            }

            set
            {
                if (this.generate != value)
                {
                    this.generate = value;
                    OnPropertyChanged(nameof(this.Generate));
                }
            }
        }
    }
}
