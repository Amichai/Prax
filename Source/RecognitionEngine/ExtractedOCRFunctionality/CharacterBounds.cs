using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows;
using Prax.Recognition;

namespace ExtractedOCRFunctionality {
	public class CharacterBounds {
		public List<Tuple<Rectangle, string>> bounds { private set; get; }
		public void AddBound(Rectangle bound, string charString){
			bounds.Add(new Tuple<Rectangle, string>(bound, charString));
		}

		public CharacterBounds(List<TextSegment> words){
			bounds = new List<Tuple<Rectangle, string>>();
			foreach (var word in words) {
				var r = word.Bounds;
				bounds.Add(new Tuple<Rectangle, string>(asRectangle(r), word.Text));
			}
		}
		
		private Rectangle asRectangle(Rect r){
			return new Rectangle((int)Math.Round(r.X), (int)Math.Round(r.Y), (int)Math.Round(r.Width), (int)Math.Round(r.Height));
		}

		public void DrawOnImage(Bitmap bitmap) {
			foreach (var bound in bounds) {
				GraphicsHelper.DrawBounds(bound.Item1, bitmap);
			}
		}
	}
}
