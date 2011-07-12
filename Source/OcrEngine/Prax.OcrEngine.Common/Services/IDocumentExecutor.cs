using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Services {
	///<summary>Performs OCR processing and result conversion on a document in storage.</summary>
	public interface IDocumentExecutor {
		///<summary>OCR's a document and stores the result.</summary>
		void Execute(DocumentIdentifier id);

		///<summary>Cancels the document with the given ID.</summary>
		///<remarks>This method supports active cancellation.
		///If the processor controller cannot perform active
		///cancellation, it doesn't need to do anything here.
		///The DocumentManager will set CancellationPending 
		///on the document, supporting passive cancellation.</remarks>
		void CancelProcessing(DocumentIdentifier id);
	}
}
