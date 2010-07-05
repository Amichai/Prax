using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac.Integration.Web;
using Autofac.Integration.Web.Mvc;
using Autofac;

namespace Prax.OcrEngine.Website {
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class PraxMvcApplication : HttpApplication, IContainerProviderAccessor {
		public static void RegisterRoutes(RouteCollection routes) {
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute("Default home page", "", new { controller = "Documents", action = "Index" });

			routes.MapRoute("Content pages",
							"{action}",
							new { controller = "Content", action = "Home" },
							new { action = new ValueListConstraint(Controllers.ContentController.Pages) }
			);

			routes.MapRoute(
				"Default route", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}

		static void RegisterComponents(ContainerBuilder builder) {
			//Register components here
		}

		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();


			var builder = new ContainerBuilder();
			RegisterComponents(builder);

			//Set up Autofac for MVC
			builder.RegisterControllers(typeof(PraxMvcApplication).Assembly);
			containerProvider = new ContainerProvider(builder.Build());
			ControllerBuilder.Current.SetControllerFactory(new AutofacControllerFactory(ContainerProvider));

			RegisterRoutes(RouteTable.Routes);
		}


		static IContainerProvider containerProvider;
		public IContainerProvider ContainerProvider { get { return containerProvider; } }
	}

	class ValueListConstraint : IRouteConstraint {
		readonly IEnumerable<string> allowedValues;
		public ValueListConstraint(IEnumerable<string> allowedValues) { this.allowedValues = allowedValues; }

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
			return allowedValues.Contains((string)values[parameterName], StringComparer.OrdinalIgnoreCase);
		}
	}
}