using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Prax.OcrEngine.Engine.Tests.PatternRecognition {
	class LegacyRecognizer {
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


		#region Test/Read algorithm
		private class variancesForEachHeuristic {
			public List<List<rollingVarianceCalc>> var2 = new List<List<rollingVarianceCalc>>();

			public variancesForEachHeuristic(int numberOfHeur) {
				for (int i = 0; i < numberOfHeur; i++)
					var2.Add(new List<rollingVarianceCalc>());
			}

			public class rollingVarianceCalc {
				public int n = 0;
				double mean = 0,
						M2 = 0;

				public double calcOnlineVar(double x) {
					n++;
					double delta = x - mean;
					mean = mean + delta / n;
					M2 = M2 + delta * (x - mean);

					double variance = M2 / (n - 1);
					return variance;
				}
			}

			public double newVarianceValue(double value, int heuristicIdx, int labelIdx) {
				if (labelIdx >= var2[heuristicIdx].Count) {
					while (labelIdx >= var2[heuristicIdx].Count)
						var2[heuristicIdx].Add(new rollingVarianceCalc());
					return var2[heuristicIdx][labelIdx].calcOnlineVar(value);
				} else {
					return var2[heuristicIdx][labelIdx].calcOnlineVar(value);
				}
			}
		}

		private static variancesForEachHeuristic variances; //TODO: Make this its own class so it won't have to be static 
		private static HeuristicsControlPanel heuristicsControl = new HeuristicsControlPanel();

		private int numberOfUniqueLabels;
		public Tuple<string, double> Recognize(LegacyDataLibrary data, int[] heuristicArray) {
			int sizeOfHeuristicArray = heuristicArray.Length;// 1550;

			numberOfUniqueLabels = data.listOfIndexLabels.Count;
			//int numberOfLabelsToCount = numberOfUniqueLabels - IndiciesToCheck.Count;
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
				//foreach(int inspectionLbl in getInspectionLabelIdicies())
				for (int inspectionLbl = 0; inspectionLbl < numberOfUniqueLabels; inspectionLbl++) //This is the label we are trying to get information about. We want to know how feasible this label is given our current array
				{
					for (int lblTrialIdx = 0; lblTrialIdx < data.listOfIndicies[inspectionLbl].Count; lblTrialIdx++) { //Iterate through every heuristic array within our given label
						if (heuristicArray[heurIdx] == data.trainingLibrary[data.listOfIndicies[inspectionLbl][lblTrialIdx]].Item2[heurIdx] && heuristicArray[heurIdx] != -1)//  && heuristicArray[heurIdx] != 0) //RESOLVE THE NECESSITY OF != 0 IN THIS STATEMENT
						{
							lblComparisonResults[inspectionLbl][heurIdx]++;
						}
					}
					totalComparison_test[inspectionLbl] += lblComparisonResults[inspectionLbl][heurIdx];
				}
				for (int labelIndex = 0; labelIndex < numberOfUniqueLabels; labelIndex++)
				//foreach (int labelIndex in getInspectionLabelIdicies())
				{
					lblComparisonResults[labelIndex][heurIdx] = lblComparisonResults[labelIndex][heurIdx] / (double)data.listOfIndicies[labelIndex].Count;
				}
				//This data structure represents:
			}       //What percentage of the time the current heuristics on the current heuristic array were equal to the heuristics on the training data for every given number

			//We are working to produce two DSs: lblComparisonResults[][], totalComparison_test[]

			double heuristicProbabilisticIndication;
			double multiplicativeOffset;
			labelProbability = new double[numberOfLabelsToCount];
			double maxProb = 0;
			int maxProbIndex = 0;
			double aprioriProb = 1.0 / (double)numberOfLabelsToCount;
			double factorIncrease = (1.0 - aprioriProb) / aprioriProb;

			for (int inspectionLbl = 0; inspectionLbl < numberOfUniqueLabels; inspectionLbl++)
			//foreach (int inspectionLbl in getInspectionLabelIdicies())
			{
				labelProbability[inspectionLbl] = 1.0 / (double)numberOfLabelsToCount;
				for (int heurIdx = 0; heurIdx < sizeOfHeuristicArray; heurIdx++) {
					double comparisonToThisLabel = lblComparisonResults[inspectionLbl][heurIdx];
					double comparisonToOtherLabels = 0;
					for (int comparisonLbl = 0; comparisonLbl < numberOfUniqueLabels; comparisonLbl++)
					//foreach (int comparisonLbl in getInspectionLabelIdicies())
					{
						if (inspectionLbl != comparisonLbl)
							comparisonToOtherLabels += lblComparisonResults[comparisonLbl][heurIdx];
					}

					if (comparisonToThisLabel + comparisonToOtherLabels != 0) {
						heuristicProbabilisticIndication = comparisonToThisLabel / (comparisonToThisLabel + comparisonToOtherLabels);
						heuristicsControl.buildHeuristicProbabilityHistorgram(heuristicProbabilisticIndication, inspectionLbl, heurIdx);
						multiplicativeOffset = variances.newVarianceValue(heuristicProbabilisticIndication, heurIdx, inspectionLbl);
						multiplicativeOffset += aprioriProb / (double)variances.var2[heurIdx][inspectionLbl].n;
						if (multiplicativeOffset < double.MaxValue) {
							labelProbability[inspectionLbl] *= (factorIncrease * heuristicProbabilisticIndication + multiplicativeOffset) / (1 - heuristicProbabilisticIndication + multiplicativeOffset);
						}
						if (double.IsInfinity(labelProbability[inspectionLbl]) || labelProbability[inspectionLbl] == 0)
							heurIdx = sizeOfHeuristicArray;
					}
				}
				if (labelProbability[inspectionLbl] > maxProb && data.listOfIndicies[maxProbIndex].Count > 0) {
					maxProb = labelProbability[inspectionLbl];
					maxProbIndex = inspectionLbl;
				}
			}
			if (maxProb > 0) {
				return new Tuple<string, double>(data.listOfIndexLabels[maxProbIndex], maxProb);
			} else {
				return null;
			}
		}
		#endregion
	}
}
