using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Engine.ReferenceData {
	public class MutableReferenceSet : KeyedCollection<string, ReferenceLabel>, IReferenceSet {
		public ReferenceLabel GetOrAdd(string key) {
			ReferenceLabel retVal;

			if (Dictionary == null) {
				retVal = this.FirstOrDefault(rl => rl.Label == key);
				if (retVal == null)
					Add(retVal = new ReferenceLabel(key));
			} else {
				if (!Dictionary.TryGetValue(key, out retVal))
					Add(retVal = new ReferenceLabel(key));
			}
			return retVal;
		}

		protected override string GetKeyForItem(ReferenceLabel item) {
			return item.Label;
		}
	}
}
