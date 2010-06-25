using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Prax.OcrEngine.Common {
	///<summary>Performs OCR processing.</summary>
	public interface IDocumentProcessor {
		//Amichai: Your main class should implement this interface.
		//Please modify its public methods as appropiate.  We will 
		//also create a dummy implementation of the interface which
		//does no actual work but takes a long time.

		///<summary>Indicates whether this instance is currently processing a document.</summary>
		bool IsProcessing { get; }

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

		//TODO: Add classes or interfaces to Common
		//that store the OCR's results, then expose
		//them as properties in this interface.
	}
}
