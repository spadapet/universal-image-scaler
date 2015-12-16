namespace UniversalImageScaler.Models
{
    internal abstract class OutputImage : ModelBase
    {
        private OutputSet owner;
        private bool? generate;

        public OutputImage(OutputSet owner)
        {
            this.owner = owner;
        }

        public abstract double PixelWidth { get; }
        public abstract double PixelHeight { get; }
        public abstract string Path { get; }

        public bool? Generate
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

        protected OutputSet Owner
        {
            get { return this.owner; }
        }
    }
}
