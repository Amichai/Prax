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
		public LookupResult PerformLookUp(HeuristicReturnValues unlabeledReturnValues) {
			if (unlabeledReturnValues.Label != null)
				throw new Exception("This guy should is supposed to be unlabeled!");
			List<LookupResult> comparisonValues = new List<LookupResult>();
			foreach (var labeledReturnValues in library) {
				comparisonValues.Add(labeledReturnValues.Value.Compare(unlabeledReturnValues));
			}
			return comparisonValues.OrderBy(i => i.ConfidenceValue).Last();
		}
	}
	/// <summary>Contains the label to be associated with the unlabeled HeuristicReturnValues and a confidence value
	/// with which reflects the algorithm's confidence in making that assignment.</summary>
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
			//Implement a comparison between labeled and unlabeled heuristics retrun values
			throw new NotImplementedException();
		}
	}
}
