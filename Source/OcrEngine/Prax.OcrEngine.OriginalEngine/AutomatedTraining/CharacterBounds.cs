using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows;
using Segmentation;
using Prax.OcrEngine.Engine.ImageUtilities;

namespace Prax.OcrEngine.Engine.AutomatedTraining {
	public class CharacterBounds {
		public string Word;
		public int LetterLocation;
		public List<LetterAndBounds> items { private set; get; }

		public CharacterBounds(List<TextSegment> words, string originalText){
			Word = originalText;
			items = new List<LetterAndBounds>();
			
			foreach (var word in words) {
				var r = word.Bounds;
				items.Add(new LetterAndBounds(word.Text, word.LetterIndexValue, asRectangle(r)));
			}
		}
		
		private Rectangle asRectangle(Rect r){
			return new Rectangle((int)Math.Round(r.X), (int)Math.Round(r.Y), (int)Math.Round(r.Width), (int)Math.Round(r.Height));
			
		}

		public void DrawOnImage(Bitmap bitmap) {
			foreach (var bound in items) {
				bitmap.DrawBounds(bound.Bounds);
			}
		}
	}
	public class LetterAndBounds {
		public string Letter;
		public int IndexLocation;
		public string Word;
		public Rectangle Bounds;
		public LetterAndBounds(string letter, int index, Rectangle bounds) {
			this.Letter = letter;
			this.IndexLocation = index;
			//this.Word = word;
			this.Bounds = bounds;
		}
	}
}
