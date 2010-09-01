using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Prax.OcrEngine.Services.Azure {
	///<summary>Reads messages from an Azure queue, guaranteeing that each message will only be returned once.</summary>
	class SingleDeliveryQueueClient {
		readonly CloudTableClient tableClient;
		readonly CloudQueue queue;
		const string TableName = "ProcessedMessages";

		public SingleDeliveryQueueClient(CloudStorageAccount account, string queueName) {
			var queueClient = account.CreateCloudQueueClient();
			queue = queueClient.GetQueueReference(queueName);
			queue.CreateIfNotExist();

			tableClient = account.CreateCloudTableClient();
			tableClient.CreateTableIfNotExist(TableName);
		}

		class ProcessedMessage : TableServiceEntity {
			public ProcessedMessage(CloudQueue queue, CloudQueueMessage message) {
				base.PartitionKey = queue.Name;
				base.RowKey = message.Id;
			}
		}

		readonly ConcurrentDictionary<CloudQueueMessage, ProcessedMessage> messages = new ConcurrentDictionary<CloudQueueMessage, ProcessedMessage>();

		///<summary>Gets the next message in the queue.</summary>
		///<returns>A non-null queue CloudQueueMessage.</returns>
		///<remarks>This is a blocking method.  If the queue is empty, the call will never return.</remarks>
		public CloudQueueMessage GetMessage() {
			while (true) {
				var message = queue.GetMessage();

				while (message == null) {
					Thread.Sleep(TimeSpan.FromSeconds(5));
					message = queue.GetMessage();
				}

				var dataContext = tableClient.GetDataServiceContext();
				var pm = new ProcessedMessage(queue, message);
				dataContext.AddObject(TableName, pm);
				try {
					dataContext.SaveChanges();
				} catch (DataServiceRequestException ex) {
					if (ex.Response.All(o => o.StatusCode == 409))	//HTTP 409 Conflict
						continue;
					else
						throw;
				}	//If we failed to save, assume that a different machine already received the message.

				messages.AddOrUpdate(message, pm, (k, v) => pm);

				return message;
			}
		}

		///<summary>Deletes a processed message from the queue.</summary>
		///<remarks>This is a non-blocking asynchronous method.</remarks>
		public void DeleteMessage(CloudQueueMessage message) {
			queue.BeginDeleteMessage(message, r => {
				queue.EndDeleteMessage(r);

				//I have no idea whether this is necessary. I'm
				//afraid that the Azure queue might return the 
				//same message yet again just after deleting it
				Thread.Sleep(TimeSpan.FromSeconds(8));

				ProcessedMessage pm;
				messages.TryRemove(message, out pm);

				var dataContext = tableClient.GetDataServiceContext();
				dataContext.AttachTo(TableName, pm);
				dataContext.DeleteObject(pm);
				dataContext.SaveChangesWithRetries();
			}, null);
		}
	}
}
