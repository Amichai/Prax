using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Collections.ObjectModel;
using SLaks.Progression;

namespace Prax.OcrEngine.Services {
	///<summary>Performs OCR processing.</summary>
	///<remarks>Implementations of this interface should be stateless.</remarks>
	public interface IDocumentRecognizer {
		///<summary>Recognizes the text in a document.</summary>
		IEnumerable<RecognizedSegment> Recognize(Stream document, IProgressReporter progress);
	}

	///<summary>Describes a single string recognized in an image.</summary>
	public class RecognizedSegment {
		///<summary>Creates a RecognizedSegment value.</summary>
		public RecognizedSegment(Rectangle bounds, string text, double certainty) {
			Bounds = bounds;
			Text = text;
			Certainty = certainty;
		}

		///<summary>Gets the area in the image that contains the string.</summary>
		public Rectangle Bounds { get; private set; }
		///<summary>Gets the recognized text.</summary>
		public string Text { get; private set; }
		///<summary>Gets the certainty of the recognition, between 0 and 1.</summary>
		public double Certainty { get; private set; }
	}
}
