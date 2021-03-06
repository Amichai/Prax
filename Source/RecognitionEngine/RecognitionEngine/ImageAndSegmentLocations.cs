﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
	
namespace Prax.Recognition {
	class ImageAndSegmentLocations {
		private Tree segmentData = new Tree();
		public Bitmap TrainingImage = new Bitmap(1, 1);
		enum RenderMethod { LetterByLetter, WholeTextAtOnce, MeasureRanges };
		public ImageAndSegmentLocations() {
			RenderMethod renderMethod = RenderMethod.LetterByLetter;
			string dataFileName = @"C:\Users\Amichai\Documents\Prax\Trunk\Source\RecognitionEngine\RecognitionEngine\bin\Debug\TxtToRender.txt"; 
			string dataFontName = "Times New Roman";
			string dataSize = "16";
			string dataStyle = "".ToLower();

			FontStyle style = FontStyle.Regular;
			if (dataStyle.Contains("b"))
				style |= FontStyle.Bold;
			if (dataStyle.Contains("i"))
				style |= FontStyle.Italic;

			using (var font = new Font(dataFontName, float.Parse(dataSize), style, GraphicsUnit.Pixel)) {
				var lines = WrapLines(File.ReadLines(dataFileName)).ToArray();
				var text = String.Join(Environment.NewLine, lines);
				//TODO: Figure out why we can't get the image to render in two lines
				var size = TextRenderer.MeasureText(text, font);
				var image = new Bitmap(size.Width * 2, size.Height * 2);

				using (var objGraphics = Graphics.FromImage(image)) {
					objGraphics.Clear(Color.White);

					switch (renderMethod) {
						case RenderMethod.LetterByLetter:
							drawTextLetterByLetter(objGraphics, font, Brushes.Black, lines);
							break;
						case RenderMethod.WholeTextAtOnce:
							objGraphics.DrawString(text, font, Brushes.Black, new PointF(image.Width, 0), new StringFormat(StringFormatFlags.DirectionRightToLeft));
							break;
						case RenderMethod.MeasureRanges:
							MeasureText(objGraphics, font, text);
							break;
					}

					TrainingImage = image;
					image.Save(renderMethod.ToString() + ".bmp", ImageFormat.Bmp);
				}
			}
			printSegmentData();
		}

		private void printSegmentData() {
			FileStream segmentDataFile = new FileStream("segmentData.txt", FileMode.Create);
			StreamWriter writer = new StreamWriter(segmentDataFile);

			for (int i = 0; i < segmentData.YCoordinates.Count; i++) {
				writer.Write("Y val: " + segmentData.YCoordinates[i].Value.ToString());
				writer.Write(Environment.NewLine);
				for (int j = 0; j < segmentData.YCoordinates[i].XCoordinates.Count(); j++) {
					writer.Write("X val: " + segmentData.YCoordinates[i].XCoordinates[j].Value.ToString());
					foreach (KeyValuePair<int, string> widthChar in segmentData.YCoordinates[i].XCoordinates[j].Texts) {
						writer.Write(" Width: " + widthChar.Key.ToString() + " Char: " + widthChar.Value);
					}

					writer.Write(Environment.NewLine);
				}
				writer.Write(Environment.NewLine + Environment.NewLine);
			}
		} 

		static IEnumerable<string> WrapLines(IEnumerable<string> lines, int maxLength = 100) {
			StringBuilder currentLine = new StringBuilder();
			foreach (var line in lines) {
				currentLine.Clear();
				foreach (var word in line.Split(' ')) {
					if (currentLine.Length + 1 + word.Length > maxLength            //Leave room for the space
					 && !(currentLine.Length == 0 && word.Length >= maxLength)) {   //If the word alone won't fit, just use it.
						yield return currentLine.ToString();
						currentLine.Clear();
					}

					if (currentLine.Length > 0)
						currentLine.Append(' ');
					currentLine.Append(word);
				}

				if (currentLine.Length > 0)
					yield return currentLine.ToString();
			}
		}
		#region Tree Data Sturcture
		class Tree {
			public List<BoundedString> Words = new List<BoundedString>();
			public List<BoundedString> Letters = new List<BoundedString>();

			public List<YCoordinate> YCoordinates = new List<YCoordinate>();

