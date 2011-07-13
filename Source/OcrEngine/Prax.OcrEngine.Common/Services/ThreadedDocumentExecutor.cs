using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SLaks.Progression;
using SLaks.Progression.Display;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading;

namespace Prax.OcrEngine.Services {
	///<summary>An IDocumentExecutor that reports progress in the background.</summary>
	public class ThreadedDocumentExecutor : IDocumentExecutor {
		public IStorageClient StorageClient { get; private set; }
		public IDocumentRecognizer Recognizer { get; private set; }
		public IEnumerable<IResultsConverter> ResultConverters { get; private set; }

		public ThreadedDocumentExecutor(IStorageClient storageClient, IDocumentRecognizer recognizer, IEnumerable<IResultsConverter> resultConverters) {
			if (storageClient == null) throw new ArgumentNullException("storageClient");
			if (recognizer == null) throw new ArgumentNullException("recognizer");

			StorageClient = storageClient;
			Recognizer = recognizer;
			ResultConverters = resultConverters;
		}


		public void Execute(DocumentIdentifier id) {
			try {
				using (var reporter = new AsyncProgressReporter(StorageClient, id)) {
					reporter.StartReporter();

					var document = StorageClient.GetDocument(id);
					if (document == null) return;
					document.State = DocumentState.Scanning;
					document.ScanProgress = 0;
					StorageClient.UpdateDocument(document);

					var stream = new MemoryStream();
					using (var source = document.OpenRead())
						source.CopyTo(stream);
					var results = new ReadOnlyCollection<RecognizedSegment>(
						Recognizer.Recognize(stream, reporter).ToList()
					);
					reporter.StopReporter();

					document = StorageClient.GetDocument(document.Id);	//Refresh properties before saving them (eg, if it was renamed)
					if (document == null) return;

					foreach (var converter in ResultConverters) {
						var convertedStream = converter.Convert(document.OpenRead(), results);
						document.UploadStream(converter.OutputFormat.ToString(), convertedStream, convertedStream.Length);
					}

					document.ScanProgress = 100;
					document.State = DocumentState.Scanned;
					StorageClient.UpdateDocument(document);
				}
			} catch (Exception ex) {
				var document = StorageClient.GetDocument(id);
				if (document == null) return;

				document.State = DocumentState.Error;
				document.UploadString("Error", ex.ToString());
				StorageClient.UpdateDocument(document);
			}
		}

		public void CancelProcessing(DocumentIdentifier id) { }

		///<summary>Reports scan progress on a separate thread.</summary>
		///<remarks>Since updating the scan progress involves making network
		///calls, this class updates the progress on a separate thread.  The
		///reporter thread is managed internally; all public methods in this
		///class must be called on the primary thread.</remarks>
		sealed class AsyncProgressReporter : ScaledProgressReporter, IProgressReporter, IDisposable {
			readonly IStorageClient storage;
			readonly DocumentIdentifier documentId;
			public AsyncProgressReporter(IStorageClient storage, DocumentIdentifier documentId) { this.storage = storage; this.documentId = documentId; }

			readonly AutoResetEvent notifier = new AutoResetEvent(false);
			volatile int currentProgress;

			int lastProgress = 0;

			volatile bool cancellationPending;

			//Since the scan will usually take a long time, this isn't a ManualResetEventSlim.
			readonly ManualResetEvent reporterStoppedEvent = new ManualResetEvent(false);

			///<summary>Launches the reporter thread.</summary>
			///<remarks>This is an asynchronous method that creates a new thread.</remarks>
			public void StartReporter() {
				ThreadPool.QueueUserWorkItem(delegate { RunReporter(); });
			}

			protected override int ScaledMax { get { return 100; } }
			protected override void UpdateBar(int? oldValue, int? newValue) {
				currentProgress = newValue ?? 0;
				notifier.Set();
			}

			///<summary>Stops the reporter thread.</summary>
			///<remarks>This is a synchronous method that only returns after the reporter thread exits.
			///By blocking until the reporter stops, I avoid race conditions where the reporter sets the 
			///progress after the processor thread sets the state to Scanned.</remarks>
			public void StopReporter() {
				currentProgress = -1;
				notifier.Set();

				while (true) {
					if (reporterStoppedEvent.WaitOne(TimeSpan.FromSeconds(.5)))
						return;	//If the event was signaled, stop.
					//If the event was not signaled, set the notifier again.
					notifier.Set();
				}
			}

			///<summary>Runs the reporter listener synchronously.</summary>
			///<remarks>This method runs until StopReporter() is called.
			///If the document is canceled, we continue running until the
			///recognizer actually finishes.</remarks>
			void RunReporter() {
				while (true) {
					//Check for cancellation at  intervals.
					//In addition, if the progress changes,
					//update the progress immediately.
					notifier.WaitOne(TimeSpan.FromSeconds(15));	//TODO: Increase timeout?

					var progress = currentProgress;	//Read the volatile cross-thread field

					if (progress < 0)
						break;

					var doc = storage.GetDocument(documentId);	//Refresh the document from storage
					if (!cancellationPending) {
						if (doc == null || doc.CancellationPending)
							cancellationPending = true;
						if (doc == null)
							break;			//If the document was deleted, it is impossible to report progress, so we exit early.
					}

					if (lastProgress == progress) continue;
					lastProgress = progress;

					doc.ScanProgress = progress;
					storage.UpdateDocument(doc);
				}
				reporterStoppedEvent.Set();
			}

			public void Dispose() { notifier.Dispose(); reporterStoppedEvent.Dispose(); }

			public bool AllowCancellation { get { return true; } set { } }
			public string Caption { get; set; }
			public bool WasCanceled { get { return cancellationPending; } }
		}
	}
}
