using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prax.OcrEngine.Services;
using Prax.OcrEngine;
using System.Xml.Linq;
using System.IO;

namespace SimpleWebDemo.Controllers {
	public class DocumentsController : Controller {
		readonly IAuthenticator Authenticator;
		IDocumentManager DocumentManager { get { return Authenticator.CurrentUser.DocumentManager; } }
		readonly IImageCreator ImageCreator;

		public DocumentsController(IAuthenticator authenticator, IImageCreator imageCreator) { Authenticator = authenticator; ImageCreator = imageCreator; }

		public ActionResult View(Guid id) {
			var doc = DocumentManager.GetDocument(id);
			return File(doc.OpenRead(), doc.MimeType);
		}

#if DEBUG
		[ValidateInput(false)]	//This method exists to test the image renderer.
		public ActionResult RenderImage(string html) {
			return File(ImageCreator.CreateImage(html), ImageCreator.ContentType);
		}
#endif
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult CreateFromHtml(string html) {
			var stream = ImageCreator.CreateImage(html);

			var id = DocumentManager.UploadDocument("Created image, " + Request.UserHostAddress, ImageCreator.ContentType, stream, stream.Length);

			return Content(id.ToString(), "text/plain");
		}
		[HttpPost]
		public ActionResult UploadImage(HttpPostedFileBase image) {
			var id = DocumentManager.UploadDocument(image.FileName, image.ContentType, image.InputStream, image.ContentLength);
			return Content(id.ToString(), "text/plain");
		}

		public ActionResult Status(Guid id) {
			var doc = DocumentManager.GetDocument(id);
			switch (doc.State) {
				case DocumentState.ScanQueued:
					return Json(new { state = "Queued", progress = 0 }, JsonRequestBehavior.AllowGet);
					
				case DocumentState.Scanning:
					return Json(new { state = "Scanning", progress = doc.ScanProgress }, JsonRequestBehavior.AllowGet);
					
				case DocumentState.Scanned:
					using (var reader = new StreamReader(doc.OpenStream("PlainText")))
						return Json(new { state = "Complete", text = reader.ReadToEnd() }, JsonRequestBehavior.AllowGet);

				default:	//Including DocumentState.Error
					return Json(new { state = "Error" }, JsonRequestBehavior.AllowGet);
			}
		}
	}
}
