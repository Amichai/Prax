using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace Prax.OcrEngine.Services.Stubs {
	///<summary>An IProcessorController implementation that processes documents locally.</summary>
	public class SimpleProcessorController : IProcessorController {
		public IStorageClient StorageClient { get; private set; }
		public Func<IDocumentProcessor> ProcessorCreator { get; private set; }

		readonly ConcurrentDictionary<DocumentIdentifier, bool> canceledDocuments = new ConcurrentDictionary<DocumentIdentifier, bool>();
		public SimpleProcessorController(IStorageClient storageClient, Func<IDocumentProcessor> processorCreator) {
			if (storageClient == null) throw new ArgumentNullException("storageClient");
			if (processorCreator == null) throw new ArgumentNullException("processorCreator");

			StorageClient = storageClient;
			ProcessorCreator = processorCreator;
		}

		public void BeginProcessing(DocumentIdentifier id) {
			ThreadPool.QueueUserWorkItem(delegate { DoProcess(StorageClient.GetDocument(id)); });
		}

		private void DoProcess(Document doc) {
			var processor = ProcessorCreator();
			StorageClient.SetState(doc.Id, DocumentState.Scanning);

			processor.CheckCanceled += (sender, e) => e.Cancel = canceledDocuments.ContainsKey(doc.Id);
			processor.ProgressChanged += delegate { StorageClient.SetScanProgress(doc.Id, processor.ProgressPercentage()); };
			//In the Azure worker, ProgressChanged will update the blob metadata on a worker thread.

			processor.ProcessDocument(doc.Read());

			StorageClient.SetState(doc.Id, DocumentState.Scanned);
		}

		public void CancelProcessing(DocumentIdentifier id) {
			canceledDocuments.AddOrUpdate(id, true, (otherId, v) => true);
		}
	}
}
