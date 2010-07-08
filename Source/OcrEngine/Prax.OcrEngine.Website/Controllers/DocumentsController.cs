using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace Prax.OcrEngine.Website.Controllers {
	public class DocumentsController : Controller {
		readonly IAuthenticator Authenticator;
		IDocumentManager DocumentManager { get { return Authenticator.CurrentUser.DocumentManager; } }

		public DocumentsController(IAuthenticator authenticator) { Authenticator = authenticator; }

		///<summary>Shows a list of documents in the user's account.</summary>
		///<remarks>This is the website's default home page.</remarks>
		public ActionResult Index() {
			return View("DocumentList", new Models.DocumentListModel(true, DocumentManager.GetDocuments()));
		}

		[HttpPost]
		public ActionResult Upload(HttpPostedFileBase file) {
			DocumentManager.UploadDocument(Path.GetFileName(file.FileName), file.InputStream, file.ContentLength);
			return Index();
		}
	}
}
