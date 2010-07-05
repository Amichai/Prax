using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prax.OcrEngine.Website.Controllers {
	public class DocumentsController : Controller {

		///<summary>Shows a list of documents in the user's account.</summary>
		///<remarks>This is the website's default home page.</remarks>
		public ActionResult Index() {
			return View("DocumentList", new Models.DocumentListModel(true, null));
		}

		[HttpPost]
		public ActionResult Upload(HttpPostedFileBase file) {
			return File(file.InputStream, file.ContentType);
		}
	}
}
