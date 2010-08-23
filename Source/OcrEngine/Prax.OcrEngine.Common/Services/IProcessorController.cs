using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Services {
	///<summary>Controls document processors, possibly on remote computers.</summary>
	public interface IProcessorController {
		///<summary>Causes the document with the given ID to start processing.</summary>
		void BeginProcessing(DocumentIdentifier id);

		///<summary>Cancels the document with the given ID.</summary>
		void CancelProcessing(DocumentIdentifier id);
	}
}
