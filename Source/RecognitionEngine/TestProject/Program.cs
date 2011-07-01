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
		static BitmapSource ToBitmap(this DrawingGroup dg) {
			var dv = new DrawingVisual();
			using (var c = dv.RenderOpen())
				c.DrawImage(new DrawingImage(dg), new Rect(dg.Bounds.Size));
			var rtb = new RenderTargetBitmap((int)dg.Bounds.Width, (int)dg.Bounds.Height, 96, 96, PixelFormats.Pbgra32);
			rtb.Render(dv);

			return rtb;
		}


		[STAThreadAttribute]
		static void Main(string[] args) {
			string fileName = @"C:\Users\Public\Pictures\temp.png";
			string renderMe = "تلبستبي بيسا سي";
//			string renderMe = //"تلبستبي بيسا سي";


			var output = new DrawingGroup();
			BasicTextParagraphProperties format = new BasicTextParagraphProperties("Tahoma", 13, FlowDirection.LeftToRight);

			//Execute the entire iterator
			var words = TextSegment.GetWords(renderMe, Measurer.MeasureLines(renderMe, 200, format, output)).ToList();
			output.ToBitmap().CreateStream(fileName).Close();
			Document uploadDocument = new Document(fileName);
			CharacterBounds charBounds = new CharacterBounds(words);
			//charBounds.DrawOnImage(uploadDocument.documentImage);
			IteratedBoards boards = uploadDocument.DefineIteratedBoards();
			boards.Segment();
			//TODO: automated training
			Bitmap temp = uploadDocument.document.ExtractRectangularContentArea(charBounds.bounds.Last().Item1)
						.ConvertDoubleArrayToBitmap(System.Drawing.Color.White); 
		}
	}
}
