using System.Linq;
using System.Windows;
using System.Windows.Media;
using Prax.OcrEngine.Engine.AutomatedTraining;
using Prax.OcrEngine.Engine.Segmentation;
using Segmentation;
using TextRenderer;

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

		public void TestAlgorithm() {
			//fileQueue.ProcessFiles();
		}

		public void TrainAlgorithm() {
			//AlgorithmTrainer trainer = new AlgorithmTrainer();
		}
		public void RenderAnImage() {
			string renderText = "تلبستبي بيسا سي";
			var output = new DrawingGroup();
			var format = new BasicTextParagraphProperties("Tahoma", 13, FlowDirection.LeftToRight);
			var charSegments = TextSegment.GetWords(renderText, Measurer.MeasureLines(renderText, 200, format, output)).ToList();
			var RenderedText = new RenderedText(renderText, charSegments);
			var stream = output.ToBitmap().CreateStream();
			var imageData = new ImageData(stream);
			stream.Close();

			var boards = imageData.DefineIteratedBoards();
			var trainingData = new TrainingLibrary();

			foreach (var word in RenderedText.WordBounds)
				boards.Train(word, trainingData);

			foreach (var segment in boards.Segment()) {
				var returnVal = trainingData.PerformLookup(segment);
			}
		}
	}
	static class Program {
		static void Main(string[] args) {
			var UI = new ExposedFunctionality();
			UI.RenderAnImage();
		}
	}
}
