using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Prax.OcrEngine.Services.Stubs {
	///<summary>A fake legacy DocumentManager that stores all documents in memory.</summary>
	///<remarks>This class pre-dates the IStorageClient and IProcessorController interfaces and should be avoided.</remarks>
	public class InMemoryDocumentManager : IDocumentManager {
		readonly List<InMemoryDocument> documents = new List<InMemoryDocument>();

		readonly Func<IDocumentProcessor> ProcessorCreator;
		public InMemoryDocumentManager(Guid userId, Func<IDocumentProcessor> processorCreator) {
			if (processorCreator == null) throw new ArgumentNullException("processorCreator");
			ProcessorCreator = processorCreator;

			UserId = userId;
		}

		public Guid UserId { get; private set; }
		public Guid UploadDocument(string name, Stream document, long length) {
			byte[] bytes = new byte[length];
			document.ReadFill(bytes);
			var doc = new InMemoryDocument(UserId, bytes) {
				Name = name
			};
			documents.Add(doc);

			ThreadPool.QueueUserWorkItem(delegate { DoProcess(doc); });
			return doc.Id.DocumentId;
		}

		private void DoProcess(InMemoryDocument doc) {
			var processor = ProcessorCreator();
			doc.State = DocumentState.Scanning;
			doc.SetProcessor(processor);
			processor.ProcessDocument(doc.OpenRead());
			doc.State = DocumentState.Scanned;
		}

		public IEnumerable<Document> GetDocuments() {
			return documents.Select(s => s);	//Prevent modify-by-cast
		}

		public Document GetDocument(Guid id) {
			return documents.SingleOrDefault(d => d.Id.DocumentId == id);
		}

		public void DeleteDocument(Guid id) {
			documents.RemoveAll(d => d.Id.DocumentId == id);
		}
		class InMemoryDocument : Document {
			readonly byte[] bytes;
			public InMemoryDocument(Guid userId, byte[] bytes)
				: base(new DocumentIdentifier(userId, Guid.NewGuid())) {
				this.bytes = bytes;
				Length = bytes.Length;
				DateUploaded = DateTime.Now;
			}

			IDocumentProcessor processor;
			internal void SetProcessor(IDocumentProcessor proc) {
				processor = proc;
			}

			public override int ScanProgress {
				get {
					if (processor == null) return 0;
					if (State == DocumentState.Scanned) return 100;
					return (int)(100 * (double)processor.CurrentProgress / processor.MaximumProgress);
				}
				set { }
			}

			public override Stream OpenRead() { return new MemoryStream(bytes, false); }
		}
	}
}
