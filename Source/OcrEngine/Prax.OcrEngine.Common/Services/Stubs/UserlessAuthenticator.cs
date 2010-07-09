using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Services.Stubs {
	///<summary>Authenticates a single user at all times.</summary>
	public class UserlessAuthenticator : IAuthenticator {
		const string DefaultEmail = "Demo@Prax.com";
		static readonly Guid UserId = new Guid("89B5331F-4043-4497-8E66-9C8C6D14FB9F");

		public UserlessAuthenticator(Func<Guid, IUserAccount> userMaker) {
			CurrentUser = userMaker(UserId);
			CurrentUser.Email = DefaultEmail;
		}

		public IUserAccount CurrentUser { get; private set; }
		public bool Login(string email, string password) { return false; }
	}
}
