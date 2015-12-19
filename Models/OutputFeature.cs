using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UniversalImageScaler.Models
{
    public class OutputFeature : ModelBase
    {
        private string name;
        private string description;
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
            get { return this.description; }
            set
            {
                if (this.description != value)
                {
                    this.description = value;
                    this.OnPropertyChanged(nameof(this.Tooltip));
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
