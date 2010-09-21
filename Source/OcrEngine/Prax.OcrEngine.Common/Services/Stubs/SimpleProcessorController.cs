using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace Prax.OcrEngine.Services.Stubs {
	///<summary>An IProcessorController implementation that processes documents locally.</summary>
	public class SimpleProcessorController : IProcessorController {
		public IStorageClient StorageClient { get; private set; }
		public Func<IDocumentProcessor> ProcessorCreator { get; private set; }
		public IEnumerable<IResultsConverter> ResultConverters { get; private set; }

		readonly ConcurrentDictionary<DocumentIdentifier, bool> canceledDocuments = new ConcurrentDictionary<DocumentIdentifier, bool>();
		public SimpleProcessorController(IStorageClient storageClient, Func<IDocumentProcessor> processorCreator, IEnumerable<IResultsConverter> resultConverters) {
			if (storageClient == null) throw new ArgumentNullException("storageClient");
			if (processorCreator == null) throw new ArgumentNullException("processorCreator");

			StorageClient = storageClient;
			ProcessorCreator = processorCreator;
			ResultConverters = resultConverters;
		}

		public void BeginProcessing(DocumentIdentifier id) {
			ThreadPool.QueueUserWorkItem(delegate { DoProcess(StorageClient.GetDocument(id)); });
		}

		private void DoProcess(Document doc) {
			var processor = ProcessorCreator();
			doc.State = DocumentState.Scanning;
			doc.ScanProgress = 0;
			StorageClient.UpdateDocument(doc);

			processor.CheckCanceled += (sender, e) => e.Cancel = canceledDocuments.ContainsKey(doc.Id);
			processor.ProgressChanged += delegate {
				doc = StorageClient.GetDocument(doc.Id);
				if (doc == null) return;
				doc.ScanProgress = processor.ProgressPercentage();
				StorageClient.UpdateDocument(doc);
			};

			//TODO: Cache document stream
			processor.ProcessDocument(doc.OpenRead());

			doc = StorageClient.GetDocument(doc.Id);
			foreach (var converter in ResultConverters) {
				var stream = converter.Convert(doc.OpenRead(), processor.Results);
				doc.UploadStream(converter.OutputFormat.ToString(), stream, stream.Length);
			}

			doc.State = DocumentState.Scanned;
			doc.ScanProgress = 100;
			StorageClient.UpdateDocument(doc);
		}

		public void CancelProcessing(DocumentIdentifier id) {
			canceledDocuments.AddOrUpdate(id, true, (otherId, v) => true);
		}
	}
}
