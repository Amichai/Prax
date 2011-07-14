using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prax.OcrEngine.Engine.HeuristicGeneration;
using Prax.OcrEngine.Services;
using System.Collections.ObjectModel;
using System.IO;
using Prax.OcrEngine.Engine.ReferenceData;
using SLaks.Progression;
using System.Diagnostics;

namespace Prax.OcrEngine.Engine.ReferenceData {
	///<summary>Searches a set of reference data to find matching labels.</summary>
	public interface IReferenceSearcher {
		///<summary>Finds the best matching labels for a given piece of heuristics.</summary>
		IEnumerable<RecognizedSegment> PerformLookup(HeuristicSet heuristics, IProgressReporter progress = null);
		IEnumerable<RecognizedSegment> PerformWhitespaceLookup(HeuristicSet heuristics, IProgressReporter progress = null);
	}

	/// <summary>Training data is a set of heuristics return values associated with their corresponding input label.
	/// Training data is used to take an unlabeled set of heuristic return values and to compare those return values
	/// to labeled sets of heuristic return value and use that comparison to determine the most appropriate label to 
	/// associate.</summary>
	public class ReferenceSearcher : IReferenceSearcher {
		public IReferenceSet Library { get; private set; }

		public ReferenceSearcher(IReferenceSet data) { Library = data; }

		///<summary>Finds the best matching labels for a given piece of heuristics.</summary>
		public IEnumerable<RecognizedSegment> PerformLookup(HeuristicSet heuristics, IProgressReporter progress = null) {
			if (heuristics.Label != null)
				throw new ArgumentException("PerformLookup expects an unidentified segment", "heuristics");

			var results = GetMatchesIterator(heuristics, progress).OrderBy(i => i.Certainty);
			return results;
		}

