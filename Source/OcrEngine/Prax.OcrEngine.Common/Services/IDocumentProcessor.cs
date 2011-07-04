﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Services {
	///<summary>Performs OCR processing.</summary>
	///<remarks>When implementing this interface, you should inherit the DocumentProcessorBase class to get basic logic.</remarks>
	public interface IDocumentProcessor {
		//Amichai: Your main class should implement this interface.
		//Please modify its public methods as appropriate.  We will
		//also create a dummy implementation of the interface which
		//does no actual work but takes a long time.

		//TODO: What types should this method take and return?
		///<summary>Processes a document.</summary>
		///<exception cref="System.InvalidOperationException">Thrown if this instance is already processing a different document.</exception>
		void ProcessDocument(Stream document);


		///<summary>Gets the current progress of the operation.</summary>
		int CurrentProgress { get; }
		///<summary>Gets the maximum progress of the operation.</summary>
		///<remarks>If CurrentProgress is equal to MaximumProgress, the operation has finished.</remarks>
		int MaximumProgress { get; }

		///<summary>Raised when the progress of the operation changes.</summary>
		event EventHandler ProgressChanged;
		///<summary>Raised to check whether the operation should be canceled.</summary>
		event CancelEventHandler CheckCanceled;

		//TODO: Add classes or interfaces to Common
		//that store the OCR's results, then expose
		//them as properties in this interface.
		///<summary>Gets the results of the recognition.</summary>
		ReadOnlyCollection<RecognizedSegment> Results { get; }
	}

	///<summary>Describes a single string recognized in an image.</summary>
	public struct RecognizedSegment {
		///<summary>Creates a RecognizedSegment value.</summary>
		public RecognizedSegment(Rectangle bounds, string text, double certainty)
			: this() {
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

		//TODO: Angle?

		//TODO: Equals, GetHashCode, == operator
	}
}