			public void AddLetter(BoundedString letter) { Letters.Add(letter); AddNode(letter); }
			public void AddWord(BoundedString word) { Words.Add(word); AddNode(word); }

			void AddNode(BoundedString val) {
				YCoordinate existingCoordinate = GetExistingNode(val.Bounds.Y);
				if (existingCoordinate == null) {
					XCoordinate xcoord = new XCoordinate(val.Bounds.X);
					xcoord.Texts[val.Bounds.Width] = val.Text;
					YCoordinate ycoord = new YCoordinate(val.Bounds.Y);
					ycoord.XCoordinates.Add(xcoord);
					this.YCoordinates.Add(ycoord);
				} else {
					existingCoordinate.AddNode(val.Bounds.X, val.Bounds.Width, val.Text);
				}
			}

			private YCoordinate GetExistingNode(int yCoordinate) {
				foreach (YCoordinate coord in this.YCoordinates) {
					if (coord.Value == yCoordinate)
						return coord;
				}
				return null;
			}
		}

		class YCoordinate {
			public void AddNode(int xCoordinate, int width, string text) {
				XCoordinate existingCoordinate = GetExistingNode(xCoordinate);
				if (existingCoordinate == null) {
					XCoordinate xcoord = new XCoordinate(xCoordinate);
					xcoord.Texts[width] = text;
					this.XCoordinates.Add(xcoord);
				} else {
					existingCoordinate.Texts[width] = text;
				}
			}

			private XCoordinate GetExistingNode(int xCoordinate) {
				foreach (XCoordinate coord in this.XCoordinates) {
					if (coord.Value == xCoordinate)
						return coord;
				}
				return null;
			}

			public YCoordinate(int value) {
				Value = value;
			}
			public int Value;
			public List<XCoordinate> XCoordinates = new List<XCoordinate>();
		}

		class XCoordinate {
			public XCoordinate(int value) {
				Value = value;
			}
			public int Value;
			public Dictionary<int, string> Texts = new Dictionary<int, string>();
		}
		#endregion

		void MeasureText(Graphics g, Font font, string text) {
			g.DrawString(text, font, Brushes.Black, g.VisibleClipBounds);

			List<CharacterRange> letterRanges = new List<CharacterRange>();
			List<CharacterRange> wordRanges = new List<CharacterRange>();

			int wordStart = 0;

			for (int i = 0; i < text.Length; i++) {
				if (!Char.IsWhiteSpace(text, i))
					letterRanges.Add(new CharacterRange(i, 1));
				else {					//We've reached the end of a word
					if (wordStart < i)	//If the word has characters in it, add the word
						wordRanges.Add(new CharacterRange(wordStart, i - wordStart));
					wordStart = i + 1;	//Skip this whitespace
				}
			}
			if (wordStart < text.Length)	//If the last word has characters in it, add it
				wordRanges.Add(new CharacterRange(wordStart, text.Length - wordStart));

			foreach (var letter in MeasureRanges(g, font, text, letterRanges))
				segmentData.AddLetter(letter);
			foreach (var word in MeasureRanges(g, font, text, wordRanges))
				segmentData.AddWord(word);
		}

