using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media.TextFormatting;

namespace Prax.OcrEngine.Engine.AutomatedTraining {
	public class BoundedWord {
		static readonly char[] whitespaceChars = " \t\r\n".ToCharArray();	//TODO: More chars
		public static IEnumerable<BoundedWord> GetWords(string text, IEnumerable<TextLine> lines) {
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

					var word = text.Substring(lastSpace, nextSpace - lastSpace);
					List<BoundedCharacter> letters = new List<BoundedCharacter>(word.Length);
					for (int i = 0; i < word.Length; i++) {
						var ch = UnicodeConvert.convertUnicodeChar(word, ref i);
						var letterBounds = line.GetTextBounds(i, 1)[0].Rectangle;
						letters.Add(new BoundedCharacter(ch, Rect.Offset(letterBounds, 0, top)));
					}
					var wordBounds = line.GetTextBounds(lastSpace, word.Length)[0].Rectangle;
					yield return new BoundedWord(word, Rect.Offset(wordBounds, 0, top), letters);

					lastSpace = nextSpace;
				}
				lineStart += line.Length;
				top += line.Height;
				line.Dispose();
			}
		}

		public BoundedWord(string text, Rect bounds, IList<BoundedCharacter> characters) {
			Text = text;
			Bounds = bounds;
			Characters = new ReadOnlyCollection<BoundedCharacter>(characters);
		}

		public string Text { get; private set; }
		public Rect Bounds { get; private set; }
		public ReadOnlyCollection<BoundedCharacter> Characters { get; private set; }

		public override string ToString() {
			return "Word: " + Text + ", Bounds = " + Bounds;
		}
	}
	public class BoundedCharacter {
		public BoundedCharacter(char ch, Rect bounds) {
			Character = ch;
			Bounds = bounds;
		}

		public char Character { get; private set; }
		public Rect Bounds { get; private set; }

		public override string ToString() {
			return "Character: " + Character + ", Bounds = " + Bounds;
		}
	}
}
