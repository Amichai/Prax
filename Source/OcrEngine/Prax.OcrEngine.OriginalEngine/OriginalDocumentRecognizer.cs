﻿using System;
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
			//TODO: Report progress

			var imageData = new ImageData(document);
			var boards = imageData.DefineIteratedBoards();

			var heuristics = boards.Segment().ToList();

			progress.Maximum = heuristics.Count * 1000;
			return heuristics.Select(h => trainingData.PerformLookup(h, progress.ScaledChildOperation(1000)).FirstOrDefault())
							 .Where(rs => rs != null);
		}
	}
}
