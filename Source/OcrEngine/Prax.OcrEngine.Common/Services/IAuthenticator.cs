using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Services {
	///<summary>Performs user authentication.</summary>
	public interface IAuthenticator {
		///<summary>Gets the current user.</summary>
		IUserAccount CurrentUser { get; }

		///<summary>Verifies a user's login credentials and sets an auth token.</summary>
		bool Login(string email, string password);
	}
}
