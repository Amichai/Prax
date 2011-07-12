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
		readonly IDocumentExecutor documentExecutor;
		readonly SingleDeliveryQueueClient queue;

		public AzureScanWorker(IStorageClient storage, IDocumentExecutor documentExecutor, CloudStorageAccount account) {
			this.storage = storage;
			this.documentExecutor = documentExecutor;
			queue = new SingleDeliveryQueueClient(account, "documents");
		}

		///<summary>Runs the scan worker.</summary>
		///<remarks>This method will execute indefinitely.</remarks>
		public void RunWorker() {
			while (true) {
				var message = queue.GetMessage();

				var document = storage.GetDocument(IdUtils.ParseName(message.AsString));
				if (document == null) {	//If the document was deleted while still in the queue, skip it.
					queue.DeleteMessage(message);
					continue;
				}

				if (document.State != DocumentState.ScanQueued)
					return;		//TODO: Log (How can this happen?)
				if (document.CancellationPending) {
					//TODO: Set state
					queue.DeleteMessage(message);
					continue;
				}

				document.State = DocumentState.Scanning;
				storage.UpdateDocument(document);

				queue.DeleteMessage(message);
				documentExecutor.Execute(document.Id);
			}
		}
	}
}