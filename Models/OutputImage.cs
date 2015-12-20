using System.ComponentModel;

namespace UniversalImageScaler.Models
{
    public abstract class OutputImage : ModelBase
    {
        private OutputSet owner;
        private bool generate;
        private bool enabled;

        public OutputImage(OutputSet owner)
        {
            this.owner = owner;
            this.owner.PropertyChanged += OnImagePropertyChanged;
            this.owner.Owner.PropertyChanged += OnImagePropertyChanged;
        }

        public abstract double PixelWidth { get; }
        public abstract double PixelHeight { get; }
        public abstract string Path { get; }
        public abstract string DisplayText { get; }

        public virtual void Initialize()
        {
            this.UpdateEnabled();
        }

        public virtual string Tooltip
        {
            get { return null; }
        }

        public bool Generate
        {
            get { return this.generate; }
            set
            {
                if (this.generate != value)
                {
                    this.generate = value;
                    this.OnPropertyChanged(nameof(this.Generate));
                }
            }
        }

        public bool Enabled
        {
            get { return this.enabled; }
            set
            {
                if (this.enabled != value)
                {
                    this.enabled = value;
                    this.OnPropertyChanged(nameof(this.Enabled));
                }
            }
        }

        protected OutputSet Owner
        {
            get { return this.owner; }
        }

        protected SourceImage Image
        {
            get { return this.Owner.Owner; }
        }

        protected void UpdateEnabled()
        {
            this.Enabled = this.ShouldEnable;
        }

        protected virtual bool ShouldEnable
        {
            get { return true; }
        }

        private void OnImagePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            this.UpdateEnabled();
        }
    }
}
