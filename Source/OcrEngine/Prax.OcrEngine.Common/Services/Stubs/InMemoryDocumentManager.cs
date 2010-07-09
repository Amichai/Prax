using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Prax.OcrEngine.Services.Stubs {
	///<summary>A fake DocumentManager that stores all documents in memory.</summary>
	public class InMemoryDocumentManager : IDocumentManager {
		readonly List<InMemoryDocument> documents = new List<InMemoryDocument>();

		readonly Func<IDocumentProcessor> ProcessorCreator;
		public InMemoryDocumentManager(Guid userId, Func<IDocumentProcessor> processorCreator) {
			if (processorCreator == null) throw new ArgumentNullException("processorCreator");
			ProcessorCreator = processorCreator;

			UserId = userId;
		}

		public Guid UserId { get; private set; }
		public void UploadDocument(string name, Stream document, long length) {
			byte[] bytes = new byte[length];
			document.Read(bytes, 0, bytes.Length);	//TODO: ReadFill extension
			var doc = new InMemoryDocument(bytes) {
				Name = name
			};
			documents.Add(doc);

			ThreadPool.QueueUserWorkItem(delegate { DoProcess(doc); });
		}

		private void DoProcess(InMemoryDocument doc) {
			var processor = ProcessorCreator();
			doc.State = DocumentState.Scanning;
			doc.SetProcessor(processor);
			processor.ProcessDocument(doc.Read());
			doc.State = DocumentState.Scanned;
		}

		public IEnumerable<Document> GetDocuments() {
			return documents.Select(s => s);	//Prevent modify-by-cast
		}

		public Document GetDocument(Guid id) {
			return documents.SingleOrDefault(d => d.Id == id);
		}

		public void DeleteDocument(Guid id) {
			documents.RemoveAll(d => d.Id == id);
		}
	}
	class InMemoryDocument : Document {
		readonly byte[] bytes;
		public InMemoryDocument(byte[] bytes)
			: base(Guid.NewGuid()) {
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

		public override MemoryStream Read() { return new MemoryStream(bytes, false); }
	}
}
