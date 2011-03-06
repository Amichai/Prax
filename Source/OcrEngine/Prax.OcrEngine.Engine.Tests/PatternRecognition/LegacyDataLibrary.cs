using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Prax.OcrEngine.Engine.ReferenceData;

namespace Prax.OcrEngine.Engine.Tests.PatternRecognition {
	class LegacyDataLibrary {
		public static LegacyDataLibrary FromFile(string path) {
			using (var file = File.OpenRead(path))
				return new LegacyDataLibrary(file);
		}
		public static LegacyDataLibrary FromStream(Stream file) { return new LegacyDataLibrary(file); }

		public List<Tuple<string, List<int>>> trainingLibrary { get; private set; }
		public List<List<int>> listOfIndicies { get; private set; }
		public List<string> listOfIndexLabels { get; private set; }

		public IReferenceSet ReferenceSet { get; private set; }

		private LegacyDataLibrary(Stream file) {
			var formatter = new BinaryFormatter();

			trainingLibrary = (List<Tuple<string, List<int>>>)formatter.Deserialize(file);
			listOfIndicies = (List<List<int>>)formatter.Deserialize(file);
			listOfIndexLabels = (List<string>)formatter.Deserialize(file);


			ReferenceSet = new InMemoryReferenceSet(trainingLibrary.Select(t => new ReferenceItem(t.Item1, t.Item2)));
		}
	}
}
