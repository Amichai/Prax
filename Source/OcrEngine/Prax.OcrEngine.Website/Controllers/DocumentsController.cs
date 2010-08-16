﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Globalization;
using System.Dynamic;
using Prax.OcrEngine.Services;
using Prax.OcrEngine.Website.Models;

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
			if (file == null) return Index();	//TODO: Show error message
			DocumentManager.UploadDocument(Path.GetFileName(file.FileName), file.InputStream, file.ContentLength);
			return Index();
		}

		public ActionResult View(Guid id, string name = null) {
			var doc = DocumentManager.GetDocument(id);
			return File(doc.Read(), MimeTypes.ForExtension(Path.GetExtension(doc.Name)));
		}

		///<summary>Gets the document list in JSON format for table updates.</summary>
		///<remarks>This method is called as a child action from the DocumentList view,
		///and as a normal action via AJAX.  When called as a child action, it gets the
		///document list from the parent ViewContext to save a network call.</remarks>
		public ActionResult Data() {
			IEnumerable<Document> documents;
			if (ControllerContext.ParentActionViewContext != null)
				documents = (IEnumerable<Document>)ControllerContext.ParentActionViewContext.ViewData["Documents"];
			else
				documents = DocumentManager.GetDocuments();

			TimeSpan? refreshTimeout = documents.Max(d => d.GetRefreshTime());

			return Json(new {
				refreshTimeout = (refreshTimeout ?? TimeSpan.Zero).TotalMilliseconds,
				documents = documents.Select(GetJsonObject)
			}, JsonRequestBehavior.AllowGet);
		}
		static object GetJsonObject(Document doc) {
			if (doc.State == DocumentState.Scanned)
				return new {
					id = doc.Id,
					state = "Scanned"
				};
			else
				return new {
					id = doc.Id,
					state = doc.State.ToString(),
					progress = doc.ScanProgress,
					progressCaption = GetBarCaption(doc.State, doc.ScanProgress)
				};
		}

		///<summary>Renders a partial view that shows the progress of a single document.</summary>
		///<remarks>The parameters are bound from a Document instance.  
		///Since Document is abstract, I cannot simply pass the Document itself.</remarks>
		[ChildActionOnly]
		public ActionResult ProgressBar(DocumentState state, int scanProgress) {
			if (state == DocumentState.Scanned)
				return Content("Scanned");

			return PartialView(new Models.ProgressBarModel {
				Progress = scanProgress,
				Caption = GetBarCaption(state, scanProgress)
			});
		}
		static string GetBarCaption(DocumentState state, int scanProgress) {
			switch (state) {
				case DocumentState.Scanned:
					throw new ArgumentException("Scanned documents have no progress bar.", "state");

				case DocumentState.ScanQueued:
					return "Queued";
				case DocumentState.Scanning:
					return scanProgress.ToString(CultureInfo.CurrentCulture) + "%";
				case DocumentState.Error:
					return "Error";
				default:
					return "Unknown";
			}
		}
	}
}