		private IEnumerable<RecognizedSegment> GetMatchesIterator(HeuristicSet unlabledHeuristic, IProgressReporter progress) {
			int whitespaceIdx = int.MinValue,
				allLabelsIdx = int.MinValue;
			for (int i = 0; i < Library.Count; i++) {
				if (Library[i].Label == "whitespace") {
					whitespaceIdx = i;
				} if (Library[i].Label == "AllLabels") {
					allLabelsIdx = i;
				} if (whitespaceIdx != int.MinValue && allLabelsIdx != int.MinValue) {
					i = Library.Count;
				}
			}
			HashSet<int> dontCheck = new HashSet<int>() { whitespaceIdx, allLabelsIdx };

			progress = progress ?? new EmptyProgressReporter();

			int heuristicCount = unlabledHeuristic.Heuristics.Count;
			double[][] probabilityFromEachHeuristic = new double[Library.Count][];
			double[][] lblComparisonResults = new double[Library.Count][];
			double[] labelProbability;

			var totalSampleCount = Library.Where((_, i) => !dontCheck.Contains(i)).Sum(rl => rl.Samples.Count);

			progress.Maximum = heuristicCount * totalSampleCount + heuristicCount * (Library.Count - 2);

			//double[] totalComparison_test = new double[numberOfLabelsToCount];
			for (int i = 0; i < Library.Count; i++) {
				probabilityFromEachHeuristic[i] = new double[heuristicCount];
				lblComparisonResults[i] = new double[heuristicCount];
			}

			for (int heurIdx = 0; heurIdx < heuristicCount; heurIdx++) {
				for (int lblIdx = 0; lblIdx < Library.Count; lblIdx++) {
					while (dontCheck.Contains(lblIdx))
						lblIdx++;
					if (lblIdx == Library.Count) break;

					var current = Library[lblIdx];
					foreach (var item in current.Samples) {
						progress.Progress++;
						if (unlabledHeuristic.GetAtIndex(heurIdx) == item.Heuristics[heurIdx])
							lblComparisonResults[lblIdx][heurIdx]++;
					}
				}
				for (int labelIndex = 0; labelIndex < Library.Count; labelIndex++) {
					while (dontCheck.Contains(labelIndex)) {
						labelIndex++;
					}
					if (labelIndex != Library.Count) {
						lblComparisonResults[labelIndex][heurIdx] = lblComparisonResults[labelIndex][heurIdx] / (double)Library[labelIndex].Samples.Count;
					}
				}
			}
			Debug.Assert(progress.Progress == heuristicCount * totalSampleCount);

			//We are working to produce two DSs: lblComparisonResults[][], totalComparison_test[]
			double heuristicProbabilisticIndication;
			double multiplicativeOffset;
			labelProbability = new double[Library.Count];
			double aprioriProb = 1.0 / ((double)Library.Count - dontCheck.Count());
			double factorIncrease = (1.0 - aprioriProb) / aprioriProb;
			//factorIncrease+=10;
			for (int inspectionLbl = 0; inspectionLbl < Library.Count; inspectionLbl++) {
				while (dontCheck.Contains(inspectionLbl))
					inspectionLbl++;

				if (inspectionLbl == Library.Count) break;
				labelProbability[inspectionLbl] = 1.0 / ((double)Library.Count - dontCheck.Count());
				for (int heurIdx = 0; heurIdx < heuristicCount; heurIdx++) {
					progress.Progress++;

					double comparisonToThisLabel = lblComparisonResults[inspectionLbl][heurIdx];
					double comparisonToOtherLabels = lblComparisonResults.Sum(h => h[heurIdx]) - comparisonToThisLabel;

					if (comparisonToThisLabel + comparisonToOtherLabels != 0) {
						heuristicProbabilisticIndication = comparisonToThisLabel / (comparisonToThisLabel + comparisonToOtherLabels);
						heuristicsControl.buildHeuristicProbabilityHistorgram(heuristicProbabilisticIndication, inspectionLbl, heurIdx);
						multiplicativeOffset = Library[inspectionLbl].Variances.Append(heuristicProbabilisticIndication);
						multiplicativeOffset += aprioriProb / (double)Library[inspectionLbl].Variances.Count;

						if (multiplicativeOffset < double.MaxValue)
							labelProbability[inspectionLbl] *= (factorIncrease * heuristicProbabilisticIndication + multiplicativeOffset) / (1 - heuristicProbabilisticIndication + multiplicativeOffset);

						if (double.IsInfinity(labelProbability[inspectionLbl]) || labelProbability[inspectionLbl] == 0) {
							progress.Progress += heuristicCount - heurIdx - 1;
							break;
						}
					}
				}

				if (Library[inspectionLbl].Samples.Count > 0) {
					yield return new RecognizedSegment(unlabledHeuristic.Bounds, Library[inspectionLbl].Label, labelProbability[inspectionLbl]);
				}
			}
			Debug.Assert(progress.Progress == progress.Maximum);
		}

		private static HeuristicsControlPanel heuristicsControl = new HeuristicsControlPanel();

