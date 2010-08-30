using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Threading;

namespace Prax.OcrEngine.Services.Azure {
	///<summary>Processes documents in an Azure queue.</summary>
	public class AzureScanWorker {
		//TODO: Multiple queues

		readonly IStorageClient storage;
		readonly Func<IDocumentProcessor> processorCreator;
		readonly SingleDeliveryQueueClient queue;

		public AzureScanWorker(IStorageClient storage, Func<IDocumentProcessor> processorCreator, CloudStorageAccount account) {
			this.storage = storage;
			this.processorCreator = processorCreator;
			queue = new SingleDeliveryQueueClient(account, "documents");
		}

		///<summary>Runs the scan worker.</summary>
		///<remarks>This method will execute indefinitely.</remarks>
		public void RunWorker() {
			while (true) {
				var message = queue.GetMessage();

				var document = storage.GetDocument(Utils.ParseName(message.AsString));
				if (document == null) {	//If the document was deleted while still in the queue, skip it.
					queue.DeleteMessage(message);
					return;
				}

				if (document.State != DocumentState.ScanQueued)
					return;		//TODO: Log (How can this happen?)
				if (document.CancellationPending) {
					//TODO: Set state
					queue.DeleteMessage(message);
					return;
				}

				storage.SetState(document.Id, DocumentState.Scanning);

				queue.DeleteMessage(message);

				using (var reporter = new ProgressReporter(storage, document.Id)) {
					var processor = processorCreator();
					reporter.StartReporter();

					processor.ProgressChanged += (sender, e) => reporter.SetProgress(processor.ProgressPercentage());
					processor.CheckCanceled += (sender, e) => e.Cancel = reporter.CancellationPending;

					processor.ProcessDocument(document.OpenRead());
					reporter.StopReporter();
					storage.SetState(document.Id, DocumentState.Scanned);
				}
			}
		}

		///<summary>Reports scan progress on a separate thread.</summary>
		///<remarks>Since updating the scan progress involves making network
		///calls, this class updates the progress on a separate thread.  The
		///reporter thread is managed internally; all public methods in this
		///class must be called on the primary thread.</remarks>
		sealed class ProgressReporter : IDisposable {
			readonly IStorageClient storage;
			readonly DocumentIdentifier documentId;
			public ProgressReporter(IStorageClient storage, DocumentIdentifier documentId) { this.storage = storage; this.documentId = documentId; }

			readonly AutoResetEvent notifier = new AutoResetEvent(false);
			volatile int currentProgress;

			int lastProgress = 01;
			public void SetProgress(int progress) {
				currentProgress = progress;
				notifier.Set();
			}

			public volatile bool CancellationPending;

			//Since the scan will usually take a long time, this isn't a ManualResetEventSlim.
			readonly ManualResetEvent reporterStoppedEvent = new ManualResetEvent(false);

			///<summary>Launches the reporter thread.</summary>
			///<remarks>This is an asynchronous method that creates a new thread.</remarks>
			public void StartReporter() {
				ThreadPool.QueueUserWorkItem(delegate { RunReporter(); });
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
			void RunReporter() {
				while (true) {
					//Check for cancellation at  intervals.
					//In addition, if the progress changes,
					//update the progress immediately.
					notifier.WaitOne(TimeSpan.FromSeconds(15));	//TODO: Increase timeout?

					var progress = currentProgress;	//Read the volatile cross-thread field

					if (progress < 0)
						break;
					//TODO: Reduce storage calls.  We end up calling GetMetadata twice.
					if (!CancellationPending) {
						var doc = storage.GetDocument(documentId);
						if (doc == null || doc.CancellationPending)
							CancellationPending = true;
						if (doc == null) 
							break;			//If the document was deleted, there's no point in reporting progress.
					}

					if (lastProgress == progress) continue;
					lastProgress = progress;

					storage.SetScanProgress(documentId, progress);
				}
				reporterStoppedEvent.Set();
			}

			public void Dispose() { notifier.Dispose(); reporterStoppedEvent.Dispose(); }
		}
	}
}
