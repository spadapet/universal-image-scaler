using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UniversalImageScaler.Models
{
    internal class OutputSets : ModelBase
    {
        private string name;
        private ObservableCollection<OutputSet> sets;

        public OutputSets(string name)
        {
            this.name = name;
            this.sets = new ObservableCollection<OutputSet>();
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.name != value)
                {
                    this.name = value ?? string.Empty;
                    this.OnPropertyChanged(nameof(this.Name));
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
