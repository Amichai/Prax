using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Prax.OcrEngine.Services.Stubs {
	///<summary>A basic IStorageClient implementation that stores documents in memory.</summary>
	public class InMemoryStorage : IStorageClient {
		readonly List<InMemoryDocument> list = new List<InMemoryDocument>();

		public Guid UploadDocument(Guid userId, string name, Stream document, long length) {
			byte[] bytes = new byte[length];
			document.ReadFill(bytes);
			var doc = new InMemoryDocument(userId, bytes) { Name = name };

			lock (list)
				list.Add(doc);
			return doc.Id.DocumentId;
		}

		public IEnumerable<Document> GetDocuments(Guid userId) {
			lock (list)
				return list.Where(d => d.Id.UserId == userId);
		}

		public Document GetDocument(DocumentIdentifier id) {
			lock (list)
				return list.Single(d => d.Id == id);
		}

		public void DeleteDocument(DocumentIdentifier id) {
			lock (list)
				list.RemoveAll(d => d.Id == id);
		}

		public void SetScanProgress(DocumentIdentifier id, int progress) {
			var doc = GetDocument(id);
			doc.ScanProgress = progress;
			doc.State = DocumentState.Scanning;
		}

		public void SetState(DocumentIdentifier id, DocumentState state) {
			GetDocument(id).State = state;
		}

		class InMemoryDocument : Document {
			readonly byte[] bytes;
			public InMemoryDocument(Guid userId, byte[] bytes)
				: base(new DocumentIdentifier(userId, Guid.NewGuid())) {
				this.bytes = bytes;
				Length = bytes.Length;
				DateUploaded = DateTime.Now;
			}

			public override MemoryStream Read() { return new MemoryStream(bytes, false); }
		}
	}
}
