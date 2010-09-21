using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Prax.OcrEngine.Services;
using System.IO;

namespace Prax.OcrEngine.Website.Views {
	///<summary>Contains extensions methods used by MVC views.</summary>
	public static class ViewsExtensions {
		public static MvcHtmlString ResultsLink(this HtmlHelper html, Document doc, ResultFormat format) {
			var shortName = format.GetExtension().Substring(1).ToUpperInvariant();	//Remove .
			return html.ActionLink(
				linkText: " ",
				//linkText: "Download " + shortName,
				actionName: "Results",
				routeValues: new { controller = "Documents", format, id = doc.Id.DocumentId, name = Path.ChangeExtension(doc.Name, format.GetExtension()) },
				htmlAttributes: new { @class = "DownloadIcon Sprite16 " + shortName, title = "Download OCR results as a " + shortName + " file" }
			);
		}
	}
}