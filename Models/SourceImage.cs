using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Shell.Interop;
using UniversalImageScaler.Image;
using UniversalImageScaler.Utility;

namespace UniversalImageScaler.Models
{
    public class SourceImage : ModelBase, IDisposable
    {
        private VSITEMSELECTION item;

        private string path;
        private int pathScaleStart;
        private int pathScaleLength;
        private BitmapSource image;
        private ImageFileType imageType;
        private ObservableCollection<OutputFeature> features;
        private OutputFeature feature;
        private double scale;
        private bool scaleReadOnly;
        private bool showOptionalScales8;
        private bool showOptionalScales10;

        public SourceImage()
        {
            if (!WpfHelpers.IsDesignMode)
            {
                throw new InvalidOperationException("Must create SourceImage with a selected image");
            }

            this.path = @"x:\test\image.scale-400.png";
            this.pathScaleStart = 13;
            this.pathScaleLength = 11;
            this.image = OutputHelpers.CreateDesignTimeSourceImage();
            this.imageType = ImageFileType.Png;
            this.scale = 4;
            this.features = new ObservableCollection<OutputFeature>();

            OutputHelpers.PopulateDesignTimeFeatures(this);
            this.PostInitialize();
        }

        public SourceImage(VSITEMSELECTION item)
        {
            this.item = item;
            this.features = new ObservableCollection<OutputFeature>();

            InitSourcePathAndScale();
            InitSourceImage();

            OutputHelpers.PopulateFeatures(this);
            this.PostInitialize();
        }

        public void PostInitialize()
        {
            foreach (OutputFeature feature in this.features)
            {
                feature.Initialize();
            }

            if (this.Feature == null && this.features.Count > 0)
            {
                this.Feature = this.features[0];
            }
        }

        public void Dispose()
        {
        }

        private void InitSourcePathAndScale()
        {
            object nameObj;
            if (this.item.pHier.GetCanonicalName(this.item.itemid, out this.path) < 0 ||
                this.item.pHier.GetProperty(this.item.itemid, (int)__VSHPROPID.VSHPROPID_Name, out nameObj) < 0 ||
                !(nameObj is string))
            {
                throw new ArgumentException("item must be an image");
            }

            // Replace the lower-case name with the original case
            this.path = Path.Combine(Path.GetDirectoryName(this.path), (string)nameObj);

            Match match = Regex.Match(this.path, @"\.scale-(?<scale>\d{3})\.");
            if (match != null && match.Success && match.Index >= this.path.Length - Path.GetFileName(this.path).Length)
            {
                this.pathScaleStart = match.Index;
                this.pathScaleLength = match.Length;

                int scale;
                if (int.TryParse(match.Groups["scale"].Value, out scale) && scale > 0)
                {
                    this.scale = scale / 100.0;
                    this.scaleReadOnly = true;
                }
            }
            else
            {
                string extension = Path.GetExtension(this.path);
                this.pathScaleStart = this.path.Length - extension.Length;
                this.pathScaleLength = 0;

                // Just guess for now, the user can change it
                this.scale = 2;
                this.scaleReadOnly = false;
            }
        }

        private void InitSourceImage()
        {
            this.imageType = ImageHelpers.GetFileType(this.path);
            this.image = ImageHelpers.LoadSourceImage(this.path);
        }

        public VSITEMSELECTION Item
        {
            get { return this.item; }
        }

        public IEnumerable<OutputFeature> Features
        {
            get { return this.features; }
        }

        public OutputFeature Feature
        {
            get { return this.feature; }
            set
            {
                if (this.feature != value)
                {
                    this.feature = value;
                    this.OnPropertyChanged(nameof(this.Feature));
                }
            }
        }

        public IEnumerable<OutputSet> SetsToGenerate
        {
            get
            {
                if (this.Feature != null)
                {
                    foreach (OutputSet set in this.Feature.Sets)
                    {
                        bool hasImage = false;

                        foreach (OutputImage image in set.Images)
                        {
                            if (image.Enabled && image.Generate)
                            {
                                if (!this.FullPath.Equals(image.Path, StringComparison.OrdinalIgnoreCase))
                                {
                                    hasImage = true;
                                    break;
                                }
                            }
                        }

                        if (hasImage)
                        {
                            yield return set;
                        }
                    }
                }
            }
        }

