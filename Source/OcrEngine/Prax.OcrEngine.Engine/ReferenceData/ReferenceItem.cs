using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Prax.OcrEngine.Engine.ReferenceData {
	///<summary>Stores a single labeled item from reference data.</summary>
	[ImmutableObject(true)]
	public sealed class ReferenceItem {
		///<summary>The label describing this item.</summary>
		public string Label { get; private set; }
		///<summary>Data corresponding to the item.</summary>
		public ReadOnlyCollection<int> Data { get; private set; }
    }
}
