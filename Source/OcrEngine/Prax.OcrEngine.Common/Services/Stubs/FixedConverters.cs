using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Services.Stubs {
	///<summary>An IResultConverter that generates text files containing a hard-coded string.</summary>
	public class FixedTextConverter : IResultsConverter {
		public FixedTextConverter(string text) { bytes = Encoding.ASCII.GetBytes(text); }

		readonly byte[] bytes;

		public ResultFormat OutputFormat { get { return ResultFormat.PlainText; } }

		public Stream Convert(Stream input, ReadOnlyCollection<RecognizedSegment> results) {
			return new MemoryStream(bytes, writable: false);
		}
	}
	///<summary>An IResultsConverter that generates empty PDF files.</summary>
	public class EmptyPdfConverter : IResultsConverter {
		public ResultFormat OutputFormat { get { return ResultFormat.Pdf; } }

		public Stream Convert(Stream input, ReadOnlyCollection<RecognizedSegment> results) {
			return new MemoryStream(new byte[0]);
		}
	}
}
