using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Threading;
using System.Collections.Concurrent;

namespace Prax.OcrEngine.Services.Azure {
	///<summary>Reads messages from an Azure queue, guaranteeing that each message will only be returned once.</summary>
	class SingleDeliveryQueueClient {
		readonly CloudQueue queue;
		readonly TableServiceContext processedMessagesContext;
		const string TableName = "ProcessedMessages";

		public SingleDeliveryQueueClient(CloudStorageAccount account, string queueName) {
			var queueClient = account.CreateCloudQueueClient();
			queue = queueClient.GetQueueReference(queueName);
			queue.CreateIfNotExist();

			var tableClient = account.CreateCloudTableClient();
			tableClient.CreateTableIfNotExist(TableName);
			processedMessagesContext = tableClient.GetDataServiceContext();
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

				var pm = new ProcessedMessage(queue, message);
				processedMessagesContext.AddObject(TableName, pm);
				try {
					processedMessagesContext.SaveChangesWithRetries();
				} catch { continue; }	//If we failed to save, assume that a different machine already received the message.
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
				processedMessagesContext.DeleteObject(pm);
			}, null);
		}
	}
}
