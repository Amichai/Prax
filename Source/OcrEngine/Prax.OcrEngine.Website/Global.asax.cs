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
using System.Diagnostics.CodeAnalysis;

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

			routes.MapRoute("OCR Results",
				"Documents/{id}/{format}/{name}",
				new { controller = "Documents", action = "Results" },
				new { id = GuidConstraint.Instance, format = new EnumConstraint(typeof(ResultFormat)) });

			routes.MapRoute("Default route", // Route name
				"{controller}/{action}/{id}/{name}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional, name = UrlParameter.Optional } // Parameter defaults
			);
		}

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Automatically added event handler")]
		protected void Application_Start() {
			//AreaRegistration.RegisterAllAreas();

			var config = Config.CreateCurrent();

			containerProvider = config.CreateProvider();

			RegisterRoutes(RouteTable.Routes);
		}

		///<summary>Gets the Autofac container provider.</summary>
		public IContainerProvider ContainerProvider { get { return containerProvider; } }
		//This must be static, since HttpApplications are not reused.
		static IContainerProvider containerProvider;
	}

	///<summary>An IRouteConstraint implementation that constrains a parameter to a fixed set of values.</summary>
	class ValueListConstraint : IRouteConstraint {
		readonly IEnumerable<string> allowedValues;
		public ValueListConstraint(IEnumerable<string> allowedValues) { this.allowedValues = allowedValues; }

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
			return allowedValues.Contains(values[parameterName].ToString(), StringComparer.OrdinalIgnoreCase);
		}
	}
	///<summary>An IRouteConstraint implementation that constrains a parameter to the names in an enum.</summary>
	class EnumConstraint : ValueListConstraint {
		public EnumConstraint(Type enumType) : base(Enum.GetNames(enumType)) { }
	}
	///<summary>An IRouteConstraint implementation that constrains a parameter to GUID.</summary>
	class GuidConstraint : IRouteConstraint {
		private GuidConstraint() { }
		public static readonly GuidConstraint Instance = new GuidConstraint();

		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
			object value = values[parameterName];
			Guid temp;
			return value is Guid || Guid.TryParse((string)value, out temp);
		}
	}
}