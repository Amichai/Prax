﻿using System.Linq;
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
using System.Collections.Generic;
using Prax.OcrEngine.Services;
using SLaks.Progression.Display;

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
			//string renderText = "أد";
			//string renderText = "ﺀﺁﺂﺃﺄﺅﺆﺇﺈﺉﺊﺋﺌﺍﺎﺏﺐﺑﺒﺓﺔﺕﺖﺗﺘﺙﺚﺛﺜﺝﺞﺟﺠﺡﺢﺣﺤﺥﺦﺧﺨﺩﺪﺫﺬﺭﺮﺯﺰﺱﺲﺳﺴﺵﺶﺷﺸﺹﺺﺻﺼﺽﺾﺿﻀﻁﻂﻃﻄﻅﻆﻇﻈﻉﻊﻋﻌﻍﻎﻏﻐﻑﻒﻓﻔﻕﻖﻗﻘﻙﻚﻛﻜﻝﻞﻟﻠﻡﻢﻣﻤﻥﻦﻧﻨﻩﻪﻫﻬﻭﻮﻯﻰﻱﻲﻳﻴﻵﻶﻷﻸﻹﻺﻻﻼ";
			//string renderText = "ﺬﻤﺈﺗﻹﻕﻐﻘﻝﺺﻔﺒﺭﻧﻂﺓﻫﻲﺸﺪﺊﻭﻜﺶﻆﻸﻒﺆﺄﻤﻴﺣﻄﻗﻖﺹﺶﺅﺆﺔﻣﻕﻨﺝﻓﻂﺭﺡﺀﻷﺣﺜﻷﻞﺶﺼﺈﻎﻍﺁﺂﻊﻓﻶﻃﺰﻊﻒﺑﺈﻙﻦﻬﺣﻃﺲﺄﺿﻠﺄﻊﺰﺆﺊﻁﻗﺨﻣﺡﺚﺱﻤﺴﺄﺑﺙﺂﻮﻖﺢﻴﻘﺐﻴﺞﺹﺬﻔﺛﺄﺷﻅﻫﻈﺳﻎﻃﻉﺵﻨﺇﻤﺱﻴﻔﻱﺲﻨﺑﺩﺯﺒﻥﺽﺍﻯﺠﺏﻨﺇﺫﻑﻈﺜﻟﻨﻶﻨﻎﻀﻒﺵﺹﻫﺚﺏﺧﺙﺟﺪﺇﻁﻂﻮﻑﺤﻝﺭﺻﻁﺮﺴﻝﺭﺽﺳﺛﺔﺔﻩﻸﻆﺙﻎﻓﻶﻖﻷﺣﻺﻇﺇﻬﺾﺀﻦﺎﻖﻈﺻﺋﻈﺭﺌﺑﺞﻕﺋﺮﻤﻱﺒﺅﺳﺮﺽﺨﻱﺛﻗﻊﻣﺊﺽﻶﺧﻄﺞﺭﻔﺤﻁﻉﻕﻝﺯﺘﺌﺼﺴﺡﻊﻈﺼﺉﺵﺁﺹﺏﺿﺾﺚﺻﻭﻭﺥﺽﻬﺓﻧﻗﺷﻚﻗﺿﺯﻅﺬﻒﻼﺥﺛﺴﻣﺶﻼﺚﻋﺳﻁﺥﺊﺎﻫﺕﺊﺆﻙﻥﺸﻯﺨﻶﻒﻚﻧﻭﻮﻹﻗ";

			var output = new DrawingGroup();
			var format = new BasicTextParagraphProperties("Times New Roman", 14, FlowDirection.LeftToRight);
			var words = BoundedWord.GetWords(renderText, Measurer.MeasureLines(renderText, 200, format, output)).ToList();

			//using (var stream = output.ToBitmap().CreateStream()) {
			using (var stream = File.OpenRead(@"C:\Users\SSL\Temp\Arabic.png")) {
				//using (var targetStream = File.Create(@"C:\Users\SSL\Temp\Arabic-Console.png"))
				//    stream.CopyTo(targetStream);

				stream.Position = 0;

				var imageData = new ImageData(stream);
				stream.Position = 0;

				//System.Drawing.Bitmap b = boards.Boards.First().Matrix.ExtractRectangularContentArea(segment.Bounds).ConvertDoubleArrayToBitmap(System.Drawing.Color.White);
				//whitespaceResults.Last().Log(boards.Boards.First().Matrix.ExtractRectangularContentArea(segment.Bounds).ConvertDoubleArrayToBitmap(System.Drawing.Color.White));

				//characterResults.Last().Log(boards.Boards.First().Matrix.ExtractRectangularContentArea(segment.Bounds).ConvertDoubleArrayToBitmap(System.Drawing.Color.White));
				//Debug.Print(characterResults.Last().Text + " " + characterResults.Last().Certainty.ToString());

				var trainingData = new MutableReferenceSet();
				trainingData.ReadFrom(trainingFolder);
				var searcher = new ReferenceSearcher(trainingData);
				var recognizer = new OriginalDocumentRecognizer(searcher);


				var reporter = new ConsoleProgressReporter(false);
				var results = recognizer.Recognize(stream, reporter).ToList().AsReadOnly();

				var outputString = new StreamReader(OutputRenderer.PlainText.Convert(stream, results)).ReadToEnd();
				Debug.Print(outputString);
			}
		}
		//TODO: Test for white space first
		public void TrainAlgorithm() {
			string renderText = "أدخل نص هنا لترجمة";
			//string renderText = "ﻂﺎﺉﻳﺕﺍﻈﺕﻊﺹﻫﻑﻈﻂﻟﺐﺻﺔﺛﺅﺡﺽﻛﻟﺊﺒﻖﺼﻞﺫﻳﺀﺎﻠﻜﻯﺛﺠﻔﺡﺜﺠﻹﺡﺕﺨﺜﻛﻃﺜﺗﺯﺘﺈﻞﺄﺰﺦﻨﺂﺳﺋﺳﺟﻃﻪﻃﻤﻣﻭﻱﺚﺽﻷﻦﺁﻁﺹﻊﻞﻲﻷﻶﻁﺓﻃﻥﺅﺂﻗﺿﻲﺵﻑﻪﺐﻩﻚﻛﺧﻢﺵﻵﻓﺨﻹﻯﺝﺟﻝﺿﻳﻘﺉﺌﺸﺒﻮﺋﻼﺅﺽﻢﺋﺭﻻﻢﻨﺍﻕﺔﻤﻛﺢﻹﺽﺂﺣﺙﺖﻑﻇﻣﺢﺘﻈﺙﺫﻄﺱﻺﻷﻎﻛﺢﺥﻏﻩﺥﺩﺗﺇﺐﻆﻈﻘﺘﺶﺍﻌﺶﻬﻕﻱﺫﺺﻖﻀﻰﻢﻡﺶﺣﻘﻗﺙﺖﻼﻼﻔﻫﺕﻺﻬﻼﻉﺶﺜﻖﺼﺰﻠﺞﻵﻸﺎﺢﻙﺯﻤﻋﺏﻵﺿﻄﻝﻡﺵﻬﺯﻂﺘﺶﺵﻞﺠﻮﻥﻱﻟﺄﺺﻦﺳﺲﺁﺘﺪﻜﻉﻴﺹﻍﺘﺉﻛﺜﺻﺧﺟﻥﺁﺭﺧﻷﺗﺪﻥﺗﻗﺜﺹﻥﻍﻠﺐﺥﻂﺍﻃﻅﺶﻕﺣﺸﺈﻇﺞﺬﺂﺃﺳﺽﻒﻶﺃﺌﺿﺸﺚﻓﺯﺒﺄﺬﺺﻐﺀﺴﻵﺶﻗﺭﻡﻗﺮﺨﺊﻫﻯﻌﺌﺹﺕﻔﻦﻸﺒﻓﻝﺄﻢﺉﻯﻬﺕﺤﻐﻳﻺﺵﻇﻦﺴﻉﺨﺅﻙﻷﻇﻗﺂﻐﻆﻊﻇﻲﻺﺭﻃﺾﺽﺩﻓﻰﻐﻎﺺﻱﻁﺊﻕﺓﻭﺺﺅﺢﺄﻈﺫﻠﺟﺘﺌﺆﺂﺑﺿﺤﺤﺟﺦﺣﺰﺼﻴﺞﺒﻀﻶﻗﻇﻤﺶﺼﺂﻫﻟﺌﻅﺥﻮﺱﺁﻟﺟﺥﺝﻍﻮﺾﺎﺖﻌﺡﻠﺱﻠﺱﻂﻣﺏﻁﻏﻐﺘﺷﻞﻌﺃﺓﺮﺳﻜﻼﺮﻲﺡﻍﺝﻎﺀﻂﻎﺷﻉﺱﺘﺔﻙﻍﺊﺮﻆﻞﺕﺭﻞﺭﻇﻤﻃﻍﻉﻛﺏﻗﺶﻍﻯﻫﺐﺒﺔﻱﻢﻊﻯﺗﺇﻣﺞﻉﻰﺳﺉﺀﻩﻺﻦﺁﺓﻰﺆﺵﺊﺰﻵﻰﺨﻮﻷﻈﺢﻪﺽﻴﺚﻒﻸﻈﺺﻆﺦﻠﻅﻶﻚﺖﻑﺏﻫﺹﻣﻮﻗﺜﺌﻛﻧﺲﻒﻻﻬﺕﻨﻓﺧﺅﻳﺑﻣﺗﺁﺞﻄﻉﻉﺢﻬﺐﻒﻞﺞﺘﺃﺾﻡﺱﺒﺠﻨﺻﻨﻅﻲﺻﻗﺑﻵﻯﻏﻔﺟﻀﺩﻓﺷﻄﺩﺎﻐﻦﻬﻖﻝﻰﻜﻓﺒﺪﻮﻳﻰﻓﺦﺧﺬﺸﺨﺈﻯﺸﺪﻺﺾﻺﺾﻛﻬﺬﺆﺝﺇﺄﻍﺁﻋﺑﻥﺂﻜﺃﻻﻊﺀﻘﻡﻉﺚﺁﺧﻻﺛﺡﺛﺗﺼﻯﺘﻠﺱﺁﻤﻣﺣﻑﻬﻫﺆﺤﻘﺚﻐﺦﺩﻬﺺﻜﻺﻢﺘﻲﻀﻫﺄﺇﺁﻢﻦﺵﻁﻙﻶﺄﻍﻚﺛﺘﺧﺫﻨﻳﻃﻲﺥﺌﺫﻱﻵﺫﻚﻜﻲﺲﻇﺼﺽﻵﻞﺛﻐﺊﺅﻍﻠﻻﺡﻌﻖﻋﻐﺡﺵﻎﻷﺢﺮﻋﻅﻴﻙﺌﻩﺺﻘﻢﻤﻃﺹﺐﺜﺣﺷﺋﻘﻞﻭﻳﺒﺦﻨﻜﺛﺪﻘﺸﺧﺣﺸﺥﺎﻯﺰﻦﺿﻃﺝﻈﻱﻺﺗﻚﺒﻅﺯﺭﻧﺩﻳﺘﺲﺷﺸﺞﺌﻺﻭﺿﻞﺠﺺﻁﺋﺔﺂﺩﻯﻍﻉﺕﻀﺤﺂﺔﺏﺍﻊﻓﻷﺪﻍﻻﺭﺟﻰﻓﺷﺒﻚﻂﻒﻛﻦﻑﻀﻓﻏﻺﻂﺅﺺﺚﻝﻞﻕﺍﺵﺣﺎﻼﺰﺚﻄﺴﻏﺼﺓﻨﺡﺫﻯﻝﻰﺇﺿﻢﺻﺮﻑﺹﺻﻌﺐﻲﻬﺳﺝﻌﻇﻢﺞﺓﺃﻛﺒﺸﺗﺲﺁﺃﺿﻣﻘﺖﺒﻚﺦﻖﺞﻯﻏﺇﻏﻒﻁﻠﺉﻎﻄﺛﺞﻄﺡﺼﺍﻜﻚﺕﻲﺇﻊﺈﺬﻍﻷﻔﻱﻷﻠﺒﺌﻄﺭﻔﺓﺖﻀﺮﺙﺮﺯﻅﻫﻢﻢﻷﻂﺜﺉﺳﺀﺑﻡﺗﺥﻆﻘﺁﻗﻩﻫﺏﻫﺲﺎﻗﺗﻉﺨﻳﺛﺫﻸﻃﻕﺅﺚﻆﻨﻼﺑﻔﻭﺬﺑﻸﻠﺖﺯﻉﺻﺾﻓﺭﻀﺟﻖﺸﻔﺊﻠﻶﻰﺒﺗﻑﺖﺝﺿﺟﺬﺑﺖﻗﻘﺖﺞﺾ";
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

			var characters = words.SelectMany(w => w.Characters).ToList();
			for (int i = 0; i < characters.Count(); i++) {
				var ch1 = characters[i];
				var letterHeuristics = boards.GetLetterHeuristics(ch1);
				if (i < characters.Count() - 1) {
					var ch2 = characters[i + 1];
					var whitespaceHeuristics = boards.GetSpaceHeuristics(ch1, ch2);
					trainingData.AddHeuristics(whitespaceHeuristics);
				}
				if (letterHeuristics != null)
					trainingData.AddHeuristics(letterHeuristics);
			}
			ImageUtilities.Utilities.TrainingLog.ToString();

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
