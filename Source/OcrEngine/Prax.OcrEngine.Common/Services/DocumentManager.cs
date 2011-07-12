using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace Prax.OcrEngine.Services {
	///<summary>The primary IDocumentManager implementation, which forwards to IStorageClient and IProcessorController.</summary>
	public class DocumentManager : IDocumentManager {
		public IStorageClient StorageClient { get; private set; }
		public IDocumentExecutor DocumentExecutor { get; private set; }

		public DocumentManager(Guid userId, IStorageClient storageClient, IDocumentExecutor documentExecutor) {
			if (storageClient == null) throw new ArgumentNullException("storageClient");
			if (documentExecutor == null) throw new ArgumentNullException("documentExecutor");

			UserId = userId;
			StorageClient = storageClient;
			DocumentExecutor = documentExecutor;
		}

		///<summary>Gets the user ID that this instance manages.</summary>
		public Guid UserId { get; private set; }

		///<summary>Creates a DocumentIdentifier value from a document GUID for this user</summary>
		private DocumentIdentifier MakeId(Guid documentId) { return new DocumentIdentifier(UserId, documentId); }

		public Guid UploadDocument(string name, string mimeType, Stream document, long length) {
			var id = StorageClient.UploadDocument(UserId, name, mimeType, document, length);
			ThreadPool.QueueUserWorkItem(delegate { DocumentExecutor.Execute(MakeId(id)); });
			return id;
		}

		public IEnumerable<Document> GetDocuments() { return StorageClient.GetDocuments(UserId); }
		public Document GetDocument(Guid id) { return StorageClient.GetDocument(MakeId(id)); }

		public void DeleteDocument(Guid id) {
			DocumentExecutor.CancelProcessing(MakeId(id));	//TODO: Check state?
			StorageClient.DeleteDocument(MakeId(id));
		}

		public void RenameDocument(Guid id, string newName) {
			if (String.IsNullOrWhiteSpace(newName)) throw new ArgumentNullException("newName");

			var doc = StorageClient.GetDocument(MakeId(id));
			doc.Name = newName;
			StorageClient.UpdateDocument(doc);
		}
	}
}
