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

		public Guid UploadDocument(Guid userId, string name, string mimeType, Stream document, long length) {
			var id = new DocumentIdentifier(userId, Guid.NewGuid());
			var doc = new BlobDocument(id, name, CreateBlob(id));

			doc.UpdateMetadata();
			doc.Blob.Properties.ContentType = mimeType;

			doc.Blob.UploadFromStream(document);

			return id.DocumentId;
		}
		class BlobDocument : Document {
			internal CloudBlob Blob { get; private set; }

			///<summary>Creates a new BlobDocument.</summary>
			internal BlobDocument(DocumentIdentifier id, string name, CloudBlob emptyBlob)
				: base(id) {
				this.Blob = emptyBlob;
				this.Name = name;
				SetInitialValues();
			}

			///<summary>Creates a BlobDocument from an existing blob.</summary>
			public BlobDocument(CloudBlob blob)
				: base(IdUtils.ParseUri(blob.Uri)) {
				this.Blob = blob;
				base.Length = blob.Properties.Length;

				//When adding new properties, be sure to maintain 
				//backwards compatibility with existing documents.
				//(Or update the existing documents manually)

				base.DateUploaded = DateTime.Parse(blob.Metadata["Date"], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
				base.Name = Uri.UnescapeDataString(blob.Metadata["Name"]);
				base.ScanProgress = int.Parse(blob.Metadata["Progress"], CultureInfo.InvariantCulture);
				base.CancellationPending = bool.Parse(blob.Metadata["CancellationPending"]);
				base.State = (DocumentState)Enum.Parse(typeof(DocumentState), blob.Metadata["State"]);
			}

			///<summary>Updates the blob metadata from the writable properties of the document.</summary>
			internal void UpdateMetadata() {
				Blob.Metadata["Name"] = Uri.EscapeDataString(Name);
				Blob.Metadata["Date"] = DateUploaded.ToString("u", CultureInfo.InvariantCulture);
				Blob.Metadata["Progress"] = ScanProgress.ToString(CultureInfo.InvariantCulture);
				Blob.Metadata["CancellationPending"] = CancellationPending.ToString();
				Blob.Metadata["State"] = State.ToString();
			}

			public override string MimeType {
				get { return Blob.Properties.ContentType; }
				protected set { }
			}

			public override Stream OpenRead() { return Blob.OpenRead(); }

			internal CloudBlob CreateAlternateBlob(string name) {
				return new CloudBlob(Blob.Container.Name + "/AlternateStreams/" + Id.FileName() + "." + name, Blob.ServiceClient);
			}

			public override Stream OpenStream(string name) {
				return CreateAlternateBlob(name).OpenRead();
			}

			public override void UploadStream(string name, Stream stream, long length) {
				CreateAlternateBlob(name).UploadFromStream(stream);	//The PUT Blob operation will replace existing blobs.

				//Update the list of child blobs in the metadata
				Blob.Metadata["AlternateStreams"] += name + ";";		//This will result in a trailing `;`, which is ignored.
				Blob.SetMetadata();
			}

			public override IEnumerable<string> AlternateStreamNames {
				get { return (Blob.Metadata["AlternateStreams"] ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries); }
			}
		}

		static readonly BlobRequestOptions GetDocsOptions = new BlobRequestOptions { BlobListingDetails = BlobListingDetails.Metadata, UseFlatBlobListing = true };
		public IEnumerable<Document> GetDocuments(Guid userId) {
			return client.ListBlobsWithPrefix(container.Name + "/" + userId.ToString(), GetDocsOptions)
				.OfType<CloudBlob>()
				.Select(b => new BlobDocument(b));
		}

		public Document GetDocument(DocumentIdentifier id) {
			var blob = container.GetBlobReference(id.FileName());
			try {
				blob.FetchAttributes();
			} catch (StorageClientException) { return null; }	//If the blob was deleted

			return new BlobDocument(blob);
		}
		CloudBlob CreateBlob(DocumentIdentifier id) { return new CloudBlob(container.Name + "/" + id.FileName(), client); }

		public void DeleteDocument(DocumentIdentifier id) {
			var doc = (BlobDocument)GetDocument(id);

			foreach (var alternateStream in doc.AlternateStreamNames) {
				doc.CreateAlternateBlob(alternateStream).Delete();
			}

			doc.Blob.Delete();
		}

		public bool UpdateDocument(Document doc) {
			var bdoc = (BlobDocument)doc;
			bdoc.UpdateMetadata();

			try {
				bdoc.Blob.SetMetadata();
			} catch (StorageClientException) { return false; }	//If the blob was deleted

			return true;
		}
	}
}
