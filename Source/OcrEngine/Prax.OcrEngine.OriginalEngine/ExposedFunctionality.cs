using System.Linq;
using System.Windows;
using System.Windows.Media;
using Prax.OcrEngine.Engine.AutomatedTraining;
using Prax.OcrEngine.Engine.Segmentation;
using Prax.OcrEngine.Engine.ImageUtilities;
using TextRenderer;
using Prax.OcrEngine.Engine.ReferenceData;
using System.IO;
using System;
using System.Diagnostics;

namespace Prax.OcrEngine.Engine {
	class ExposedFunctionality {
		//FileQueue fileQueue = new FileQueue();
		//OCRResults results = new OCRResults();
		//TrainingDataLibrary = new ReferenceLibrary();

		public void GetImage() {
			//Open file dialog
			//ImageData imageData = new ImageData();
			//fileQueue.Add(imageData);
		}

		public void OpenTrainingData() {
			//Open file dialog
			//trainingDataLibrary = new ReferenceLibrary();
		}

		public void EditTrainingData() {
			//if(trainingDataLibrary != null)
			//trainingDataLibrary.ContentEditor();
		}

		public void OpenOCRResults() {
			//results.Display();
		}

		static readonly string trainingFolder = Environment.ExpandEnvironmentVariables(@"%TEMP%\PadOcrTraining");
		public void TestAlgorithm() {
			string renderText = "أدخل نص هنا لترجمة";
			var output = new DrawingGroup();
			var format = new BasicTextParagraphProperties("Times New Roman", 14, FlowDirection.LeftToRight);
			var words = BoundedWord.GetWords(renderText, Measurer.MeasureLines(renderText, 200, format, output)).ToList();
			var stream = output.ToBitmap().CreateStream();

			var imageData = new ImageData(stream);
			stream.Close();
			var boards = imageData.DefineIteratedBoards();
			var trainingData = new MutableReferenceSet();
			trainingData.ReadFrom(trainingFolder);
			var searcher = new ReferenceSearcher(trainingData);
			var results = new OutputRenderer();
			foreach (var segment in boards.Segment()) {
				var returnVal = searcher.PerformLookup(segment).Where(r => r.Certainty > OutputRenderer.ThresholdCertainty).ToList();
				if (returnVal.Count() > 0) {
					results.Add(returnVal);
					Debug.Print(returnVal.First().Text + " " + returnVal.First().Certainty.ToString());
				}
			}
			var outputString = results.Render();
		}

		public void TrainAlgorithm() {

			string renderText = "أدخل نص هنا لترجمة";
			var output = new DrawingGroup();
			var format = new BasicTextParagraphProperties("Times New Roman", 14, FlowDirection.LeftToRight);
			var words = BoundedWord.GetWords(renderText, Measurer.MeasureLines(renderText, 200, format, output)).ToList();
			var stream = output.ToBitmap().CreateStream();
			var imageData = new ImageData(stream);
			imageData.SaveFile("RenderedFile1.png");
			stream.Close();

			Directory.CreateDirectory(trainingFolder);

			var boards = imageData.DefineIteratedBoards();
			var trainingData = new MutableReferenceSet();
			
			//ADD TO EXISTING TRAINING DATA LIBRARY:
				//trainingData.ReadFrom(trainingFolder);

			foreach (var ch in words.SelectMany(w => w.Characters)) {
				var heuristics = boards.GetHeuristics(ch);
				if (heuristics != null)
					trainingData.AddHeuristics(heuristics);
			}

			trainingData.WriteTo(trainingFolder);
		}
		//TODO: Train for white space recognition 
	}
	static class Program {
		static void Main(string[] args) {
			var UI = new ExposedFunctionality();
			UI.TrainAlgorithm();
			UI.TestAlgorithm();
		}
	}
}
