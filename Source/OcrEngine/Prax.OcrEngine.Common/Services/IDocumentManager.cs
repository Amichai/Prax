using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace Prax.OcrEngine.Services {
	///<summary>Used by the website to interact with a user's documents in storage and with the scan workers.</summary>
	///<remarks>This service is associated with a single user.</remarks>
	public interface IDocumentManager {

		///<summary>Gets the user ID to manage documents for.</summary>
		Guid UserId { get; }

		///<summary>Uploads a document to storage.</summary>
		///<returns>The ID of the new document.</returns>
		Guid UploadDocument(string name, Stream document, long length);

		///<summary>Gets all of the user's documents.</summary>
		///<remarks>Calling this method might result in a network request.</remarks>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Calling this method might result in a network request.")]
		IEnumerable<Document> GetDocuments();
		///<summary>Gets the document with the given ID, or null if this user has no document with the ID.</summary>
		Document GetDocument(Guid id);

		///<summary>Deletes the document with the specified ID.</summary>
		void DeleteDocument(Guid id);

		//TODO: Rename
	}
	//The Azure implementation will probably
	//use this pattern: http://azure.snagy.name/blog/?p=219

	///<summary>A document that can be processed by the OCR engine.</summary>
	public abstract class Document {
		///<summary>Creates a new Document with the specified ID.</summary>
		protected Document(DocumentIdentifier id) { Id = id; }

		///<summary>Gets the document's identifier.</summary>
		public DocumentIdentifier Id { get; private set; }
		///<summary>Gets the date that this document was initially uploaded into the system.</summary>
		public DateTime DateUploaded { get; protected set; }

		///<summary>Gets or sets the name of the document.</summary>
		public string Name { get; set; }
		///<summary>Gets or sets the state of this document.</summary>
		public virtual DocumentState State { get; set; }

		///<summary>Gets or sets the progress of the scan operation as a number between 0 and 100.</summary>
		public virtual int ScanProgress { get; set; }

		///<summary>Returns a read-only MemoryStream containing the underlying file.</summary>
		///<remarks>The cloud implementation can stream directly from the storage service, and can in turn be streamed directly to the client browser.</remarks>
		public abstract MemoryStream Read();
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
