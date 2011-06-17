using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SimpleWebDemo.Controllers {
	public class HomeController : Controller {
		public ActionResult Index() {
			if (Request.Url.DnsSafeHost.StartsWith("demo", StringComparison.OrdinalIgnoreCase))
				return View("Demo");
			return View();
		}

		public ActionResult Demo() {
			return View();
		}

		public ActionResult About() {
			return View();
		}
	}
}
