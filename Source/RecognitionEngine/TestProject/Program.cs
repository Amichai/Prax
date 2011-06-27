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
	class Program {
		[STAThreadAttribute]
		static void Main(string[] args) {
			string fileName = @"C:\Users\Public\Pictures\temp.bmp";
			string renderMe = "تلبستبي بيسا سي";
			//Bitmap bitmap = 
			Renderer.RenderImage(renderMe).CreateStream(fileName).Close();//.BitmapSourceToBitmap2();
			Document uploadDocument = new Document(fileName);
			//uploadDocument.documentImage.Save("renderedImage.bmp");
			RenderTargetBitmap src = (RenderTargetBitmap)Renderer.RenderImage(renderMe);
			
			System.Windows.Controls.Image image = new System.Windows.Controls.Image();
			image.Source = src;
			
			var output = new DrawingGroup();
			BasicTextParagraphProperties format = new BasicTextParagraphProperties("Tahoma", 13, FlowDirection.LeftToRight);
			foreach (var line in Measurer.MeasureLines(renderMe, 300, format, output)) {
				var temp = line.GetTextBounds(0, renderMe.Count());
				Debug.Print(temp.ToString());
				TestRectangularBounds(temp, uploadDocument.documentImage);
			}

			IteratedBoards boards = uploadDocument.DefineIteratedBoards();
			//TODO: automated training
			uploadDocument.Segment();
		}

		private static void TestRectangularBounds(IList<TextBounds> textBounds, System.Drawing.Bitmap bitmap) {
			TextBounds bounds = textBounds.First();
			//Rectangle rect = bounds.Rectangle.ToRectangle();
			Rectangle rect = new Rectangle((int)bounds.Rectangle.X, (int)bounds.Rectangle.Y, (int)bounds.Rectangle.Width, (int)bounds.Rectangle.Height);
			rect.X = bitmap.Width - rect.Width;
			rect.Y = bitmap.Height - rect.Height;
			Graphics g = Graphics.FromImage(bitmap);
			g.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Black), rect);
		}
	}
}
