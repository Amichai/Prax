using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prax.OcrEngine.Engine.PatternRecognition;
using Prax.OcrEngine.Engine.ImageRecognition;

namespace Prax.OcrEngine.Engine.ExposedFunctionality {
	class ExposedFunctionality {
		//FileQueue fileQueue = new FileQueue();
		//OCRResults results = new OCRResults();
		//TrainingDataLibrary = new ReferenceLibrary();
		PatternRecognizer patternRecognizer = new PatternRecognizer();

		public void GetImage(){
			//Open file dialog
			ImageData imageData = new ImageData();
			//fileQueue.Add(imageData);
		}

		public void OpenTrainingData{
			//Open file dialog
			//trainingDataLibrary = new ReferenceLibrary();
		}

		public void EditTrainingData{
			//if(trainingDataLibrary != null)
				//trainingDataLibrary.ContentEditor();
		}

		public void OpenOCRResults{
			//results.Display();
		}

		public void TestAlgorithm{
			//fileQueue.ProcessFiles();
		}

		public void TrainAlgorithm{
			//AlgorithmTrainer trainer = new AlgorithmTrainer();
		}
	}
}
