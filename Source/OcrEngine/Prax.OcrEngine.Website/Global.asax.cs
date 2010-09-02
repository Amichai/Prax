using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Web;
using Autofac.Integration.Web.Mvc;
using Microsoft.WindowsAzure;
using Prax.OcrEngine.Services;
using Azure = Prax.OcrEngine.Services.Azure;
using Stubs = Prax.OcrEngine.Services.Stubs;

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
				"{controller}/{action}/{id}/{name}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional, name = UrlParameter.Optional } // Parameter defaults
			);
		}

		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();

			var builder = new ContainerBuilder();
			builder.Configure();

			//Set up Autofac for MVC
			builder.RegisterControllers(typeof(PraxMvcApplication).Assembly);
			containerProvider = new ContainerProvider(builder.Build());
			ControllerBuilder.Current.SetControllerFactory(new InjectingControllerFactory(ContainerProvider));

			containerProvider.ApplicationContainer.Resolve<Azure.InMemoryWorkerPool>().StartPool();

			RegisterRoutes(RouteTable.Routes);
		}

		static IContainerProvider containerProvider;
		public IContainerProvider ContainerProvider { get { return containerProvider; } }
	}

	///<summary>An IRouteConstraint implementation that constrains a parameter to a fixed set of values.</summary>
	class ValueListConstraint : IRouteConstraint {
		readonly IEnumerable<string> allowedValues;
		public ValueListConstraint(IEnumerable<string> allowedValues) { this.allowedValues = allowedValues; }

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
			return allowedValues.Contains((string)values[parameterName], StringComparer.OrdinalIgnoreCase);
		}
	}

	///<summary>A ControllerFactory that adds the current RequestContext to the AutoFac registry.</summary>
	class InjectingControllerFactory : AutofacControllerFactory {
		readonly IContainerProvider containerProvider;
		public InjectingControllerFactory(IContainerProvider containerProvider) : base(containerProvider) { this.containerProvider = containerProvider; }

		protected override IController GetControllerInstance(RequestContext context, Type controllerType) {
			var newBuilder = new ContainerBuilder();
			newBuilder.RegisterInstance(context).As<RequestContext>();
			newBuilder.Update(containerProvider.RequestLifetime.ComponentRegistry);

			return base.GetControllerInstance(context, controllerType);
		}
	}
}