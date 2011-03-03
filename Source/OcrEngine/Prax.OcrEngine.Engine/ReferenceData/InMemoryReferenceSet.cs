using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Engine.ReferenceData {
	///<summary>An IReferenceSet implementation that stores the entire set in memory.</summary>
	public class InMemoryReferenceSet : IReferenceSet {
		readonly ILookup<string, ReferenceItem> lookup;
		///<summary>Creates an InMemoryReferenceSet that contains a set of items.</summary>
		public InMemoryReferenceSet(IEnumerable<ReferenceItem> items) {
			lookup = items.ToLookup(i => i.Label);
			Labels = new ReadOnlyCollection<string>(lookup.Select(g => g.Key).ToList());
		}

		public int HeuristicCount { get { return lookup.First().First().Data.Count; } }

		public ReadOnlyCollection<string> Labels { get; private set; }

		public IEnumerable<IGrouping<string, ReferenceItem>> GetAllItems() {
			return lookup;
		}
	}
}