		///<summary>The maximum number of ranges that we'll process at once.</summary>
		///<remarks>I got an Overflow error from GDI+ when processing 800 ranges.</remarks>
		const int maxRanges = 32;		//The maximum number of ranges that we'll process at once.
		static IEnumerable<BoundedString> MeasureRanges(Graphics g, Font font, string text, List<CharacterRange> ranges) {
			for (int r = 0; r < ranges.Count; r += maxRanges) {
				//Process at most maxRanges at a time
				var subRanges = new CharacterRange[Math.Min(ranges.Count - r, maxRanges)];
				ranges.CopyTo(r, subRanges, 0, subRanges.Length);

				Region[] regions;
				using (var format = new StringFormat()) {	//Dispose the StringFormat before returning results
					format.SetMeasurableCharacterRanges(subRanges);
					regions = g.MeasureCharacterRanges(text, font, g.VisibleClipBounds, format);
				}

				for (int i = 0; i < subRanges.Length; i++) {
					RectangleF bounds;
					using (regions[i])
						bounds = regions[i].GetBounds(g);

					yield return new BoundedString(
						text.Substring(subRanges[i].First, subRanges[i].Length),
						Rectangle.Round(bounds)
					);
				}
			}
		}
		private void drawTextLetterByLetter(Graphics objGraphics, Font font, Brush brush, IEnumerable<string> lines) {
			int docWidth, docHeight;
			docWidth = (int)objGraphics.VisibleClipBounds.Size.Width;
			docHeight = (int)objGraphics.VisibleClipBounds.Size.Height;

			Rectangle textBound = new Rectangle(0, 0, docWidth, docHeight);

			float yIndex = docHeight / 4;
			float tempWidth = docWidth - docWidth / 4;
			//SizeF spaceSize = objGraphics.MeasureStringSize(" ", font);
			SizeF spaceSize = new SizeF(9, 0);
			foreach (string line in lines) {
				float xIndex = tempWidth;
				foreach (string word in line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) {
					float wordWidth = 0;
					float wordXidx = xIndex;
					for (int i = 0; i < word.Count(); i++) {
						char c = ExtractedOCRFunctionality.UnicodeConversion.convertUnicodeChar(word, ref i);
						if (c == 0) continue;
						SizeF size = objGraphics.MeasureStringSize(c.ToString(), font);
						float width = size.Width;

						xIndex -= width;
						objGraphics.DrawString(c.ToString(), font, brush, new PointF(xIndex, yIndex));
						segmentData.AddLetter(new BoundedString(c.ToString(), new Rectangle((int)Math.Round(xIndex) + 5, (int)Math.Round(yIndex), (int)Math.Round(width), (int)Math.Round(size.Height))));

						wordWidth += width;
					}
					objGraphics.DrawString(" ", font, brush, xIndex, yIndex);
					var wordHeight = objGraphics.MeasureStringSize(word, font).Height;
					xIndex -= spaceSize.Width;
					segmentData.AddWord(new BoundedString(word, new Rectangle((int)Math.Round(xIndex), (int)Math.Round(yIndex), (int)Math.Round(wordWidth), (int)Math.Round(wordHeight))));
				}
				float height = objGraphics.MeasureString(line, font).Height;
				yIndex += height;
			}
		}

		private void CalculateGap(Graphics objGraphics, Font font, out int charHorizontalGap, out int charVerticalGap) {
			int combinedWidth = (int)objGraphics.MeasureString("ab", font).Width;
			int aWidth = (int)objGraphics.MeasureString("a", font).Width;
			int bWidth = (int)objGraphics.MeasureString("b", font).Width;

			charHorizontalGap = combinedWidth - aWidth - bWidth;
			int combinedHeight = (int)objGraphics.MeasureString("a" + Environment.NewLine + "b", font).Height;
			int aHeight = (int)objGraphics.MeasureString("a", font).Height;
			int bHeight = (int)objGraphics.MeasureString("b", font).Height;
			charVerticalGap = combinedHeight - aHeight - bHeight;
		}

		private Tuple<string, double> getTextAndRating(BoundedString rect, Rectangle segmentLocation) {
			double area, overlap, overlapRating;

			area = rect.Bounds.Area();
			overlap = Rectangle.Intersect(segmentLocation, rect.Bounds).Area();
			if (segmentLocation != rect.Bounds) //Test for exact match lest divide by 0 error
				overlapRating = (overlap) / ((area - overlap) + (segmentLocation.Area() - overlap));
			else
				overlapRating = int.MaxValue;
			return new Tuple<string, double>(rect.Text, overlapRating);
		}

		private double getOverlapRating(Rectangle rect, Rectangle segmentLocation) {
			double area, overlap, overlapRating;
			area = rect.Area();
			overlap = Rectangle.Intersect(segmentLocation, rect).Area();
			if (segmentLocation != rect) //Test for exact match lest divide by 0 error
				overlapRating = (overlap) / ((area - overlap) + (segmentLocation.Area() - overlap));
			else
				overlapRating = int.MaxValue;
			return overlapRating;
		}

