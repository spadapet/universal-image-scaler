using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UniversalImageScaler.Models
{
    public class OutputFeature : ModelBase
    {
        private string name;
        private string description;
        private bool allowChangeScale;
        private bool allowChangeSize;
        private bool checkFileSize;
        private ObservableCollection<OutputSet> sets;

        public OutputFeature(string name)
        {
            this.name = name;
            this.description = string.Empty;
            this.sets = new ObservableCollection<OutputSet>();
        }

        public void Initialize()
        {
            foreach (OutputSet set in this.sets)
            {
                set.Initialize();
            }
        }

        public string Name
        {
            get { return this.name; }
        }

        public string Tooltip
        {
            get { return !string.IsNullOrEmpty(this.description) ? this.description : null; }
            set
            {
                if (this.description != value)
                {
                    this.description = value;
                    this.OnPropertyChanged(nameof(this.Tooltip));
                }
            }
        }

        public bool AllowChangeScale
        {
            get { return this.allowChangeScale; }
            set
            {
                if (this.allowChangeScale != value)
                {
                    this.allowChangeScale = value;
                    this.OnPropertyChanged(nameof(this.AllowChangeScale));
                }
            }
        }

        public bool AllowChangeSize
        {
            get { return this.allowChangeSize; }
            set
            {
                if (this.allowChangeSize != value)
                {
                    this.allowChangeSize = value;
                    this.OnPropertyChanged(nameof(this.AllowChangeSize));
                }
            }
        }

        public bool CheckFileSize
        {
            get { return this.checkFileSize; }
            set
            {
                if (this.checkFileSize != value)
                {
                    this.checkFileSize = value;
                    this.OnPropertyChanged(nameof(this.CheckFileSize));
                }
            }
        }

        public IEnumerable<OutputSet> Sets
        {
            get { return this.sets; }
        }

        public void AddSet(OutputSet set)
        {
            if (set != null && !this.sets.Contains(set))
            {
                this.sets.Add(set);
            }
        }
    }
}
