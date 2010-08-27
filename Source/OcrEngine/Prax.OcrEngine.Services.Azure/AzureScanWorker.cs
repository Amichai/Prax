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
		readonly CloudQueue queue;
		public AzureScanWorker(IStorageClient storage, Func<IDocumentProcessor> processorCreator, CloudStorageAccount account) {
			this.storage = storage;
			this.processorCreator = processorCreator;

			var client = account.CreateCloudQueueClient();
			queue = client.GetQueueReference("documents");
			queue.CreateIfNotExist();
		}

		///<summary>Runs the scan worker.</summary>
		///<remarks>This method will execute indefinitely.</remarks>
		public void RunWorker() {
			while (true) {
				var message = queue.GetMessage();

				while (message == null) {
					Thread.Sleep(TimeSpan.FromSeconds(5));
					message = queue.GetMessage();
				}

				var document = storage.GetDocument(Utils.ParseName(message.AsString));

				//Queue handlers must be idempotent.
				//TODO: Handle race condition
				if (document.State != DocumentState.ScanQueued) return;
				storage.SetState(document.Id, DocumentState.Scanning);

				queue.DeleteMessage(message);

				var processor = processorCreator();
				processor.ProgressChanged += (sender, e) => {
					ThreadPool.QueueUserWorkItem(delegate {
						storage.SetScanProgress(document.Id, processor.ProgressPercentage());
					});
				};
				processor.ProcessDocument(document.OpenRead());
				storage.SetState(document.Id, DocumentState.Scanned);
			}
		}
	}
}
