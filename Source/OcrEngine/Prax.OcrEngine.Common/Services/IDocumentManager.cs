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
}
