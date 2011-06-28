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

		///<summary>Describes a single string recognized in an image.</summary>
		public struct TextSegment {
			///<summary>Creates a RecognizedSegment value.</summary>
			public TextSegment(Rect bounds, string text)
				: this() {
				Bounds = bounds;
				Text = text;
			}

			///<summary>Gets the area in the image that contains the string.</summary>
			public Rect Bounds { get; private set; }
			///<summary>Gets the recognized text.</summary>
			public string Text { get; private set; }
		}
		static readonly char[] whitespaceChars = " \t\r\n".ToCharArray();	//TODO: More chars
		static IEnumerable<TextSegment> GetWords(string text, IEnumerable<TextLine> lines) {
			string fullText = text + "\n";
			int lineStart = 0;	//In characters
			double top = 0;		//In pixels
			foreach (var line in lines) {
				int lastSpace = lineStart;
				while (lastSpace < lineStart + line.Length - 1) {
					while (lastSpace < lineStart + line.Length && Char.IsWhiteSpace(fullText[lastSpace]))
						lastSpace++;	//Skip over the previous chunk of whitespace

					if (lastSpace == lineStart + line.Length)
						continue;		//If the line ends in whitespace, skip it entirely

					//Find the next space within this line
					int nextSpace = fullText.IndexOfAny(whitespaceChars, lastSpace + 1, line.Length - (lastSpace + 1 - lineStart));

					if (nextSpace < 0)		//Include the last word, even if it doesn't end with a space.
						nextSpace = lineStart + line.Length - 1;

					//if (nextSpace == lastSpace) continue;	//Entirely Skip double spaces

					var word = text.Substring(lastSpace, nextSpace - lastSpace);
					var bounds = line.GetTextBounds(lastSpace, word.Length);

					//bounds is relative to the line
					yield return new TextSegment(Rect.Offset(bounds[0].Rectangle, 0, top), word);

					lastSpace = nextSpace;
				}
				lineStart += line.Length;
				top += line.Height;
				line.Dispose();
			}
		}

		[STAThreadAttribute]
		static void Main(string[] args) {
			string fileName = @"C:\Users\Public\Pictures\temp.bmp";
			string renderMe = "The quick brown fox...\r\n...";	//"تلبستبي بيسا سي";

			var output = new DrawingGroup();
			BasicTextParagraphProperties format = new BasicTextParagraphProperties("Tahoma", 13, FlowDirection.LeftToRight);

			//Execute the entire iterator
			var words = GetWords(renderMe, Measurer.MeasureLines(renderMe, 200, format, output)).ToList();
			output.ToBitmap().CreateStream(fileName).Close();
			Document uploadDocument = new Document(fileName);

			foreach (var word in words) {
				TestRectangularBounds(word.Bounds, uploadDocument.documentImage);
			}

			IteratedBoards boards = uploadDocument.DefineIteratedBoards();
			//TODO: automated training
			uploadDocument.Segment();
		}

		private static void TestRectangularBounds(Rect r, System.Drawing.Bitmap bitmap) {
			Rectangle rect = new Rectangle((int)Math.Round(r.X), (int)Math.Round(r.Y), (int)Math.Round(r.Width), (int)Math.Round(r.Height));

			//rect.X = bitmap.Width - rect.Width;
			//rect.Y = bitmap.Height - rect.Height;
			Graphics g = Graphics.FromImage(bitmap);
			g.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Black), rect);
		}
	}
}
