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

namespace Prax.OcrEngine.Engine {
	public class OriginalDocumentProcessor : DocumentProcessorBase {
		readonly IReferenceSearcher trainingData;
		public OriginalDocumentProcessor(IReferenceSearcher trainingData) { this.trainingData = trainingData; }

		public override void ProcessDocument(Stream document) {
			//TODO: Report progress

			var imageData = new ImageData(document);
			var boards = imageData.DefineIteratedBoards();

			IEnumerable<HeuristicSet> heuristics = boards.Segment();

			Results = new ReadOnlyCollection<RecognizedSegment>(
				heuristics.Select(h => trainingData.PerformLookup(h).FirstOrDefault())
						  .Where(rs => !String.IsNullOrWhiteSpace(rs.Text))	//Where it isn't default(RecognizedSegment)
						  .ToList()
			);
		}
	}
}
