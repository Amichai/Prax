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
		enum RenderMethod { letterByLetter, wholeTextAtOnce };
		public ImageAndSegmentLocations() {
			RenderMethod renderMethod = RenderMethod.letterByLetter;
			string dataFileName = "doc.txt";
			string dataFontName = "Times New Roman";
			string dataSize = "16";
			string dataStyle = "".ToLower();

			FontStyle style = FontStyle.Regular;

			if (dataStyle.Contains("b"))
				style |= FontStyle.Bold;
			if (dataStyle.Contains("i"))
				style |= FontStyle.Italic;
			using (var font = new Font(dataFontName, float.Parse(dataSize), style, GraphicsUnit.Pixel)) {
				string text = string.Empty;
				StringCollection lines = new StringCollection();
				string textDocument = string.Empty;

				using (StreamReader reader = new StreamReader(new FileStream(dataFileName, FileMode.Open, FileAccess.Read))) {
					while (reader.Peek() != -1) {
						string line = reader.ReadLine();
						lines.Add(line);
					}
				}
				lines = getAlteredLines(lines);
				foreach (string s in lines) {
					text += s + Environment.NewLine;
				}

				var size = TextRenderer.MeasureText(text, font);
				var image = new Bitmap(size.Width * 2, size.Height * 2);

				using (var objGraphics = Graphics.FromImage(image)) {
					string imageFileName = string.Empty;
					objGraphics.Clear(Color.White);

					if (renderMethod == RenderMethod.letterByLetter) {
						drawTextLetterByLetter(objGraphics, font, Brushes.Black, lines);
						imageFileName = "letterByLetter.bmp";
					}
					if (renderMethod == RenderMethod.wholeTextAtOnce) {
						objGraphics.DrawString(text, font, Brushes.Black, new PointF(image.Width, 0), new StringFormat(StringFormatFlags.DirectionRightToLeft));
						imageFileName = "wholeTextAtOnce.bmp";
					}

					TrainingImage = image;
					image.Save(imageFileName, ImageFormat.Bmp);
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

		private StringCollection getAlteredLines(StringCollection document) {
			StringCollection lines = new StringCollection();

			foreach (string line in document) {
				string[] words = line.Split(new char[] { ' ' });

				string newline = string.Empty;
				bool bShortLine = false;
				foreach (string word in words) {
					bShortLine = true;
					if (newline.Length + word.Length <= 100) {
						bShortLine = true;
						newline += " ";
						newline += word;
					} else {
						bShortLine = false;
						lines.Add(newline);
						newline = word;
					}
				}
				if (bShortLine)
					lines.Add(newline);
			}

			return lines;
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

		static IEnumerable<BoundedString> MeasureRanges(Graphics g, Font font, string text, IEnumerable<CharacterRange> ranges) {
			Region[] regions;
			var allRanges = ranges.ToArray();
			using (var format = new StringFormat()) {	//Dispose the StringFormat before returning results
				format.SetMeasurableCharacterRanges(allRanges);
				regions = g.MeasureCharacterRanges(text, font, g.VisibleClipBounds, format);
			}

			for (int i = 0; i < allRanges.Length; i++) {
				RectangleF bounds;
				using (regions[i])
					bounds = regions[i].GetBounds(g);

				yield return new BoundedString(
					text.Substring(allRanges[i].First, allRanges[i].Length),
					Rectangle.Round(bounds)
				);
			}
		}

		private void drawTextLetterByLetter(Graphics objGraphics, Font font, Brush brush, StringCollection lines) {
			int docWidth, docHeight;
			docWidth = (int)objGraphics.VisibleClipBounds.Size.Width;
			docHeight = (int)objGraphics.VisibleClipBounds.Size.Height;

			Rectangle textBound = new Rectangle(0, 0, docWidth, docHeight);

			float yIndex = docHeight / 4;
			float tempWidth = docWidth - docWidth / 4;
			SizeF spaceSize = objGraphics.MeasureStringSize(" ", font);
			foreach (string line in lines) {
				float xIndex = tempWidth;
				foreach (string word in line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) {
					float wordWidth = 0;
					float wordXidx = xIndex;
					for (int i = 0; i < word.Count(); i++) {
						char c = convertUnicodeChar(word, ref i);
						if (c == 0) continue;
						SizeF size = objGraphics.MeasureStringSize(c.ToString(), font);
						float width = size.Width;

						xIndex -= width;
						objGraphics.DrawString(c.ToString(), font, brush, new PointF(xIndex, yIndex));
						segmentData.AddLetter(new BoundedString(c.ToString(), new Rectangle((int)Math.Round(xIndex) + 2, (int)Math.Round(yIndex), (int)Math.Round(width), (int)Math.Round(size.Height))));


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

		public Tuple<string, double> LabelAtThisSegmentLocation(Rectangle segmentLocation) {
			double thresholdOverlap = .3;

			var newRectangles = segmentData.Words.Concat(segmentData.Letters)	//TODO: Figure out what to search in
									.Where(t => Rectangle.Intersect(segmentLocation, t.Bounds).Width > 0 &&
											 Rectangle.Intersect(t.Bounds, segmentLocation).Width / (double)t.Bounds.Width > thresholdOverlap);
			if (newRectangles.Count() > 0) {
				int width = newRectangles.Max(r => r.Bounds.Right) - newRectangles.Min(r => r.Bounds.X);
				Rectangle newRect = new Rectangle(newRectangles.Min(r => r.Bounds.X), newRectangles.Min(r => r.Bounds.Y),
											width,
											newRectangles.Max(r => r.Bounds.Height));
				string newString = string.Concat(newRectangles.Select(t => t.Text));
				double overlapRating = getOverlapRating(newRect, segmentLocation);
				if (overlapRating > thresholdOverlap && (double)segmentLocation.Width / newRect.Width > .8) {
					return new Tuple<string, double>(newString, overlapRating);
				}
			}
			return null;
		}

		#region letter conversion
		private enum letterPosition {
			start, middle, end, isolated
		};

		private char testForLigatures(int currentChar, int nextChar) {
			if (currentChar == 1604 && nextChar == 1570) {
				return (char)65269;
			}
			if (currentChar == 1604 && nextChar == 1571) {
				return (char)65271;
			}
			if (currentChar == 1604 && nextChar == 1573) {
				return (char)65273;
			}
			if (currentChar == 1604 && nextChar == 1575) {
				return (char)65275;
			}
			return char.MinValue;
		}

		private HashSet<int> restrictedForms = new HashSet<int> { 1570, 1571, 1573, 1575, 1577, 1583, 1584, 1585, 1586, 1608, 1609 };

		private enum arabicLetterForms {
			restricted, unrestricted
		};

		private arabicLetterForms previousForm = arabicLetterForms.restricted;

		private char convertUnicodeChar(string word, ref int idx) {
			int currentChar = word[idx];
			arabicLetterForms currentForm;
			if (restrictedForms.Contains(currentChar))
				currentForm = arabicLetterForms.restricted;
			else
				currentForm = arabicLetterForms.unrestricted;

			letterPosition currentPosition = letterPosition.middle;

			if (idx == word.Count() - 1)
				currentPosition = letterPosition.end;
			if (idx == 0) {
				currentPosition = letterPosition.start;
				previousForm = arabicLetterForms.restricted;
			}
			if (word.Count() == 1)
				currentPosition = letterPosition.isolated;

			int newCharVal = getContextualForm(currentChar);
			if (newCharVal == char.MinValue)
				return char.MinValue;

			int nextChar = 0, prevChar = 0;

			if (idx > 0)
				prevChar = word[idx - 1];
			if (idx < word.Count() - 1)
				nextChar = word[idx + 1];

			int ligature = testForLigatures(currentChar, nextChar);
			if (ligature != char.MinValue) {
				newCharVal = ligature;
				currentForm = arabicLetterForms.restricted;
				idx++;
			}

			if (currentPosition == letterPosition.isolated) //stand alone letter
            {
				newCharVal = newCharVal + 0; //isolated glyph
			}

			if (currentPosition == letterPosition.start || previousForm == arabicLetterForms.restricted) {   //No right bind
				if (currentForm == arabicLetterForms.restricted)
					newCharVal = newCharVal + 0; //isolated glyph
				if (currentForm == arabicLetterForms.unrestricted && currentPosition != letterPosition.end)
					newCharVal = newCharVal + 2; //starting glyph
			}
			if ((currentPosition == letterPosition.end || currentForm == arabicLetterForms.restricted) && previousForm == arabicLetterForms.unrestricted) {  //Right bind only
				newCharVal = newCharVal + 1;
			}
			if (currentPosition != letterPosition.end && currentForm == arabicLetterForms.unrestricted && previousForm == arabicLetterForms.unrestricted) { //Right and left bind
				newCharVal = newCharVal + 3;
			}

			previousForm = currentForm;
			return (char)newCharVal;
		}

		int getContextualForm(int currentChar) {
			switch (currentChar) {
				case 1575: return 65165;
				case 1576: return 65167;
				case 1578: return 65173;
				case 1579: return 65177;
				case 1580: return 65181;
				case 1581: return 65185;
				case 1582: return 65189;
				case 1583: return 65193;
				case 1584: return 65195;
				case 1585: return 65197;
				case 1586: return 65199;
				case 1587: return 65201;
				case 1588: return 65205;
				case 1589: return 65209;
				case 1590: return 65213;
				case 1591: return 65217;
				case 1592: return 65221;
				case 1593: return 65225;
				case 1594: return 65229;
				case 1601: return 65233;
				case 1602: return 65237;
				case 1603: return 65241;
				case 1604: return 65245;
				case 1605: return 65249;
				case 1606: return 65253;
				case 1607: return 65257;
				case 1608: return 65261;
				case 1610: return 65265;
				case 1570: return 65153;
				case 1571: return 65155;
				case 1577: return 65171;
				case 1609: return 65263;
			}
			return char.MinValue;
		}
		#endregion
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
