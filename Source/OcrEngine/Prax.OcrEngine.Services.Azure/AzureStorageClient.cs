using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Services.Azure {
	///<summary>An IStorageClient implementation that uses Azure blob storage.</summary>
	public class AzureStorageClient : IStorageClient {
		public Guid UploadDocument(Guid userId, string name, System.IO.Stream document, long length) {
			throw new NotImplementedException();
		}

		public IEnumerable<Document> GetDocuments(Guid userId) {
			throw new NotImplementedException();
		}

		public Document GetDocument(DocumentIdentifier id) {
			throw new NotImplementedException();
		}

		public void DeleteDocument(DocumentIdentifier id) {
			throw new NotImplementedException();
		}

		public void SetScanProgress(DocumentIdentifier id, int progress) {
			throw new NotImplementedException();
		}

		public void SetState(DocumentIdentifier id, DocumentState state) {
			throw new NotImplementedException();
		}
	}
}
