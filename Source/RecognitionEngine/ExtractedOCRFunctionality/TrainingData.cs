using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtractedOCRFunctionality {
	/// <summary>Training data is a set of heuristics return values associated with their corresponding input label.
	/// Traning data is used to take an unlabeled set of heuristic return values and to compare those return values
	/// to labeled sets of heuristic return value and use that comparison to determine the most appropriate label to 
	/// associate.</summary>
	public class TrainingData {
		/// <summary>Key is the label associated with the value which is a set of heuristic return values</summary>
		private Dictionary<string, List<HeuristicReturnValues>> library 
															= new Dictionary<string, List<HeuristicReturnValues>>();
		public void AddHeuristics(HeuristicReturnValues returnValuesToAdd) {
			if (returnValuesToAdd.Label == null) {
				throw new NullReferenceException(
					"Trying to add an unlabeled set of heuristic return values to the traning data library");
			}
			if(library.ContainsKey(returnValuesToAdd.Label)){
				//Add the heuristic return values to the list associated with the corresponding label in the library
				List<HeuristicReturnValues> listOfHeuristics = library[returnValuesToAdd.Label];
				listOfHeuristics.Add(returnValuesToAdd);
				library[returnValuesToAdd.Label] = listOfHeuristics;
			} else {
				//Create a new label entry in the library
				library.Add(returnValuesToAdd.Label, new List<HeuristicReturnValues>(){returnValuesToAdd});
			}
		}
		/// <summary>Take an unlabeled HeursiticReturnVaules object and compare it to each key value pair in the 
		/// library and return the best match as a LookupResult</summary>
		public List<LookupResult> PerformLookUp(HeuristicReturnValues unlabeledReturnValues) {
			if (unlabeledReturnValues.Label != null)
				throw new Exception("This guy is supposed to be unlabeled!");
			List<LookupResult> comparisonValues = new List<LookupResult>();
			comparisonValues = library.Compare(unlabeledReturnValues);
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

	public static class LibraryExtensionMethods {
		public static LookupResult Compare(this List<HeuristicReturnValues> labeledSet, 
																		HeuristicReturnValues unlabledHeuristic) {
			//Implement a comparison between labeled and unlabeled heuristic return values
			throw new NotImplementedException();
		}

		public static List<LookupResult> Compare(this Dictionary<string, List<HeuristicReturnValues>> library, HeuristicReturnValues unlabledHeuristic) {
			{
				throw new NotImplementedException();
				/*
				int numberOfUniqueLabels = library.Count();
				int sizeOfHeuristicArray = unlabledHeuristic.Count;
				//int numberOfLabelsToCount = numberOfUniqueLabels - IndiciesToCheck.Count;
				int numberOfLabelsToCount = numberOfUniqueLabels;
				double[][] probabilityFromEachHeuristic = new double[numberOfLabelsToCount][];
				double[][] lblComparisonResults = new double[numberOfLabelsToCount][];
				double[] labelProbability;
				double[] totalComparison_test = new double[numberOfLabelsToCount];

				for (int i = 0; i < numberOfLabelsToCount; i++) {
					probabilityFromEachHeuristic[i] = new double[sizeOfHeuristicArray];
					lblComparisonResults[i] = new double[sizeOfHeuristicArray];
				}

				for (int heurIdx = 0; heurIdx < sizeOfHeuristicArray; heurIdx++) {
						foreach(var indicies in library){
							for(int lblTrial =0; lblTrial < indicies.Value.Count(); lblTrial++){
								if(unlabledHeuristic.GetAtIndex(heurIdx) == indicies.Value[lblTrial].GetAtIndex(heurIdx)){
									lblComparisonResults[inspectionLbl][heurIdx]++;
								}
							}
							totalComparison_test[inspectionLbl] += lblComparisonResults[inspectionLbl][heurIdx];
						}
					for (int labelIndex = 0; labelIndex < numberOfUniqueLabels; labelIndex++)
					//foreach (int labelIndex in getInspectionLabelIdicies())
                {
						lblComparisonResults[labelIndex][heurIdx] = lblComparisonResults[labelIndex][heurIdx] / (double)listOfIndicies[labelIndex].Count;
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
					if (labelProbability[inspectionLbl] > maxProb && listOfIndicies[maxProbIndex].Count > 0) {
						maxProb = labelProbability[inspectionLbl];
						maxProbIndex = inspectionLbl;
					}
				}
				if (maxProb > 0) {
					return new Tuple<string, double>(listOfIndexLabels[maxProbIndex], maxProb);
				} else {
					return null;
				}
				 */ 
			}
		}
	}
}
