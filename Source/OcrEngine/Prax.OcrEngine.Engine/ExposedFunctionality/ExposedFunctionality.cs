using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prax.OcrEngine.Engine.PatternRecognition;
using Prax.OcrEngine.Engine.ImageRecognition;
using System.Windows.Media;
using System.Windows;
using Segmentation;
using TextRenderer;
using System.IO;
using Prax.OcrEngine.Engine.ImageUtilities;
using Prax.OcrEngine.Engine.AutomatedTraining;
using Prax.OcrEngine.Engine.HeuristicGeneration;
using Prax.OcrEngine.Engine.Training;
using Prax.OcrEngine.Engine.Segmentation;

namespace Prax.OcrEngine.Engine.ExposedFunctionality {
	class ExposedFunctionality {
		//FileQueue fileQueue = new FileQueue();
		//OCRResults results = new OCRResults();
		//TrainingDataLibrary = new ReferenceLibrary();
		PatternRecognizer patternRecognizer = new PatternRecognizer();

		public void GetImage(){
			//Open file dialog
			//ImageData imageData = new ImageData();
			//fileQueue.Add(imageData);
		}

		public void OpenTrainingData(){
			//Open file dialog
			//trainingDataLibrary = new ReferenceLibrary();
		}

		public void EditTrainingData(){
			//if(trainingDataLibrary != null)
				//trainingDataLibrary.ContentEditor();
		}

		public void OpenOCRResults(){
			//results.Display();
		}

		public void TestAlgorithm(){
			//fileQueue.ProcessFiles();
		}

		public void TrainAlgorithm(){
			//AlgorithmTrainer trainer = new AlgorithmTrainer();
		}
		public void RenderAnImage() {
			string renderText = "تلبستبي بيسا سي";
			var output = new DrawingGroup();
			var format = new BasicTextParagraphProperties("Tahoma", 13, FlowDirection.LeftToRight);
			var charSegments = TextSegment.GetWords(renderText, Measurer.MeasureLines(renderText, 200, format, output)).ToList();
			var renderedText = new RenderedText(renderText, charSegments);
			var stream = output.ToBitmap().CreateStream();
			var imageData = new ImageData(stream);
				stream.Close();
			var boards = imageData.DefineIteratedBoards();
			var trainingData = renderedText.ProduceTrainingData(boards);
			var heuristics = boards.Segment();
		}
	}
	static class Program {
		static void Main(string[] args) {
			var UI = new ExposedFunctionality();
			UI.RenderAnImage();
		}
	}
}
