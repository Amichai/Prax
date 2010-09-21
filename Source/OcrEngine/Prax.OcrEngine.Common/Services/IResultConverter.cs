using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Services {
	///<summary>Converts OCR results into an output format.</summary>
	public interface IResultsConverter {
		///<summary>Gets the format that this service converts to.</summary>
		ResultFormat OutputFormat { get; }
		///<summary>Converts a recognized image.</summary>
		Stream Convert(Stream input, ReadOnlyCollection<RecognizedSegment> results);
	}

	///<summary>Contains formats that OCR results can be converted to.</summary>
	///<remarks>File extensions are defined in the Extensions class.</remarks>
	public enum ResultFormat {
		///<summary>A plain text file.</summary>
		PlainText,
		///<summary>A PDF document.</summary>
		Pdf
	}
}
