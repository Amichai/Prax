using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Segmentation;
using Prax.OcrEngine.Engine.HeuristicGeneration;

namespace Prax.OcrEngine.Engine.AutomatedTraining {
	class RenderedText {
		public RenderedText(string text, List<TextSegment> characterSegments) {
			this.Text = text;
			this.Words = text.Split(' ');
			foreach (var word in Words) {
				wordBounds.Add(new CharacterBounds(characterSegments, word));
			}
		}
		public string[] Words;
		public string Text;
		public List<CharacterBounds> wordBounds = new List<CharacterBounds>();

		public TrainingData ProduceTrainingData(IterateBoards boards) {
			var trainingData = new TrainingData();
			foreach (var word in wordBounds) {
				trainingData = boards.Train(word);
			}
			return trainingData;
		}
	}
}
