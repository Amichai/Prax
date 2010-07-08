using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine {
	///<summary>Authenticates a single user at all times.</summary>
	public class UserlessAuthenticator : IAuthenticator {
		const string DefaultEmail = "Demo@Prax.com";
		public UserlessAuthenticator(Func<string, IUserAccount> userMaker) { CurrentUser = userMaker(DefaultEmail); }

		public IUserAccount CurrentUser { get; private set; }
		public bool Login(string email, string password) { return false; }
	}

	///<summary>A user account stored in memory.</summary>
	public class InMemoryUserAccount : IUserAccount {
		public InMemoryUserAccount(string email, Func<string, IDocumentManager> docManCreator) {
			Email = email;
			DocumentManager = docManCreator(email);
		}

		public string Email { get; private set; }

		public bool ChangePassword(string currentPassword, string newPassword) { return false; }

		public IDocumentManager DocumentManager { get; private set; }
	}
}
