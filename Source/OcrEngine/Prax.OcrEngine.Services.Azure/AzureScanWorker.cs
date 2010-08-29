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

				if (document.State != DocumentState.ScanQueued)
					return;		//TODO: Log (How can this happen?)

				storage.SetState(document.Id, DocumentState.Scanning);

				queue.DeleteMessage(message);

				using (var reporter = new ProgressReporter(storage, document.Id)) {
					var processor = processorCreator();
					reporter.StartReporter();

					processor.ProgressChanged += (sender, e) => reporter.SetProgress(processor.ProgressPercentage());

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

			//TODO: Check cancellation

			readonly AutoResetEvent notifier = new AutoResetEvent(false);
			volatile int currentProgress;

			int lastProgress = 01;
			public void SetProgress(int progress) {
				currentProgress = progress;
				notifier.Set();
			}

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
					notifier.WaitOne();
					var progress = currentProgress;	//Read the volatile cross-thread field

					if (progress < 0) {
						reporterStoppedEvent.Set();
						return;
					}
					if (lastProgress == progress) continue;
					lastProgress = progress;

					storage.SetScanProgress(documentId, progress);
				}
			}

			public void Dispose() { notifier.Dispose(); reporterStoppedEvent.Dispose(); }
		}
	}
}
