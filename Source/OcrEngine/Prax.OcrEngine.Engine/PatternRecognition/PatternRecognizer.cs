using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prax.OcrEngine.Engine.ReferenceData;

namespace Prax.OcrEngine.Engine.PatternRecognition {
	///<summary>Recognizes patterns.</summary>
	public class PatternRecognizer {
		///<summary>Maintains the variance of a heuristic in an array.</summary>
		///<remarks>
		/// WARNING: This is a mutable struct.
		/// This is designed to be used in an array (not a List).
		/// It must never be copied or exposed; it must ONLY be used from in the array.
		/// 
		/// Using a struct avoids initialization issues and saves memory by not
		/// having separate references. 
		/// This type must never be copied, or it will defeat the purpose.
		///</remarks>
		struct RollingVariance {
			public int count;
			double mean, M2;

			public double calcOnlineVar(double x) {
				count++;
				double delta = x - mean;
				mean = mean + delta / count;
				M2 = M2 + delta * (x - mean);

				double variance = M2 / (count - 1);
				return variance;
			}
		}

		///<summary>Matches a set of data to the closest match in a set of reference data.</summary>
		public RecognizedPattern Recognize(IReferenceSet set, int[] data) {
			double[][] probabilityFromEachHeuristic = new double[set.Labels.Count][];
			double[][] lblComparisonResults = new double[set.Labels.Count][];
			double[] labelProbability;

			double[] totalComparison_test = new double[set.Labels.Count];	//TODO: Delete this? Its values are never used!

			RollingVariance[][] variances = new RollingVariance[set.Labels.Count][];

			for (int i = 0; i < set.Labels.Count; i++) {
				probabilityFromEachHeuristic[i] = new double[set.HeuristicCount];
				lblComparisonResults[i] = new double[set.HeuristicCount];
				variances[i] = new RollingVariance[set.HeuristicCount];
			}

			//TODO: Move this to IReferenceSet?
			var labelData = set.Labels.Select(s => set.GetItems(s).ToList()).ToList();

			//Get the probability that the input matches each unique label
			for (int label = 0; label < labelData.Count; label++) {
				var labelSamples = labelData[label];

				for (int h = 0; h < set.HeuristicCount; h++) {
					if (data[h] == -1)
						continue;		//TODO:  || data[h] == 0) //RESOLVE THE NECESSITY OF != 0 IN THIS STATEMENT

					var score = labelSamples.Count(r => r.Data[h] == data[h]);

					totalComparison_test[label] += score;
					lblComparisonResults[label][h] = (double)score / labelSamples.Count;
				}
			}

			//This data structure represents:
			//What percentage of the time the current heuristics on the current heuristic array were equal to the heuristics on the training data for every given number

			//We are working to produce two DSs: lblComparisonResults[][], totalComparison_test[]

			double heuristicProbabilisticIndication;
			double multiplicativeOffset;
			labelProbability = new double[set.Labels.Count];
			double maxProb = 0;
			int maxProbIndex = 0;
			double aprioriProb = 1.0 / (double)set.Labels.Count;
			double factorIncrease = (1.0 - aprioriProb) / aprioriProb;

			for (int inspectionLbl = 0; inspectionLbl < set.Labels.Count; inspectionLbl++) {
				labelProbability[inspectionLbl] = 1.0 / (double)set.Labels.Count;
				for (int heurIdx = 0; heurIdx < set.HeuristicCount; heurIdx++) {
					double comparisonToThisLabel = lblComparisonResults[inspectionLbl][heurIdx];
					double comparisonToOtherLabels = 0;
					for (int comparisonLbl = 0; comparisonLbl < set.Labels.Count; comparisonLbl++) {
						if (inspectionLbl != comparisonLbl)
							comparisonToOtherLabels += lblComparisonResults[comparisonLbl][heurIdx];
					}

					if (comparisonToThisLabel + comparisonToOtherLabels != 0) {
						heuristicProbabilisticIndication = comparisonToThisLabel / (comparisonToThisLabel + comparisonToOtherLabels);
						buildHeuristicProbabilityHistorgram(heuristicProbabilisticIndication, inspectionLbl, heurIdx);
						multiplicativeOffset = variances[inspectionLbl][heurIdx].calcOnlineVar(heuristicProbabilisticIndication);

						multiplicativeOffset += aprioriProb / (double)variances[inspectionLbl][heurIdx].count;

						if (multiplicativeOffset < double.MaxValue)
							labelProbability[inspectionLbl] *= (factorIncrease * heuristicProbabilisticIndication + multiplicativeOffset) / (1 - heuristicProbabilisticIndication + multiplicativeOffset);

						if (double.IsInfinity(labelProbability[inspectionLbl]) || labelProbability[inspectionLbl] == 0)
							heurIdx = set.HeuristicCount;
					}
				}

				if (labelProbability[inspectionLbl] > maxProb && labelData[maxProbIndex].Count > 0) {
					maxProb = labelProbability[inspectionLbl];
					maxProbIndex = inspectionLbl;
				}
			}
			if (maxProb > 0)
				return new RecognizedPattern(set.Labels[maxProbIndex], maxProb);
			return null;
		}

		//From HeuristicsControlPanel
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
	}
}

