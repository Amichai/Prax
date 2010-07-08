using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Globalization;
using System.Dynamic;

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

		///<summary>Renders a partial view that shows the progress of a single document.</summary>
		///<remarks>The parameters are bound from a Document instance.  
		///Since Document is abstract, I cannot simply pass the Document itself.</remarks>
		[ChildActionOnly]
		public ActionResult ProgressBar(DocumentState state, int scanProgress) {
			string caption;
			switch (state) {
				case DocumentState.Scanned:
					return Content("Scanned");

				case DocumentState.ScanQueued:
					caption = "Queued";
					break;
				case DocumentState.Scanning:
					caption = scanProgress.ToString(CultureInfo.CurrentCulture) + "%";
					break;
				case DocumentState.Error:
					caption = "Error";
					break;
				default:
					caption = "Unknown";
					break;
			}

			return PartialView(new Models.ProgressBarModel {
				Progress = scanProgress,
				Caption = caption
			});
		}
	}
}
