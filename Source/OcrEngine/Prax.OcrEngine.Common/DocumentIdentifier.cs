using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine {
	///<summary>Uniquely identifies a single document.</summary>
	///<remarks>To allow Azure storage to efficiently retrieve a user's
	///documents, all documents are identified by both the user ID and
	///the document's GUID.  This structure stores these IDs.
	///This is hidden from the front-end by the IDocumentManager service.</remarks>
	public struct DocumentIdentifier : IEquatable<DocumentIdentifier> {
		///<summary>Creates a DocumentIdentifier.</summary>
		public DocumentIdentifier(Guid userId, Guid documentId)
			: this() {
			UserId = userId;
			DocumentId = documentId;
		}

		///<summary>Gets the ID of the user that owns the document.</summary>
		public Guid UserId { get; private set; }
		///<summary>Gets the GUID of the document.</summary>
		public Guid DocumentId { get; private set; }

		///<summary>Returns the hash code for this instance.</summary>
		///<returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() { return UserId.GetHashCode() ^ DocumentId.GetHashCode(); }
		///<summary>Checks whether this DocumentIdentifier is equal to an object.</summary>
		///<param name="obj">The value to compare to.</param>
		///<returns>True if the values are equal.</returns>
		public override bool Equals(object obj) { return obj is DocumentIdentifier && Equals((DocumentIdentifier)obj); }
		///<summary>Checks whether this DocumentIdentifier is equal to another DocumentIdentifier value.</summary>
		///<param name="other">The DocumentIdentifier to compare to.</param>
		///<returns>True if the dates are equal.</returns>
		public bool Equals(DocumentIdentifier other) { return UserId == other.UserId && DocumentId == other.DocumentId; ; }

		///<summary>Checks whether two DocumentIdentifiers are equal.</summary
		public static bool operator ==(DocumentIdentifier first, DocumentIdentifier second) { return first.Equals(second); }
		///<summary>Checks whether two DocumentIdentifiers are unequal.</summary>
		public static bool operator !=(DocumentIdentifier first, DocumentIdentifier second) { return !(first == second); }
	}
}
