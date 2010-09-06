using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Prax.OcrEngine.Services {
	///<summary>The primary IDocumentManager implementation, which forwards to IStorageClient and IProcessorController.</summary>
	public class DocumentManager : IDocumentManager {
		public IStorageClient StorageClient { get; private set; }
		public IProcessorController ProcessorController { get; private set; }

		public DocumentManager(Guid userId, IStorageClient storageClient, IProcessorController processorController) {
			if (storageClient == null) throw new ArgumentNullException("storageClient");
			if (processorController == null) throw new ArgumentNullException("processorController");

			UserId = userId;
			StorageClient = storageClient;
			ProcessorController = processorController;
		}

		///<summary>Gets the user ID that this instance manages.</summary>
		public Guid UserId { get; private set; }

		///<summary>Creates a DocumentIdentifier value from a document GUID for this user</summary>
		private DocumentIdentifier MakeId(Guid documentId) { return new DocumentIdentifier(UserId, documentId); }

		public Guid UploadDocument(string name, Stream document, long length) {
			var id = StorageClient.UploadDocument(UserId, name, document, length);
			ProcessorController.BeginProcessing(MakeId(id));
			return id;
		}

		public IEnumerable<Document> GetDocuments() { return StorageClient.GetDocuments(UserId); }
		public Document GetDocument(Guid id) { return StorageClient.GetDocument(MakeId(id)); }

		public void DeleteDocument(Guid id) {
			ProcessorController.CancelProcessing(MakeId(id));	//TODO: Check state?
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
