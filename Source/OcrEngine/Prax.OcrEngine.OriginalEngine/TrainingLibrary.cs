using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prax.OcrEngine.Engine.HeuristicGeneration;
using Prax.OcrEngine.Services;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Engine {
	public interface IReferenceLibrary {
		IEnumerable<RecognizedSegment> PerformLookup(HeuristicSet heuristics);
	}

	/// <summary>Training data is a set of heuristics return values associated with their corresponding input label.
	/// Training data is used to take an unlabeled set of heuristic return values and to compare those return values
	/// to labeled sets of heuristic return value and use that comparison to determine the most appropriate label to 
	/// associate.</summary>
	public class TrainingLibrary : IReferenceLibrary {

		/// <summary>Take an unlabeled HeursiticReturnVaules object and compare it to each key value pair in the 
		/// library and return the best match as a LookupResult</summary>
		public IEnumerable<RecognizedSegment> PerformLookup(HeuristicSet heuristics) {
			if (heuristics.Label != null)
				throw new ArgumentException("PerformLookup expects an unidentified segment", "heuristics");

			return GetMatches(heuristics).OrderBy(i => i.Certainty);
		}

		public void AddAll(IEnumerable<HeuristicSet> heruistics) {
			foreach (var item in heruistics)
				AddHeuristics(item);
		}
		public void AddHeuristics(HeuristicSet heruistics) {
			if (heruistics.Label == null)
				throw new ArgumentException("The training library requires labeled heuristics", "heruistics");

			List<HeuristicSet> list;
			if (!content.TryGetValue(heruistics.Label, out list))
				content.Add(heruistics.Label, (list = new List<HeuristicSet>()));

			list.Add(heruistics);

			allLabels.Add(heruistics.Label);
		}

		/// <summary>Key is the label associated with the value which is a set of heuristic return values</summary>
		private readonly Dictionary<string, List<HeuristicSet>> content
															= new Dictionary<string, List<HeuristicSet>>();
		private readonly List<string> allLabels = new List<string>();

		private IEnumerable<RecognizedSegment> GetMatches(HeuristicSet unlabledHeuristic) {
			int numberOfUniqueLabels = allLabels.Count();
			int sizeOfHeuristicArray = unlabledHeuristic.Count;
			int numberOfLabelsToCount = numberOfUniqueLabels;
			double[][] probabilityFromEachHeuristic = new double[numberOfLabelsToCount][];
			double[][] lblComparisonResults = new double[numberOfLabelsToCount][];
			double[] labelProbability;
			double[] totalComparison_test = new double[numberOfLabelsToCount];

			if (variances == null) {
				variances = new variancesForEachHeuristic(sizeOfHeuristicArray);
			}

			for (int i = 0; i < numberOfLabelsToCount; i++) {
				probabilityFromEachHeuristic[i] = new double[sizeOfHeuristicArray];
				lblComparisonResults[i] = new double[sizeOfHeuristicArray];
			}

			for (int heurIdx = 0; heurIdx < sizeOfHeuristicArray; heurIdx++) {
				for (int lblIdx = 0; lblIdx < numberOfUniqueLabels; lblIdx++) {
					string currentLabel = allLabels[lblIdx];
					for (int lblTrialIdx = 0; lblTrialIdx < content[currentLabel].Count(); lblTrialIdx++) {
						if (unlabledHeuristic.GetAtIndex(heurIdx) == content[currentLabel][lblTrialIdx].GetAtIndex(heurIdx)) {
							lblComparisonResults[lblIdx][heurIdx]++;
						}
					}
					totalComparison_test[lblIdx] += lblComparisonResults[lblIdx][heurIdx];
				}
				for (int labelIndex = 0; labelIndex < numberOfUniqueLabels; labelIndex++) {
					string currentLabel = allLabels[labelIndex];
					lblComparisonResults[labelIndex][heurIdx] = lblComparisonResults[labelIndex][heurIdx] / (double)content[currentLabel].Count;
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
						multiplicativeOffset = variances.AddValue(heuristicProbabilisticIndication, heurIdx, inspectionLbl);
						multiplicativeOffset += aprioriProb / (double)variances.var2[heurIdx][inspectionLbl].Count;
						if (multiplicativeOffset < double.MaxValue) {
							labelProbability[inspectionLbl] *= (factorIncrease * heuristicProbabilisticIndication + multiplicativeOffset) / (1 - heuristicProbabilisticIndication + multiplicativeOffset);
						}
						if (double.IsInfinity(labelProbability[inspectionLbl]) || labelProbability[inspectionLbl] == 0)
							heurIdx = sizeOfHeuristicArray;
					}
				}
				string currentLabel = allLabels[inspectionLbl];
				if (content[currentLabel].Count > 0) {
					yield return new RecognizedSegment(unlabledHeuristic.Bounds, currentLabel, labelProbability[inspectionLbl]);
				}
			}
		}

		private class variancesForEachHeuristic {
			public List<List<RollingVariance>> var2 = new List<List<RollingVariance>>();

			public variancesForEachHeuristic(int numberOfHeur) {
				for (int i = 0; i < numberOfHeur; i++)
					var2.Add(new List<RollingVariance>());
			}

			public class RollingVariance {
				public int Count { get; private set; }
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

			public double AddValue(double value, int heuristicIdx, int labelIdx) {
				if (labelIdx >= var2[heuristicIdx].Count) {
					while (labelIdx >= var2[heuristicIdx].Count)
						var2[heuristicIdx].Add(new RollingVariance());
					return var2[heuristicIdx][labelIdx].Append(value);
				} else {
					return var2[heuristicIdx][labelIdx].Append(value);
				}
			}
		}

		private static variancesForEachHeuristic variances; //TODO: Make this its own class so it won't have to be static 
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
