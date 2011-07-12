using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SLaks.Progression;
using SLaks.Progression.Display;

namespace Prax.OcrEngine.Services.Stubs {
	///<summary>An IDocumentExecor that processes the document on a single thread.</summary>
	///<remarks>This implementation will block the processing when reporting progress.</remarks>
	public class SimpleDocumentExecutor : IDocumentExecutor {
		public IStorageClient StorageClient { get; private set; }
		public IDocumentRecognizer Recognizer { get; private set; }
		public IEnumerable<IResultsConverter> ResultConverters { get; private set; }

		readonly ConcurrentDictionary<DocumentIdentifier, bool> canceledDocuments = new ConcurrentDictionary<DocumentIdentifier, bool>();
		public SimpleDocumentExecutor(IStorageClient storageClient, IDocumentRecognizer recognizer, IEnumerable<IResultsConverter> resultConverters) {
			if (storageClient == null) throw new ArgumentNullException("storageClient");
			if (recognizer == null) throw new ArgumentNullException("recognizer");

			StorageClient = storageClient;
			Recognizer = recognizer;
			ResultConverters = resultConverters;
		}

		public void Execute(DocumentIdentifier id) {
			var doc = StorageClient.GetDocument(id);
			if (doc == null) return;
			doc.State = DocumentState.Scanning;
			doc.ScanProgress = 0;
			StorageClient.UpdateDocument(doc);

			var stream = new MemoryStream();
			using (var source = doc.OpenRead())
				source.CopyTo(stream);
			var results = new ReadOnlyCollection<RecognizedSegment>(
				Recognizer.Recognize(stream, new StorageProgressReporter(this, id)).ToList()
			);

			doc = StorageClient.GetDocument(doc.Id);
			if (doc == null) return;
			foreach (var converter in ResultConverters) {
				var convertedStream = converter.Convert(doc.OpenRead(), results);
				doc.UploadStream(converter.OutputFormat.ToString(), convertedStream, convertedStream.Length);
			}

			doc.ScanProgress = 100;
			doc.State = DocumentState.Scanned;
			StorageClient.UpdateDocument(doc);
		}
		public void CancelProcessing(DocumentIdentifier id) {
			canceledDocuments.TryAdd(id, true);
		}

		class StorageProgressReporter : ScaledProgressReporter, IProgressReporter {
			readonly SimpleDocumentExecutor executor;
			readonly DocumentIdentifier id;
			public StorageProgressReporter(SimpleDocumentExecutor executor, DocumentIdentifier id) {
				this.executor = executor;
				this.id = id;
			}

			public bool AllowCancellation { get { return true; } set { } }
			public string Caption { get; set; }
			protected override int ScaledMax { get { return 100; } }

			public bool WasCanceled { get { return executor.canceledDocuments.ContainsKey(id); } }

			protected override void UpdateBar(int? oldValue, int? newValue) {
				var doc = executor.StorageClient.GetDocument(id);
				if (doc == null || doc.CancellationPending) {
					executor.canceledDocuments.TryAdd(id, true);
					return;
				}
				doc.ScanProgress = newValue ?? 0;
				executor.StorageClient.UpdateDocument(doc);
			}
		}
	}
}
