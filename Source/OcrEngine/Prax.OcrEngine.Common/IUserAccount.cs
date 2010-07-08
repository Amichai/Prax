using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine {
	///<summary>Performs user authentication.</summary>
	public interface IAuthenticator {
		///<summary>Gets the current user.</summary>
		IUserAccount CurrentUser { get; }

		///<summary>Verifies a user's login credentials and sets an auth token.</summary>
		bool Login(string email, string password);
	}

	///<summary>Describes a user account.</summary>
	public interface IUserAccount {
		///<summary>Gets the user's email address.</summary>
		string Email { get; }
		///<summary>Changes the user's login password.</summary>
		bool ChangePassword(string currentPassword, string newPassword);

		///<summary>Gets an IDocumentManager implementation that manages the user's documents.</summary>
		IDocumentManager DocumentManager { get; }
	}
}
