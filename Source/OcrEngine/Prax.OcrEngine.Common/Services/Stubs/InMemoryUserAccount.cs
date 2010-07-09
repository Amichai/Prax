using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Services.Stubs {
	///<summary>A user account stored in memory.</summary>
	public class InMemoryUserAccount : IUserAccount {
		public InMemoryUserAccount(Guid id, Func<Guid, IDocumentManager> docManCreator) {
			DocumentManager = docManCreator(id);
			UserId = id;
		}

		public Guid UserId { get; private set; }

		public string Email { get; set; }

		public bool ChangePassword(string currentPassword, string newPassword) { return false; }

		public IDocumentManager DocumentManager { get; private set; }
	}
}
