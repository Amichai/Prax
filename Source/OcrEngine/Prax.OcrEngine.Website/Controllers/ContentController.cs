using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Website.Controllers {
	public class ContentController : Controller {
		public static readonly ReadOnlyCollection<string> Pages = new ReadOnlyCollection<string>(
			typeof(ContentController).GetMethods()
									 .Where(m => typeof(ActionResult).IsAssignableFrom(m.ReturnType)
											 && m.GetCustomAttribute<ChildActionOnlyAttribute>() == null)
									 .Select(m => m.Name)
									 .ToArray()
		);

		public ActionResult Home() { return View(); }
		public ActionResult FAQ() { return View(); }
		public ActionResult About() { return View(); }
		public ActionResult Contact() { return View(); }
		public ActionResult Privacy() { return View(); }

		[ChildActionOnly]
		public ActionResult SiteIntro() { return PartialView(); }
	}
}
