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
				return list.Where(d => d.Id.UserId == userId).Select(d => d.Clone());
		}

		///<summary>Finds the document with the given ID.</summary>
		///<remarks>Must be called inside lock(list).</remarks>
		InMemoryDocument FindDoc(DocumentIdentifier id) { return list.SingleOrDefault(d => d.Id == id); }
		public Document GetDocument(DocumentIdentifier id) {
			InMemoryDocument doc;
			lock (list)
				doc = FindDoc(id);
			return doc == null ? null : doc.Clone();
		}

		public void DeleteDocument(DocumentIdentifier id) {
			lock (list)
				list.RemoveAll(d => d.Id == id);
		}

		public bool UpdateDocument(Document doc) {
			lock (list) {
				var index = list.FindIndex(d => d.Id == doc.Id);
				if (index < 0)
					return false;

				list[index] = ((InMemoryDocument)doc).Clone();
				return true;
			}
		}
		class InMemoryDocument : Document {
			readonly ConcurrentDictionary<string, byte[]> dataStreams = new ConcurrentDictionary<string, byte[]>();
			readonly byte[] bytes;

			public InMemoryDocument(Guid userId, string name, byte[] bytes)
				: base(new DocumentIdentifier(userId, Guid.NewGuid())) {
				base.Name = name;
				this.bytes = bytes;
				Length = bytes.Length;
				DateUploaded = DateTime.UtcNow;
			}

			public InMemoryDocument Clone() { return (InMemoryDocument)MemberwiseClone(); }

			public override Stream OpenRead() { return new MemoryStream(bytes, false); }

			public override Stream OpenStream(string name) {
				return new MemoryStream(dataStreams[name], false);
			}

			public override void UploadStream(string name, Stream stream, long length) {
				var bytes = new byte[length];
				stream.ReadFill(bytes);
				dataStreams.AddOrUpdate(name, bytes, (k, v) => bytes);
			}
		}
	}
}
