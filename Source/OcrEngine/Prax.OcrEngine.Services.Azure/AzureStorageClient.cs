using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.IO;
using System.Globalization;
using System.Diagnostics;

namespace Prax.OcrEngine.Services.Azure {
	///<summary>An IStorageClient implementation that uses Azure blob storage.</summary>
	public class AzureStorageClient : IStorageClient {
		readonly CloudBlobClient client;
		readonly CloudBlobContainer container;
		public AzureStorageClient(CloudStorageAccount account) {
			client = account.CreateCloudBlobClient();
			client.DefaultDelimiter = "/";

			container = client.GetContainerReference("documents");
			container.CreateIfNotExist();
		}

		public Guid UploadDocument(Guid userId, string name, Stream document, long length) {
			var id = new DocumentIdentifier(userId, Guid.NewGuid());
			var blob = CreateBlob(id);

			blob.Properties.ContentType = MimeTypes.ForExtension(Path.GetExtension(name));

			blob.Metadata.Add("Name", Uri.EscapeDataString(name));
			blob.Metadata.Add("Date", DateTime.Now.ToString("u", CultureInfo.InvariantCulture));
			blob.Metadata.Add("Progress", "0");
			blob.Metadata.Add("State", DocumentState.ScanQueued.ToString());

			blob.UploadFromStream(document);

			return id.DocumentId;
		}
		class BlobDocument : Document {
			readonly CloudBlob blob;
			public BlobDocument(CloudBlob blob)
				: base(Utils.ParseUri(blob.Uri)) {
				this.blob = blob;
				base.Length = blob.Properties.Length;

				base.DateUploaded = DateTime.Parse(blob.Metadata["Date"], CultureInfo.InvariantCulture);
				base.Name = Uri.UnescapeDataString(blob.Metadata["Name"]);
				base.ScanProgress = int.Parse(blob.Metadata["Progress"], CultureInfo.InvariantCulture);
				base.State = (DocumentState)Enum.Parse(typeof(DocumentState), blob.Metadata["State"]);
			}

			public override Stream Read() { return blob.OpenRead(); }
		}

		static readonly BlobRequestOptions GetDocsOptions = new BlobRequestOptions { BlobListingDetails = BlobListingDetails.Metadata, UseFlatBlobListing = true };
		public IEnumerable<Document> GetDocuments(Guid userId) {
			return client.ListBlobsWithPrefix(container.Name + "/" + userId.ToString(), GetDocsOptions)
				.OfType<CloudBlob>()
				.Select(b => new BlobDocument(b));
		}

		public Document GetDocument(DocumentIdentifier id) {
			var blob = container.GetBlobReference(id.FileName());
			blob.FetchAttributes();
			return new BlobDocument(blob);
		}
		CloudBlob CreateBlob(DocumentIdentifier id) { return new CloudBlob(container.Name + "/" + id.FileName(), client); }

		public void DeleteDocument(DocumentIdentifier id) {
			CreateBlob(id).Delete();
		}

		//The PUT metadata operation will 
		//overwrite all existing metadata.
		//Therefore, I fetch all existing 
		//metadata before writing it.
		public void SetScanProgress(DocumentIdentifier id, int progress) {
			var blob = CreateBlob(id);
			blob.FetchAttributes();
			blob.Metadata["Progress"] = progress.ToString(CultureInfo.InvariantCulture);
			blob.Metadata["State"] = DocumentState.Scanning.ToString();
			blob.SetMetadata();
		}

		public void SetState(DocumentIdentifier id, DocumentState state) {
			var blob = CreateBlob(id);
			blob.FetchAttributes();
			blob.Metadata["State"] = state.ToString();
			blob.SetMetadata();
		}
	}
}