        public IEnumerable<OutputImage> ImagesToGenerate
        {
            get
            {
                if (this.Feature != null)
                {
                    foreach (OutputSet set in this.Feature.Sets)
                    {
                        foreach (OutputImage image in set.Images)
                        {
                            if (image.Enabled && image.Generate)
                            {
                                if (!this.FullPath.Equals(image.Path, StringComparison.OrdinalIgnoreCase))
                                {
                                    yield return image;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AddFeature(OutputFeature feature)
        {
            if (feature != null && !this.features.Contains(feature))
            {
                this.features.Add(feature);
            }
        }

        public ImageFileType ImageType
        {
            get { return this.imageType; }
        }

        public BitmapSource Image
        {
            get { return this.image; }
        }

        public double Scale
        {
            get { return this.scale; }
            set
            {
                if (!this.scaleReadOnly && this.scale != value)
                {
                    this.scale = value;
                    this.OnPropertyChanged(nameof(this.Scale));
                    this.OnPropertyChanged(nameof(this.Scale100));
                }
            }
        }

        public double Scale100
        {
            get { return this.scale * 100.0; }
            set
            {
                if (!this.scaleReadOnly && this.Scale100 != value)
                {
                    this.scale = value / 100.0;
                    this.OnPropertyChanged(nameof(this.Scale));
                    this.OnPropertyChanged(nameof(this.Scale100));
                }
            }
        }

        public bool ScaleReadOnly
        {
            get { return this.scaleReadOnly; }
        }

        public double PixelWidth
        {
            get { return this.image != null ? this.image.PixelWidth : 0; }
        }

        public double PixelHeight
        {
            get { return this.image != null ? this.image.PixelHeight : 0; }
        }

        public string FileName
        {
            get { return Path.GetFileName(this.path); }
        }

        public string UnscaledPath
        {
            get { return this.GetScaledPath(0); }
        }

        public string GetScaledPath(double scale, string fileNameOverride = null)
        {
            string path;
            int insertPos = -1;

            if (string.IsNullOrEmpty(fileNameOverride))
            {
                path = this.pathScaleLength > 0
                    ? this.path.Remove(this.pathScaleStart, this.pathScaleLength - 1)
                    : this.path;

                insertPos = this.pathScaleStart;
            }
            else
            {
                path = Path.Combine(this.FullDir, fileNameOverride);
                insertPos = path.Length - Path.GetExtension(path).Length;
            }

            if (insertPos != -1 && scale > 0)
            {
                path = path.Insert(insertPos, $".scale-{(int)(scale * 100.0)}");
            }

            return path;
        }

        public string GetTargetSizePath(double targetSize, bool unplated, string fileNameOverride = null)
        {
            string path;
            int insertPos = -1;

            if (string.IsNullOrEmpty(fileNameOverride))
            {
                path = this.pathScaleLength > 0
                    ? this.path.Remove(this.pathScaleStart, this.pathScaleLength - 1)
                    : this.path;

                insertPos = this.pathScaleStart;
            }
            else
            {
                path = Path.Combine(this.FullDir, fileNameOverride);
                insertPos = path.Length - Path.GetExtension(path).Length;
            }

            if (insertPos != -1 && targetSize > 0)
            {
                path = unplated
                    ? path.Insert(insertPos, $".targetsize-{(int)targetSize}_altform-unplated")
                    : path.Insert(insertPos, $".targetsize-{(int)targetSize}");
            }

            return path;
        }

        public string FullDir
        {
            get { return Path.GetDirectoryName(this.path); }
        }

        public string FullPath
        {
            get { return this.path; }
        }

        public bool ShowOptionalScales8
        {
            get { return this.showOptionalScales8; }
            set
            {
                if (this.showOptionalScales8 != value)
                {
                    this.showOptionalScales8 = value;
                    this.OnPropertyChanged(nameof(this.ShowOptionalScales8));
                }
            }
        }

        public bool ShowOptionalScales10
        {
            get { return this.showOptionalScales10; }
            set
            {
                if (this.showOptionalScales10 != value)
                {
                    this.showOptionalScales10 = value;
                    this.OnPropertyChanged(nameof(this.ShowOptionalScales10));
                }
            }
        }
    }
}
