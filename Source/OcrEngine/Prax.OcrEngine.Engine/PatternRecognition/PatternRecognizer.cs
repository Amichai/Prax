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

			///<summary>Adds a number to the variated set.</summary>
			///<returns>The new variance.</returns>
			public double AddSample(double x) {
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
			double[][] labelScores = new double[set.Labels.Count][];

			double[] totalComparison_test = new double[set.Labels.Count];	//TODO: Delete this? Its values are never used!

			//TODO: Why did this use to be static?
			RollingVariance[][] variances = new RollingVariance[set.Labels.Count][];


			//Get the probability that the input matches each unique label
			int labelIndex = 0;
			foreach (var label in set.GetAllItems()) {
				variances[labelIndex] = new RollingVariance[set.HeuristicCount];

				var scores = labelScores[labelIndex] = new double[set.HeuristicCount];
				int sampleCount = 0;	//We're using a forward-only iterator; don't call Count()

				//Count total samples and the matches for each heuristic
				//in the same loop (because we must not iterate over the
				//label twice)
				foreach (var sample in label) {
					sampleCount++;
					for (int h = 0; h < set.HeuristicCount; h++) {
						if (data[h] == -1)	//TODO:  || data[h] == 0) //RESOLVE THE NECESSITY OF != 0 IN THIS STATEMENT
							continue;
						if (data[h] == sample.Data[h])
							scores[h]++;
					}
				}

				for (int h = 0; h < set.HeuristicCount; h++) {
					totalComparison_test[labelIndex] += scores[h];
					labelScores[labelIndex][h] = (double)scores[h] / sampleCount;
				}
				labelIndex++;
			}

			//This data structure represents:
			//What percentage of the time the current heuristics on the current heuristic array were equal to the heuristics on the training data for every given number

			//We are working to produce two DSs: lblComparisonResults[][], totalComparison_test[]

			var labelProbabilities = new double[set.Labels.Count];

			double bestProb = 0;
			int bestLabel = 0;

			double aprioriProb = 1.0 / (double)set.Labels.Count;
			double factorIncrease = (1.0 - aprioriProb) / aprioriProb;

			for (int label = 0; label < set.Labels.Count; label++) {
				labelProbabilities[label] = aprioriProb;

				for (int h = 0; h < set.HeuristicCount; h++) {
					double comparisonToThisLabel = labelScores[label][h];

					//Get the total score against other labels in this heuristic
					double comparisonToOtherLabels = labelScores
						.Where((a, i) => i != label)
						.Sum(a => a[h]);

					//TODO: Since label scores cannot be negative, I assume that these two lines are equivalent
					//if (comparisonToThisLabel + comparisonToOtherLabels != 0) {
					if (comparisonToThisLabel == 0 && comparisonToOtherLabels == 0)
						continue;

					var heuristicProbabilisticIndication = comparisonToThisLabel / (comparisonToThisLabel + comparisonToOtherLabels);
					buildHeuristicProbabilityHistorgram(heuristicProbabilisticIndication, label, h);

					var multiplicativeOffset = variances[label][h].AddSample(heuristicProbabilisticIndication);
					multiplicativeOffset += aprioriProb / (double)variances[label][h].count;

					if (multiplicativeOffset < double.MaxValue)
						labelProbabilities[label] *= (factorIncrease * heuristicProbabilisticIndication + multiplicativeOffset)
												 / (1 - heuristicProbabilisticIndication + multiplicativeOffset);

					if (double.IsInfinity(labelProbabilities[label]) || labelProbabilities[label] == 0)
						break;		//Once we hit 0 or inf, we can never recover - all we do is multiply
				}

				if (labelProbabilities[label] > bestProb) {
					bestProb = labelProbabilities[label];
					bestLabel = label;
				}
			}
			if (bestProb > 0)
				return new RecognizedPattern(set.Labels[bestLabel], bestProb);
			return null;
		}

		//From HeuristicsControlPanel
		//TODO: Why did this use to be static?
		private int[] probabilityHistorgram = new int[21];
		void buildHeuristicProbabilityHistorgram(double probability, int labelUnderInspection, int heuristicUnderInspection) {
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

