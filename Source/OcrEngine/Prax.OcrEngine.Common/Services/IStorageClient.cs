using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Prax.OcrEngine.Services {
	///<summary>Stores and retrieves documents.</summary>
	///<remarks>This service is used by both the website and the worker roles.
	///It is not associated with a single user.</remarks>
	public interface IStorageClient {
		///<summary>Uploads a document to storage.</summary>
		///<returns>The GUID of the new document.</returns>
		Guid UploadDocument(Guid userId, string name, Stream document, long length);

		///<summary>Gets the documents belonging to a single user.</summary>
		///<param name="userId">The ID of the owning user.</param>
		///<remarks>Calling this method might result in a network request.</remarks>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Calling this method might result in a network request.")]
		IEnumerable<Document> GetDocuments(Guid userId);

		///<summary>Gets the document with the given ID, or null if there is no document with the ID.</summary>
		Document GetDocument(DocumentIdentifier id);

		///<summary>Deletes the document with the specified ID.</summary>
		void DeleteDocument(DocumentIdentifier id);

		///<summary>Sets the scan progress of a specific document.</summary>
		///<param name="id">The document ID.</param>
		///<param name="progress">The current progress, between 0 and 100.</param>
		///<remarks>This method should also set state to Scanning.</remarks>
		void SetScanProgress(DocumentIdentifier id, int progress);

		///<summary>Sets the state of a specific document.</summary>
		///<param name="id">The document ID.</param>
		///<param name="progress">The current progress, between 0 and 100.</param>
		void SetState(DocumentIdentifier id, DocumentState state);

		//TODO: Rename
	}
	///<summary>A document that can be processed by the OCR engine.</summary>
	public abstract class Document {
		///<summary>Creates a new Document with the specified ID.</summary>
		protected Document(DocumentIdentifier id) { Id = id; }

		///<summary>Gets the document's identifier.</summary>
		public DocumentIdentifier Id { get; private set; }
		///<summary>Gets the UTC timestamp that this document was initially uploaded into the system.</summary>
		public DateTime DateUploaded { get; protected set; }

		///<summary>Gets or sets the name of the document.</summary>
		public string Name { get; protected set; }
		///<summary>Gets or sets the state of this document.</summary>
		public virtual DocumentState State { get; protected set; }

		///<summary>Gets or sets the progress of the scan operation as a number between 0 and 100.</summary>
		public virtual int ScanProgress { get; protected set; }

		///<summary>Returns a new read-only Stream containing the underlying file.</summary>
		///<returns>A read-only stream, which the caller must close.</returns>
		///<remarks>The cloud implementation can stream directly from the storage service, and can in turn be streamed directly to the client browser.</remarks>
		public abstract Stream OpenRead();
		///<summary>Gets the size of the underlying file.</summary>
		public long Length { get; protected set; }
	}
	///<summary>Describes the state of an uploaded document.</summary>
	public enum DocumentState {
		///<summary>The document has been uploaded and is queued to be scanned.</summary>
		ScanQueued,
		///<summary>The document is being processed by a DocumentProcessor.</summary>
		Scanning,
		///<summary>The document has finished being processed.</summary>
		Scanned,
		///<summary>An error occurred.</summary>
		Error
	}
}
