using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Prax.OcrEngine.Services.Azure {
	///<summary>Controls document processors on Azure worker nodes.</summary>
	public class AzureProcessorController : IProcessorController {
		//TODO: Multiple queues

		readonly CloudQueue queue;
		public AzureProcessorController(CloudStorageAccount account) {
			var client = account.CreateCloudQueueClient();
			queue = client.GetQueueReference("documents");
			queue.CreateIfNotExist();
		}

		public void BeginProcessing(DocumentIdentifier id) {
			queue.AddMessage(new CloudQueueMessage(id.FileName()));
		}

		public void CancelProcessing(DocumentIdentifier id) {
			throw new NotImplementedException();
		}
	}
}
