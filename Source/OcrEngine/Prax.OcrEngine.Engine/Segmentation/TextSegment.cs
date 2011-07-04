using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media.TextFormatting;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Controls;

namespace Segmentation {
	///<summary>Describes a single string recognized in an image.</summary>
	public class TextSegment {
		///<summary>Creates a RecognizedSegment value.</summary>
		public TextSegment(Rect bounds, string text, int index){
			Bounds = bounds;
			Text = text;
			LetterIndexValue = index;
		}
		/// <summary>Location of letter within the word (zero indexed).</summary>
		public int LetterIndexValue { get; private set; }
		///<summary>Gets the area in the image that contains the string.</summary>
		public Rect Bounds { get; private set; }
		///<summary>Gets the recognized text.</summary>
		public string Text { get; private set; }

		public static IEnumerable<TextSegment> GetWords(string text, IEnumerable<TextLine> lines) {
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

					//bounds is relative to the line
					for (int i = 0; i < word.Count(); i++) {
						string letter = word[i].ToString();

						var bounds = line.GetTextBounds(lastSpace, 1);
						lastSpace++;
						yield return new TextSegment(Rect.Offset(bounds[0].Rectangle, 0, top), letter, i);
					}

					lastSpace = nextSpace;
				}
				lineStart += line.Length;
				top += line.Height;
				line.Dispose();
			}
		}
		static readonly char[] whitespaceChars = " \t\r\n".ToCharArray();	//TODO: More chars
	}
}