		public Tuple<string, double> LabelAtThisSegmentLocation(Rectangle segmentLocation, bool isWord) {
			
			double thresholdOverlap = .5;
			int topBottomThreshold = 4;

			var allSegments = isWord ? segmentData.Words : segmentData.Letters;


			var newRectangles = allSegments.Where(t => Rectangle.Intersect(segmentLocation, t.Bounds).Width > 0 &&
													   Rectangle.Intersect(t.Bounds, segmentLocation).Width / (double)t.Bounds.Width > thresholdOverlap
													   && segmentLocation.Y - t.Bounds.Y < topBottomThreshold);
			if (newRectangles.Any()) {
				foreach(BoundedString boundedString in newRectangles){
					int width = boundedString.Bounds.Right - boundedString.Bounds.X;
					double overlapRating = getOverlapRating(boundedString.Bounds, segmentLocation);
					double widthOverlap = (double)segmentLocation.Width / boundedString.Bounds.Width;
					if (overlapRating > thresholdOverlap && widthOverlap > .8 && 1/widthOverlap > .8) {
						Debug.Print("Match Found: " + segmentLocation.ToString() + " " + boundedString.Bounds.ToString());
						Debug.Print("OverlapRating: " + overlapRating.ToString() + " widthOverlap: " + widthOverlap.ToString());
						return new Tuple<string, double>(boundedString.Text, overlapRating);
					} else {
						Debug.Print("No Match: " + segmentLocation.ToString() + " " + boundedString.Bounds.ToString());
						Debug.Print("OverlapRating: " + overlapRating.ToString() + " widthOverlap: " + widthOverlap.ToString());
					}
				}
			}


			//if (newRectangles.Any()) {
			//    int width = newRectangles.Max(r => r.Bounds.Right) - newRectangles.Min(r => r.Bounds.X);
			//    Rectangle newRect = new Rectangle(newRectangles.Min(r => r.Bounds.X), newRectangles.Min(r => r.Bounds.Y),
			//                                width,
			//                                newRectangles.Max(r => r.Bounds.Height));
			//    string newString = string.Concat(newRectangles.Select(t => t.Text));
			//    double overlapRating = getOverlapRating(newRect, segmentLocation);
			//    double widthOverlap = (double)segmentLocation.Width / newRect.Width;
			//    if (overlapRating > thresholdOverlap &&  widthOverlap > .8 && newString.Count() == 1) {
			//        Debug.Print("OverlapRating: " + overlapRating.ToString() + " width ovelap: " + widthOverlap.ToString());
			//        return new Tuple<string, double>(newString, overlapRating);
			//    } else {
			//        Debug.Print("No match: " + noMatchCounter++.ToString() + " " + segmentLocation.ToString() + " " + newRect.ToString() + " certainty: " + overlapRating.ToString());
			//        Debug.Print("OverlapRating: " + overlapRating.ToString() + " width ovelap: " + widthOverlap.ToString());
			//    }

			//}
			return null;
		}
		int noMatchCounter = 0;
	}

	///<summary>Contains a string and its location in an image.</summary>
	struct BoundedString : IEquatable<BoundedString> {
		public BoundedString(string text, Rectangle bounds)
			: this() {
			if (text == null) throw new ArgumentNullException("text");
			Text = text;
			Bounds = bounds;
		}

		///<summary>Gets the text at the location.</summary>
		public string Text { get; private set; }
		///<summary>Gets the precise bounding box of the string.</summary>
		public Rectangle Bounds { get; private set; }

		//TODO: Add utility methods to perform common tasks (eg, Intersect)

		#region Equality
		///<summary>Returns a unique hash code that identifies this value.</summary>
		public override int GetHashCode() { return Text.GetHashCode() ^ Bounds.GetHashCode(); }
		///<summary>Checks whether this value is equal to another BoundedString value.</summary>
		public bool Equals(BoundedString other) { return Text == other.Text && Bounds == other.Bounds; }
		///<summary>Checks whether this value is equal to an object.</summary>
		public override bool Equals(object obj) { return obj is BoundedString && Equals((BoundedString)obj); }
		///<summary>Checks whether two BoundedString values are equal.</summary>
		public static bool operator ==(BoundedString a, BoundedString b) { return a.Equals(b); }
		///<summary>Checks whether two BoundedString values are unequal.</summary>
		public static bool operator !=(BoundedString a, BoundedString b) { return !(a == b); }
		#endregion
	}
}
