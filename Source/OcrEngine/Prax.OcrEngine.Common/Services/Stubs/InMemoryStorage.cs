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
			var doc = new InMemoryDocument(userId, name, bytes);

			lock (list)
				list.Add(doc);
			return doc.Id.DocumentId;
		}

		public IEnumerable<Document> GetDocuments(Guid userId) {
			lock (list)
				return list.Where(d => d.Id.UserId == userId);
		}

		///<summary>Finds the document with the given ID.</summary>
		///<remarks>Must be called inside lock(list).</remarks>
		InMemoryDocument FindDoc(DocumentIdentifier id) { return list.SingleOrDefault(d => d.Id == id); }
		public Document GetDocument(DocumentIdentifier id) {
			lock (list)
				return FindDoc(id);
		}

		public void DeleteDocument(DocumentIdentifier id) {
			lock (list)
				list.RemoveAll(d => d.Id == id);
		}

		public void SetScanProgress(DocumentIdentifier id, int progress) {
			InMemoryDocument doc;
			lock (list)
				doc = FindDoc(id);
			if (doc != null)
				doc.SetScanProgress(progress);
		}

		public void SetState(DocumentIdentifier id, DocumentState state) {
			InMemoryDocument doc;
			lock (list)
				doc = FindDoc(id);
			if (doc != null)
				doc.SetState(state);
		}

		class InMemoryDocument : Document {
			readonly byte[] bytes;
			public InMemoryDocument(Guid userId, string name, byte[] bytes)
				: base(new DocumentIdentifier(userId, Guid.NewGuid())) {
				base.Name = name;
				this.bytes = bytes;
				Length = bytes.Length;
				DateUploaded = DateTime.UtcNow;
			}

			public void SetScanProgress(int progress) { base.ScanProgress = progress; base.State = DocumentState.Scanning; }
			public void SetState(DocumentState state) { base.State = state; }

			public override Stream OpenRead() { return new MemoryStream(bytes, false); }
		}
	}
}
