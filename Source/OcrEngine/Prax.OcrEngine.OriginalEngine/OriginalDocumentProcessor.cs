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

namespace Prax.OcrEngine.Engine {
	public class OriginalDocumentProcessor : DocumentProcessorBase {
		readonly TrainingData trainingData;
		public OriginalDocumentProcessor(TrainingData trainingData) { this.trainingData = trainingData; }

		public override void ProcessDocument(Stream document) {
			//TODO: Report progress

			var imageData = new ImageData(document);
			var boards = imageData.DefineIteratedBoards();

			//TODO: Segment
			//var heuristics = boards.Segment();
			IEnumerable<HeuristicReturnValues> heuristics = null;

			Results = new ReadOnlyCollection<RecognizedSegment>(
				heuristics.Select(h => trainingData.PerformLookUp(h).FirstOrDefault())
						  .Where(rs => !String.IsNullOrWhiteSpace(rs.Text))	//Where it isn't default(RecognizedSegment)
						  .ToList()
			);
		}
	}
}