		public IEnumerable<RecognizedSegment> PerformWhitespaceLookup(HeuristicSet unlabledHeuristic, IProgressReporter progress = null) {
			progress = progress ?? new EmptyProgressReporter();

			int heuristicCount = unlabledHeuristic.Heuristics.Count;
			Dictionary<ReferenceLabel, double[]> probabilityFromEachHeuristic = new Dictionary<ReferenceLabel, double[]>();
			Dictionary<ReferenceLabel, double[]> lblComparisonResults = new Dictionary<ReferenceLabel, double[]>();
			Dictionary<ReferenceLabel, double> labelProbability = new Dictionary<ReferenceLabel, double>();

			ReferenceLabel whitespace = null, allLabels = null;
			for (int i = 0; i < Library.Count; i++) {
				if (Library[i].Label == "whitespace")
					whitespace = Library[i];
				else if (Library[i].Label == "AllLabels")
					allLabels = Library[i];

				if (whitespace != null && allLabels != null)
					break;
			}


			var sampleCount = whitespace.Samples.Count + allLabels.Samples.Count;

			progress.Maximum = heuristicCount * sampleCount + 2 * heuristicCount;

			probabilityFromEachHeuristic[whitespace] = new double[heuristicCount];
			probabilityFromEachHeuristic[allLabels] = new double[heuristicCount];
			lblComparisonResults[whitespace] = new double[heuristicCount];
			lblComparisonResults[allLabels] = new double[heuristicCount];

			for (int heurIdx = 0; heurIdx < heuristicCount; heurIdx++) {
				foreach (var item in whitespace.Samples) {
					progress.Progress++;
					if (unlabledHeuristic.GetAtIndex(heurIdx) == item.Heuristics[heurIdx]) {
						lblComparisonResults[whitespace][heurIdx]++;
					}
				}
				foreach (var item in allLabels.Samples) {
					progress.Progress++;
					if (unlabledHeuristic.GetAtIndex(heurIdx) == item.Heuristics[heurIdx]) {
						lblComparisonResults[allLabels][heurIdx]++;
					}
				}
				lblComparisonResults[whitespace][heurIdx] = lblComparisonResults[whitespace][heurIdx] / (double)whitespace.Samples.Count;
				lblComparisonResults[allLabels][heurIdx] = lblComparisonResults[allLabels][heurIdx] / (double)allLabels.Samples.Count;
			}
			Debug.Assert(progress.Progress == heuristicCount * sampleCount);

			double heuristicProbabilisticIndication;
			double multiplicativeOffset;
			double aprioriProb = 1.0 / (double)2;
			double factorIncrease = (1.0 - aprioriProb) / aprioriProb;

			foreach (var label in new[] { whitespace, allLabels }) {
				labelProbability[label] = 1.0 / (double)2;
				for (int heurIdx = 0; heurIdx < heuristicCount; heurIdx++) {
					progress.Progress++;

					double comparisonToThisLabel = lblComparisonResults[label][heurIdx];
					double comparisonToOtherLabels = lblComparisonResults.Sum(h => h.Value[heurIdx]) - comparisonToThisLabel;

					if (comparisonToThisLabel + comparisonToOtherLabels != 0) {
						heuristicProbabilisticIndication = comparisonToThisLabel / (comparisonToThisLabel + comparisonToOtherLabels);
						heuristicsControl.buildHeuristicProbabilityHistorgram(heuristicProbabilisticIndication, Library.IndexOf(label), heurIdx);
						multiplicativeOffset = label.Variances.Append(heuristicProbabilisticIndication);
						multiplicativeOffset += aprioriProb / (double)label.Variances.Count;

						if (multiplicativeOffset < double.MaxValue)
							labelProbability[label] *= (factorIncrease * (heuristicProbabilisticIndication + multiplicativeOffset)) / (1 - heuristicProbabilisticIndication + multiplicativeOffset);

						if (double.IsInfinity(labelProbability[label]) || labelProbability[label] == 0) {
							progress.Progress += heuristicCount - heurIdx - 1;
							break;
						}
					}
				}
				if (label.Samples.Count > 0)
					yield return new RecognizedSegment(unlabledHeuristic.Bounds, label.Label, labelProbability[label]);
			}
			Debug.Assert(progress.Progress == progress.Maximum);
		}
	}

	class HeuristicsControlPanel {
		private int[] probabilityHistorgram = new int[21];
		public void buildHeuristicProbabilityHistorgram(double probability, int labelUnderInspection, int heuristicUnderInspection) {
			if (probability == 1)
				probabilityHistorgram[20]++;
			else
				for (int i = 0; i < 20; i++)
					if (probability >= 0 * .05 && probability < (i + 1) * .05) {
						probabilityHistorgram[i]++;
						i = 20;
					}
		}

		//TODO: Improve the lookup of labels
		//Consider ignoring heuristics that are proven to be useless

		//In order to improve the lookup of labels, expose a variable for the width of the segment so it can be adjusted as words are being resolved
		//do something similar for segmentation


		//Automated training - compare existing coordinates with list of exact coordinates
	}
}
