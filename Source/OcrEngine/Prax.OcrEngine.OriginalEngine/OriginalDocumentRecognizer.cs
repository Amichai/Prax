using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Prax.OcrEngine.Services;
using System.ComponentModel;
using System.IO;
using Prax.OcrEngine.Engine.Segmentation;
using Prax.OcrEngine.Engine.HeuristicGeneration;
using Prax.OcrEngine.Engine.ReferenceData;
using SLaks.Progression;

namespace Prax.OcrEngine.Engine {
	public class OriginalDocumentRecognizer : IDocumentRecognizer {
		readonly IReferenceSearcher trainingData;
		public OriginalDocumentRecognizer(IReferenceSearcher trainingData) { this.trainingData = trainingData; }

		public IEnumerable<RecognizedSegment> Recognize(Stream document, IProgressReporter progress) {
			var imageData = new ImageData(document);
			var boards = imageData.DefineIteratedBoards();

			var heuristics = boards.Segment().ToList();

			progress.Maximum = heuristics.Count * 1000;

			foreach (var segment in heuristics) {
				var whitespaceResults = trainingData.PerformWhitespaceLookup(segment, progress.ScaledChildOperation(500)).LastOrDefault(r => r.Certainty > 10);

				if (whitespaceResults != null && whitespaceResults.Text == "AllLabels") {
					var match = trainingData.PerformLookup(segment, progress.ScaledChildOperation(500)).LastOrDefault(r => r.Certainty > 10);
					if (match != null)
						yield return match;
				} else
					progress.Progress += 500;	//Add the progress that would have been used by the character recognition
			}
		}
	}
}
