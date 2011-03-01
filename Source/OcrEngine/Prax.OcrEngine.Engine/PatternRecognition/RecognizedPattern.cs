using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Prax.OcrEngine.Engine.PatternRecognition {
	///<summary>The result of a pattern recognizer invocation.</summary>
	[ImmutableObject(true)]
	public class RecognizedPattern {
		///<summary>Creates a RecognizedPattern value.</summary>
		///<param name="label">The label that the data was recognized as.</param>
		///<param name="certainty">The probability that the recognition is correct, between 0 and 1.</param>
		public RecognizedPattern(string label, double certainty) {
			if (certainty < 0 || certainty > 1)
				throw new ArgumentOutOfRangeException("certainty", "Certainty must be between 0 and 1.");
			Label = label;
			Certainty = certainty;
		}

		///<summary>Gets the label that the data was recognized as.</summary>
		public string Label { get; private set; }
		///<summary>Gets the probability that the recognition is correct, between 0 and 1.</summary>
		public double Certainty { get; private set; }
	}
}
