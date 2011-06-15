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
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}

		static void SetupAutofac() {
			var builder = new ContainerBuilder();
			builder.RegisterType<DocumentManager>().As<IDocumentManager>();

			builder.RegisterType<MsSampleImageRenderer>().As<IImageCreator>();

			//Trivial users
			builder.RegisterType<Stubs.InMemoryUserAccount>().As<IUserAccount>();
			builder.RegisterType<Stubs.UserlessAuthenticator>().As<IAuthenticator>()
						.SingleInstance();

			//In-memory processing
			builder.RegisterType<Stubs.InMemoryStorage>().As<IStorageClient>()
					.SingleInstance();
			builder.RegisterType<Stubs.SimpleProcessorController>().As<IProcessorController>()
					.SingleInstance();

			builder.RegisterType<Stubs.UselessProcessor>().As<IDocumentProcessor>()
						.InstancePerDependency();

			//TODO: Converters
			//Arabic for "Enter a text here to translate", moved هنا
			builder.RegisterInstance(new Stubs.FixedTextConverter("هنا أدخل النص لترجمة")).As<IResultsConverter>();

			builder.RegisterControllers(typeof(MvcApplication).Assembly);
			var container = builder.Build();
			DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
		}

		protected void Application_Start() {
			AreaRegistration.RegisterAllAreas();

			SetupAutofac();
			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
		}
	}
}