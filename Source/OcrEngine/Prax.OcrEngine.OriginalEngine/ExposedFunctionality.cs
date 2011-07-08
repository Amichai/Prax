using System.Linq;
using System.Windows;
using System.Windows.Media;
using Prax.OcrEngine.Engine.AutomatedTraining;
using Prax.OcrEngine.Engine.Segmentation;
using Segmentation;
using TextRenderer;
using Prax.OcrEngine.Engine.ReferenceData;
using System.IO;

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
			MemoryStream stream = new MemoryStream();
			var imageData = new ImageData(stream);
			stream.Close();
			var boards = imageData.DefineIteratedBoards();
			var trainingData = new MutableReferenceSet();

			var searcher = new ReferenceSearcher(trainingData);
			foreach (var segment in boards.Segment()) {
				var returnVal = searcher.PerformLookup(segment);
			}
		}

		public void TrainAlgorithm() {
			string renderText = "أدخل نص هنا لترجمة";
			var output = new DrawingGroup();
			var format = new BasicTextParagraphProperties("Tahoma", 14, FlowDirection.LeftToRight);
			var charSegments = TextSegment.GetWords(renderText, Measurer.MeasureLines(renderText, 200, format, output)).ToList();
			var RenderedText = new RenderedText(renderText, charSegments);
			var stream = output.ToBitmap().CreateStream();
			var imageData = new ImageData(stream);
			imageData.SaveFile("RenderedFile1.png");
			stream.Close();

			var boards = imageData.DefineIteratedBoards();
			var trainingData = new MutableReferenceSet();

			foreach (var word in RenderedText.WordBounds)
				boards.Train(word, trainingData);
		}

		//BUG: Run OriginalEngine as the startup project and call the TrainAlgorithm() method to see a null reference exception
		//TODO: Train for white space recognition 
		//BUG: The font being used to render the image in the simple web demo is different than the font being used to render 
		//within OriginalEngine and these need to be the same.
	}
	static class Program {
		static void Main(string[] args) {
			var UI = new ExposedFunctionality();
			UI.TrainAlgorithm();
		}
	}
}
