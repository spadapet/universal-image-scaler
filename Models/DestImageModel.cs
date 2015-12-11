using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UniversalImageScaler.Utility;

namespace UniversalImageScaler.Models
{
    internal class DestImageModel : ModelBase
    {
        private int width;
        private int height;
        private int[] targetSizes;
        private string name;
        private string extension;
        private bool enabled;
        private bool generate;
        private SourceImageModel owner;

        public DestImageModel(SourceImageModel owner, string name, int width, int height, params int[] targetSizes)
        {
            this.owner = owner;
            this.name = name;
            this.width = width;
            this.height = height;
            this.targetSizes = targetSizes ?? new int[0];
            this.enabled = true;

            this.extension = ".png";

            if (this.name.EndsWith(".png") || this.name.EndsWith(".jpg"))
            {
                Match match = Regex.Match(this.name, @".*\.(?<scale>scale-\d{3}\.)(?:png|jpg)");
                if (match != null && match.Success)
                {
                    this.name = this.name.Remove(match.Groups["scale"].Index, match.Groups["scale"].Length);
                }

                this.extension = this.name.Substring(this.name.Length - 4);
                this.name = this.name.Substring(0, this.name.Length - 4);
            }
        }

        public ImageTransformType TransformType
        {
            get; set;
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
                foreach (double scale in this.InternalScales)
                {
                    int width = this.GetScaledWidth(scale);
                    int height = this.GetScaledHeight(scale);

                    if (width <= this.owner.PixelWidth &&
                        height <= this.owner.PixelHeight)
                    {
                        yield return scale;
                    }
                }
            }
        }

        protected virtual IEnumerable<double> InternalScales
        {
            get
            {
                yield return 1.0;

                if (this.owner.IsManifestImage)
                {
                    yield return 1.25;
                    yield return 1.5;
                }

                yield return 2.0;
                yield return 4.0;
            }
        }

        public IEnumerable<int> TargetSizes
        {
            get
            {
                foreach (int targetSize in this.targetSizes)
                {
                    if (targetSize <= owner.PixelWidth &&
                        targetSize <= owner.PixelHeight)
                    {
                        yield return targetSize;
                    }
                }
            }
        }

        public int GetScaledWidth(double scale)
        {
            return (int)Math.Ceiling(this.width * scale);
        }

        public int GetScaledHeight(double scale)
        {
            return (int)Math.Ceiling(this.height * scale);
        }

        public string GetScaledFileName(double scale)
        {
            return $"{this.FileName}.scale-{(int)(scale * 100.0)}{this.extension}";
        }

        public string GetTargetSizeFileName(double targetSize)
        {
            return $"{this.FileName}.targetsize-{(int)targetSize}{this.extension}";
        }

        public string GetUnplatedTargetSizeFileName(double targetSize)
        {
            return $"{this.FileName}.targetsize-{(int)targetSize}_altform-unplated{this.extension}";
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

        public string DisplayText
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (double scale in this.Scales)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append((int)(scale * 100));
                }

                return $"{this.name}{this.extension}, Scales:{sb.ToString()}";
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public virtual string FileName
        {
            get
            {
                return this.owner.IsManifestImage ? this.name.Replace(" ", string.Empty) : this.name;
            }
        }

        public virtual string FileNameAndExtension
        {
            get
            {
                return $"{this.FileName}{this.extension}";
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
