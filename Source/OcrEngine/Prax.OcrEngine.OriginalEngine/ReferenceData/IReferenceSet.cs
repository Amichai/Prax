using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;

namespace Prax.OcrEngine.Engine.ReferenceData {
	///<summary>Stores reference data for a set of labels.</summary>
	public interface IReferenceSet : IEnumerable<ReferenceLabel> {
		///<summary>Gets the number of labels in the set.</summary>
		int Count { get; }
		///<summary>Gets an existing label by index.</summary>
		ReferenceLabel this[int index] { get; }

		//It looks like we don't need string indexing, so this
		//is essentially a ReadOnlyCollection.  However, we do
		//need string indexing to add samples, so the concrete
		//implementation is a KeyedCollection.
	}

	public class ReferenceLabel {
		public ReferenceLabel(string label) {
			Label = label;
			Samples = new Collection<LabelSample>();
			Variances = new RollingVariance();
		}

		public string Label { get; private set; }
		public Collection<LabelSample> Samples { get; private set; }
		public RollingVariance Variances { get; private set; }
	}
	public class LabelSample {
		public LabelSample(IList<int> heuristics) {
			Heuristics = new ReadOnlyCollection<int>(heuristics);
		}

		public ReadOnlyCollection<int> Heuristics { get; private set; }
	}

	public class RollingVariance {
		public void Serialize(BinaryWriter writer) {
			writer.Write(Count);
			writer.Write(mean);
			writer.Write(M2);
		}
		public void Deserialize(BinaryReader reader) {
			Count = reader.ReadInt64();
			mean = reader.ReadDouble();
			M2 = reader.ReadDouble();
		}

		public long Count { get; private set; }
		double mean = 0,
				M2 = 0;

		public double Append(double x) {
			Count++;
			double delta = x - mean;
			mean = mean + delta / Count;
			M2 = M2 + delta * (x - mean);

			double variance = M2 / (Count - 1);
			return variance;
		}
	}
}
