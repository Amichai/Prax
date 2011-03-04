using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Prax.OcrEngine.Engine.ReferenceData {
	///<summary>Stores a single labeled item from reference data.</summary>
	///<remarks>These objects are very large (especially in aggregate) and should be avoided wherever possible.</remarks>
	[ImmutableObject(true)]
	public sealed class ReferenceItem {
		public ReferenceItem(string label, IList<int> data) {
			if (label == null) throw new ArgumentNullException("label");
			if (data == null) throw new ArgumentNullException("data");

			Label = label;
			Data = new ReadOnlyCollection<int>(data);
		}

		///<summary>The label describing this item.</summary>
		public string Label { get; private set; }
		///<summary>Data corresponding to the item.</summary>
		public ReadOnlyCollection<int> Data { get; private set; }
	}
}
