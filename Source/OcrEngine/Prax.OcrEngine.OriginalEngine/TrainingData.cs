using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prax.OcrEngine.Engine.HeuristicGeneration;

namespace Prax.OcrEngine.Engine {
	

	/// <summary>Training data is a set of heuristics return values associated with their corresponding input label.
	/// Traning data is used to take an unlabeled set of heuristic return values and to compare those return values
	/// to labeled sets of heuristic return value and use that comparison to determine the most appropriate label to 
	/// associate.</summary>
	public class TrainingData {
		private Library Library = new Library();
		
		public void AddHeuristics(HeuristicReturnValues returnValuesToAdd) {
			if (returnValuesToAdd.Label == null) {
				throw new NullReferenceException(
					"Trying to add an unlabeled set of heuristic return values to the traning data library");
			}
			if(Library.Content.ContainsKey(returnValuesToAdd.Label)){
				//Add the heuristic return values to the list associated with the corresponding label in the library
				List<HeuristicReturnValues> listOfHeuristics = Library.Content[returnValuesToAdd.Label];
				listOfHeuristics.Add(returnValuesToAdd);
				Library.Content[returnValuesToAdd.Label] = listOfHeuristics;
			} else {
				//Create a new label entry in the library
				Library.Content.Add(returnValuesToAdd.Label, new List<HeuristicReturnValues>(){returnValuesToAdd});
				Library.ListOfLabels.Add(returnValuesToAdd.Label);
			}
		}
		/// <summary>Take an unlabeled HeursiticReturnVaules object and compare it to each key value pair in the 
		/// library and return the be	st match as a LookupResult</summary>
		public List<LookupResult> PerformLookUp(HeuristicReturnValues unlabeledReturnValues) {
			if (unlabeledReturnValues.Label != null)
				throw new Exception("This guy is supposed to be unlabeled!");
			List<LookupResult> comparisonValues = new List<LookupResult>();
			comparisonValues = Library.Compare(unlabeledReturnValues);
			return comparisonValues.OrderBy(i => i.ConfidenceValue).ToList();
		}
	}
	/// <summary>Contains the label to be associated with the unlabeled HeuristicReturnValues and a confidence value
	/// which reflects the algorithm's confidence in making that assignment.</summary>
	public class LookupResult {
		public LookupResult(string lbl, double confidence) {
			this.Label = lbl;
			this.ConfidenceValue = confidence;
		}
		public string Label { get; set; }
		public double ConfidenceValue { get; set; }
	}

	public class Library{
		/// <summary>Key is the label associated with the value which is a set of heuristic return values</summary>
		public Dictionary<string, List<HeuristicReturnValues>> Content 
															= new Dictionary<string, List<HeuristicReturnValues>>();
		public List<string> ListOfLabels = new List<string>();
		public List<LookupResult> Compare(HeuristicReturnValues unlabledHeuristic) {
			var results = new List<LookupResult>();
			int numberOfUniqueLabels = ListOfLabels.Count();
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
				for (int lblIdx = 0; lblIdx < numberOfUniqueLabels; lblIdx++){
					string currentLabel = ListOfLabels[lblIdx];
					for (int lblTrialIdx = 0; lblTrialIdx < Content[currentLabel].Count(); lblTrialIdx++){
						if(unlabledHeuristic.GetAtIndex(heurIdx) == Content[currentLabel][lblTrialIdx].GetAtIndex(heurIdx)){
							lblComparisonResults[lblIdx][heurIdx]++;
						}
					}
					totalComparison_test[lblIdx] += lblComparisonResults[lblIdx][heurIdx];
				}
				for (int labelIndex = 0; labelIndex < numberOfUniqueLabels; labelIndex++){
					string currentLabel = ListOfLabels[labelIndex];
					lblComparisonResults[labelIndex][heurIdx] = lblComparisonResults[labelIndex][heurIdx] / (double)Content[currentLabel].Count;
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

			for (int inspectionLbl = 0; inspectionLbl < numberOfUniqueLabels; inspectionLbl++)
			{
				labelProbability[inspectionLbl] = 1.0 / (double)numberOfLabelsToCount;
				for (int heurIdx = 0; heurIdx < sizeOfHeuristicArray; heurIdx++) {
					double comparisonToThisLabel = lblComparisonResults[inspectionLbl][heurIdx];
					double comparisonToOtherLabels = 0;
					for (int comparisonLbl = 0; comparisonLbl < numberOfUniqueLabels; comparisonLbl++)
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
				string currentLabel = ListOfLabels[inspectionLbl];
				if (Content[currentLabel].Count > 0) {
					var result = new LookupResult(currentLabel, labelProbability[inspectionLbl]);
					results.Add(result);
				}
			}
			if (results.Count() > 0) {
				return results;
			} else {
				return null;
			}
		}

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
