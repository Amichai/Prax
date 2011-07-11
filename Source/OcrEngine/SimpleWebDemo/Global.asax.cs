using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Stubs = Prax.OcrEngine.Services.Stubs;
using Prax.OcrEngine.Services;
using Prax.OcrEngine;

namespace SimpleWebDemo {
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication {
		public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
			filters.Add(new HandleErrorAttribute());
		}

		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Documents", // Route name
				"Documents/{action}/{id}", // URL with parameters
				new { controller = "Documents", id = UrlParameter.Optional } // Parameter defaults
			);

			routes.MapRoute(
				"Home Action", // Route name
				"{action}", // URL with parameters
				new { controller = "Home", action = "Index" } // Parameter defaults
			);
		}

		//TODO: Converters
		//Arabic for "Enter a text here to translate", moved هنا
		//builder.RegisterInstance(new Stubs.FixedTextConverter("هنا أدخل النص لترجمة")).As<IResultsConverter>();

		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();

			var config = Config.CreateCurrent();
			config.CreateContainer();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
		}
	}
}