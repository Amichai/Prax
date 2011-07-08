﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Prax.OcrEngine.Engine.ReferenceData {
	public static class StorageExtensions {
		private static bool IsEOF(this BinaryReader reader) { return reader.PeekChar() < 0; }

		///<summary>Writes a complete set of reference data to a folder, replacing any existing data.</summary>
		public static void WriteTo(this IReferenceSet data, string folder) {
			#region Samples
			using (var writer = new BinaryWriter(File.Create(Path.Combine(folder, "Samples.dat")))) {
				if (data == null) throw new ArgumentNullException("data");
				if (!Directory.Exists(folder)) throw new DirectoryNotFoundException(folder + " does not exist");
				if (data.Count == 0 || data[0].Samples.Count == 0)
					throw new ArgumentException("Data must not be empty", "data");

				writer.Write(data.Count);	//Label count
				writer.Write(data[0].Samples[0].Heuristics.Count);	//Heuristic count
				foreach (var label in data) {
					writer.Write(label.Label);
					writer.Write(label.Samples.Count);

					foreach (var sample in label.Samples) {
						foreach (var num in sample.Heuristics) {
							writer.Write(num);
						}
					}
				}
			}
			#endregion

			#region Variances
			using (var writer = new BinaryWriter(File.Create(Path.Combine(folder, "Variances.dat")))) {
				foreach (var label in data) {
					writer.Write(label.Label);
					label.Variances.Serialize(writer);
				}
			}
			#endregion
		}

		///<summary>Reads data from the folder into an existing MutableReferenceSet.</summary>
		///<param name="set">The MutableReferenceSet to read the data into.  Any existing data in the set is preserved.</param>
		///<param name="folder">The folder containing the files generated by WriteTo().</param>
		public static void ReadFrom(this MutableReferenceSet set, string folder) {
			if (set == null) throw new ArgumentNullException("set");
			if (!Directory.Exists(folder)) throw new DirectoryNotFoundException(folder + " does not exist");

			#region Samples
			using (var reader = new BinaryReader(File.Create(Path.Combine(folder, "Samples.dat")))) {
				int labelCount = reader.ReadInt32();
				int heuristicCount = reader.ReadInt32();

				for (int i = 0; i < labelCount; i++) {
					var label = new ReferenceLabel(reader.ReadString());
					int sampleCount = reader.ReadInt32();

					for (int s = 0; s < sampleCount; s++) {
						int[] data = new int[heuristicCount];
						for (int h = 0; h < heuristicCount; h++)
							data[h] = reader.ReadInt32();

						label.Samples.Add(new LabelSample(data));
					}
				}
				if (!reader.IsEOF())
					throw new InvalidDataException("File is too big!");
			}
			#endregion

			#region Variances
			using (var reader = new BinaryReader(File.Create(Path.Combine(folder, "Variances.dat")))) {
				while (!reader.IsEOF()) {
					var label = set.GetOrAdd(reader.ReadString());
					label.Variances.Deserialize(reader);
				}
			}
			#endregion
		}
	}
}
