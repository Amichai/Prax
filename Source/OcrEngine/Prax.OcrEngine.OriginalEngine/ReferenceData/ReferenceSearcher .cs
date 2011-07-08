using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prax.OcrEngine.Engine.HeuristicGeneration;
using Prax.OcrEngine.Services;
using System.Collections.ObjectModel;
using System.IO;
using Prax.OcrEngine.Engine.ReferenceData;

namespace Prax.OcrEngine.Engine.ReferenceData {
	///<summary>Searches a set of reference data to find matching labels.</summary>
	public interface IReferenceSearcher {
		///<summary>Finds the best matching labels for a given piece of heuristics.</summary>
		IEnumerable<RecognizedSegment> PerformLookup(HeuristicSet heuristics);
	}

	/// <summary>Training data is a set of heuristics return values associated with their corresponding input label.
	/// Training data is used to take an unlabeled set of heuristic return values and to compare those return values
	/// to labeled sets of heuristic return value and use that comparison to determine the most appropriate label to 
	/// associate.</summary>
	public class ReferenceSearcher : IReferenceSearcher {
		public IReferenceSet content { get; private set; }

		public ReferenceSearcher(IReferenceSet data) { content = data; }

		///<summary>Finds the best matching labels for a given piece of heuristics.</summary>
		public IEnumerable<RecognizedSegment> PerformLookup(HeuristicSet heuristics) {
			if (heuristics.Label != null)
				throw new ArgumentException("PerformLookup expects an unidentified segment", "heuristics");

			var results = GetMatchesIterator(heuristics).OrderBy(i => i.Certainty);
			return results;
		}

		private IEnumerable<RecognizedSegment> GetMatchesIterator(HeuristicSet unlabledHeuristic) {
			int numberOfUniqueLabels = content.Count;
			int sizeOfHeuristicArray = unlabledHeuristic.Heuristics.Count;
			int numberOfLabelsToCount = numberOfUniqueLabels;
			double[][] probabilityFromEachHeuristic = new double[numberOfLabelsToCount][];
			double[][] lblComparisonResults = new double[numberOfLabelsToCount][];
			double[] labelProbability;

			//double[] totalComparison_test = new double[numberOfLabelsToCount];

			for (int i = 0; i < numberOfLabelsToCount; i++) {
				probabilityFromEachHeuristic[i] = new double[sizeOfHeuristicArray];
				lblComparisonResults[i] = new double[sizeOfHeuristicArray];
			}

			for (int heurIdx = 0; heurIdx < sizeOfHeuristicArray; heurIdx++) {
				for (int lblIdx = 0; lblIdx < numberOfUniqueLabels; lblIdx++) {
					var current = content[lblIdx];

					foreach (var item in current.Samples) {
						if (unlabledHeuristic.GetAtIndex(heurIdx) == item.Heuristics[heurIdx])
							lblComparisonResults[lblIdx][heurIdx]++;
					}
					//totalComparison_test[lblIdx] += lblComparisonResults[lblIdx][heurIdx];
				}
				for (int labelIndex = 0; labelIndex < numberOfUniqueLabels; labelIndex++) {
					lblComparisonResults[labelIndex][heurIdx] = lblComparisonResults[labelIndex][heurIdx] / (double)content[labelIndex].Samples.Count;
				}
			}
			//We are working to produce two DSs: lblComparisonResults[][], totalComparison_test[]
			double heuristicProbabilisticIndication;
			double multiplicativeOffset;
			labelProbability = new double[numberOfLabelsToCount];
			//double maxProb = 0;
			//int maxProbIndex = 0;
			double aprioriProb = 1.0 / (double)numberOfLabelsToCount;
			double factorIncrease = (1.0 - aprioriProb) / aprioriProb;

			for (int inspectionLbl = 0; inspectionLbl < numberOfUniqueLabels; inspectionLbl++) {
				labelProbability[inspectionLbl] = 1.0 / (double)numberOfLabelsToCount;
				for (int heurIdx = 0; heurIdx < sizeOfHeuristicArray; heurIdx++) {
					double comparisonToThisLabel = lblComparisonResults[inspectionLbl][heurIdx];
					double comparisonToOtherLabels = 0;
					for (int comparisonLbl = 0; comparisonLbl < numberOfUniqueLabels; comparisonLbl++) {
						if (inspectionLbl != comparisonLbl)
							comparisonToOtherLabels += lblComparisonResults[comparisonLbl][heurIdx];
					}
					if (comparisonToThisLabel + comparisonToOtherLabels != 0) {
						heuristicProbabilisticIndication = comparisonToThisLabel / (comparisonToThisLabel + comparisonToOtherLabels);
						heuristicsControl.buildHeuristicProbabilityHistorgram(heuristicProbabilisticIndication, inspectionLbl, heurIdx);
						multiplicativeOffset = content[inspectionLbl].Variances.Append(heuristicProbabilisticIndication);
						multiplicativeOffset += aprioriProb / (double)content[inspectionLbl].Variances.Count;
						if (multiplicativeOffset < double.MaxValue) {
							labelProbability[inspectionLbl] *= (factorIncrease * heuristicProbabilisticIndication + multiplicativeOffset) / (1 - heuristicProbabilisticIndication + multiplicativeOffset);
						}
						if (double.IsInfinity(labelProbability[inspectionLbl]) || labelProbability[inspectionLbl] == 0)
							heurIdx = sizeOfHeuristicArray;
					}
				}
				if (content[inspectionLbl].Samples.Count > 0) {
					yield return new RecognizedSegment(unlabledHeuristic.Bounds, content[inspectionLbl].Label, labelProbability[inspectionLbl]);
				}
			}
		}

		private static HeuristicsControlPanel heuristicsControl = new HeuristicsControlPanel();
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
