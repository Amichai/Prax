using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Services.Azure {
	///<summary>Controls document processors on Azure worker nodes.</summary>
	public class AzureProcessorController : IProcessorController {
		public void BeginProcessing(DocumentIdentifier id) {
			throw new NotImplementedException();
		}

		public void CancelProcessing(DocumentIdentifier id) {
			throw new NotImplementedException();
		}
	}
}
