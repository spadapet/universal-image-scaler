using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UniversalImageScaler.Models
{
    internal class OutputFeature : ModelBase
    {
        private string name;
        private ObservableCollection<OutputSet> sets;

        public OutputFeature(string name)
        {
            this.name = name;
            this.sets = new ObservableCollection<OutputSet>();
        }

        public string Name
        {
            get { return this.name; }
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
