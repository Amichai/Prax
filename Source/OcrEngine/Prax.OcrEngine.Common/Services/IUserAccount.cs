using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Services {
	///<summary>Describes a user account.</summary>
	public interface IUserAccount {
		///<summary>Gets the user's unique identifier.</summary>
		Guid UserId { get; }
		///<summary>Gets the user's email address.</summary>
		string Email { get; set; }
		///<summary>Changes the user's login password.</summary>
		bool ChangePassword(string currentPassword, string newPassword);

		///<summary>Gets an IDocumentManager implementation that manages the user's documents.</summary>
		IDocumentManager DocumentManager { get; }
	}
}
