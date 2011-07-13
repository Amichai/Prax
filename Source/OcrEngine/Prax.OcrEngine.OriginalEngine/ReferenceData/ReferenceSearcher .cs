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

namespace Prax.OcrEngine.Engine.ReferenceData {
	///<summary>Searches a set of reference data to find matching labels.</summary>
	public interface IReferenceSearcher {
		///<summary>Finds the best matching labels for a given piece of heuristics.</summary>
		IEnumerable<RecognizedSegment> PerformLookup(HeuristicSet heuristics, IProgressReporter progress = null);
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
			progress = progress ?? new EmptyProgressReporter();

			int heuristicCount = unlabledHeuristic.Heuristics.Count;
			double[][] probabilityFromEachHeuristic = new double[Library.Count][];
			double[][] lblComparisonResults = new double[Library.Count][];
			double[] labelProbability;

			var totalSampleCount = Library.Sum(rl => rl.Samples.Count);

			progress.Maximum = heuristicCount * totalSampleCount + heuristicCount * Library.Count;

			//double[] totalComparison_test = new double[numberOfLabelsToCount];
			for (int i = 0; i < Library.Count; i++) {
				probabilityFromEachHeuristic[i] = new double[heuristicCount];
				lblComparisonResults[i] = new double[heuristicCount];
			}

			for (int heurIdx = 0; heurIdx < heuristicCount; heurIdx++) {
				for (int lblIdx = 0; lblIdx < Library.Count; lblIdx++) {
					var current = Library[lblIdx];

					foreach (var item in current.Samples) {
						progress.Progress++;
						if (unlabledHeuristic.GetAtIndex(heurIdx) == item.Heuristics[heurIdx])
							lblComparisonResults[lblIdx][heurIdx]++;
					}
					//totalComparison_test[lblIdx] += lblComparisonResults[lblIdx][heurIdx];
				}
				for (int labelIndex = 0; labelIndex < Library.Count; labelIndex++) {
					lblComparisonResults[labelIndex][heurIdx] = lblComparisonResults[labelIndex][heurIdx] / (double)Library[labelIndex].Samples.Count;
				}
			}

			//We are working to produce two DSs: lblComparisonResults[][], totalComparison_test[]
			double heuristicProbabilisticIndication;
			double multiplicativeOffset;
			labelProbability = new double[Library.Count];
			//double maxProb = 0;
			//int maxProbIndex = 0;
			double aprioriProb = 1.0 / (double)Library.Count;
			double factorIncrease = (1.0 - aprioriProb) / aprioriProb;


			for (int inspectionLbl = 0; inspectionLbl < Library.Count; inspectionLbl++) {
				labelProbability[inspectionLbl] = 1.0 / (double)Library.Count;
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

						if (double.IsInfinity(labelProbability[inspectionLbl]) || labelProbability[inspectionLbl] == 0)
							heurIdx = heuristicCount;
					}
				}
				if (Library[inspectionLbl].Samples.Count > 0) {
					yield return new RecognizedSegment(unlabledHeuristic.Bounds, Library[inspectionLbl].Label, labelProbability[inspectionLbl]);
				}
			}
		}

		private static HeuristicsControlPanel heuristicsControl = new HeuristicsControlPanel();

		internal object PerformWhitespaceLookup(HeuristicSet segment) {
			//TODO: Wrapper method. Perform regular lookup with restriction that we only care about whitespace
			//use the results to improve eliminate image content that isn't part of the letter under inspection
			throw new NotImplementedException();
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
