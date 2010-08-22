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
		///<returns>The ID of the new document.</returns>
		Guid UploadDocument(Guid userId, string name, Stream document, long length);

		///<summary>Gets the documents belonging to a single user.</summary>
		///<param name="userId">The ID of the owning user.</param>
		///<remarks>Calling this method might result in a network request.</remarks>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Calling this method might result in a network request.")]
		IEnumerable<Document> GetDocuments(Guid userId);

		///<summary>Gets the document with the given ID, or null if there is no document with the ID.</summary>
		Document GetDocument(Guid id);

		///<summary>Deletes the document with the specified ID.</summary>
		void DeleteDocument(Guid id);

		///<summary>Sets the scan progress of a specific document.</summary>
		///<param name="id">The document ID.</param>
		///<param name="progress">The current progress, between 0 and 100.</param>
		///<remarks>This method should also set state to Scanning.</remarks>
		void SetScanProgress(Guid id, int progress);

		///<summary>Sets the state of a specific document.</summary>
		///<param name="id">The document ID.</param>
		///<param name="progress">The current progress, between 0 and 100.</param>
		void SetState(Guid id, DocumentState state);

		//TODO: Rename
	}
}
