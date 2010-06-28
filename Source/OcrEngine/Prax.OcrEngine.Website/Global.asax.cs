using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Prax.OcrEngine.Website {
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication {
		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute("Content pages",
							"{action}",
							new { controller = "Content", action = "Home" },
							new { action = new ValueListConstraint(Controllers.ContentController.Pages) }
			);

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);

		}

		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();

			RegisterRoutes(RouteTable.Routes);
		}
	}
	class ValueListConstraint : IRouteConstraint {
		readonly IEnumerable<string> allowedValues;
		public ValueListConstraint(IEnumerable<string> allowedValues) { this.allowedValues = allowedValues; }

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
			return allowedValues.Contains((string)values[parameterName], StringComparer.OrdinalIgnoreCase);
		}
	}

}