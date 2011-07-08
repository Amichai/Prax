using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Segmentation;
using Prax.OcrEngine.Engine.HeuristicGeneration;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Engine.AutomatedTraining {
	class RenderedText {
		public RenderedText(string text, List<TextSegment> characterSegments) {
			this.Text = text;
			this.Words = new ReadOnlyCollection<string>(text.Split(' '));

			this.WordBounds = new ReadOnlyCollection<CharacterBounds>(
				Words.Select(w => new CharacterBounds(characterSegments, w)).ToList()
			);
		}
		public ReadOnlyCollection<string> Words { get; private set; }
		public string Text { get; private set; }
		public ReadOnlyCollection<CharacterBounds> WordBounds { get; private set; }
	}
}
