using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExtractedOCRFunctionality;
using TextRenderer;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media.TextFormatting;
using System.Drawing.Imaging;
using System.Drawing;
using Prax.Recognition;
using System.Windows.Controls;


namespace TestProject {
	static class Program {
		[STAThreadAttribute]
		static void Main(string[] args) {
			string fileName = @"C:\Users\Public\Pictures\temp.png";
			string renderMe = "تلبستبي بيسا سي";
			var output = new DrawingGroup();
			BasicTextParagraphProperties format = new BasicTextParagraphProperties("Tahoma", 13, FlowDirection.LeftToRight);
			var segments = TextSegment.GetWords(renderMe, Measurer.MeasureLines(renderMe, 200, format, output)).ToList();
			output.ToBitmap().CreateStream(fileName).Close();
			Document uploadDocument = new Document(fileName);

			//List<string> wordsRendered = renderMe.Split(' ').ToList();
			//List<CharacterBounds> boundsForeachWord = new List<CharacterBounds>();
			//foreach (var word in wordsRendered) {
			//    boundsForeachWord.Add(new CharacterBounds(segments, word));
			//}
			//IteratedBoards boards = uploadDocument.DefineIteratedBoards();
			////TrainingData trainingData;
			////foreach (var word in boundsForeachWord) {
			////    trainingData = boards.Train(word);
			////}
			//foreach (var testBoard in boards.Segment()) {}
		}
	}
}
