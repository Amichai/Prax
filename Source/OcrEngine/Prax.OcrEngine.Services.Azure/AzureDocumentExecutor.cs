using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Prax.OcrEngine.Services.Azure {
	///<summary>Controls document processors on Azure worker nodes.</summary>
	public class AzureDocumentExecutor: IDocumentExecutor {
		//TODO: Multiple queues

		readonly CloudQueue queue;
		public AzureDocumentExecutor(CloudStorageAccount account) {
			var client = account.CreateCloudQueueClient();
			queue = client.GetQueueReference("documents");
			queue.CreateIfNotExist();
		}

		public void Execute(DocumentIdentifier id) {
			queue.AddMessage(new CloudQueueMessage(id.FileName()));
		}

		//The DocumentManager should already update the
		//CancellationPending property of the document.
		//We do not support active cancellation, so we 
		//can't do anything in this method.
		public void CancelProcessing(DocumentIdentifier id) {	}
	}
}
