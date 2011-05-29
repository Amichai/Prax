using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtractedOCRFunctionality {
	class TrainingDataLibrary {
		List<HeuristicArray> allData = new List<HeuristicArray>();
		Dictionary<string, HashSet<int>> libraryIndex = new Dictionary<string, HashSet<int>>();
	}

	class HeuristicArray {
		List<int> heuristics = new List<int>();
		List<string> dataLabels = new List<string>();
	}
}
