using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac;
using Microsoft.WindowsAzure;
using Prax.OcrEngine.Services;
using Azure = Prax.OcrEngine.Services.Azure;
using Stubs = Prax.OcrEngine.Services.Stubs;

#region Explanation
/* This file contains all common configuration settings.
 * It is a single entry point which should be called to 
 * set up the Autofac container.
 * 
 * The RegisterServices method should call various more 
 * specific configuration methods defined below.  These 
 * methods should configure the individual components or
 * groups of components used by the projects.
 * 
 * If a component has multiple implementations (eg, for 
 * debugging), each implementation should get a separate
 * extension method to configure it.  The configuration 
 * methods for the currently active components should be
 * called by the RegisterServices method.
 * 
 * This file is linked to by each project that needs to 
 * be configured. If a project's configuration has types
 * that other projects do not have, it should be wrapped
 * in #if blocks.  This creates a central repository of 
 * all configuration settings.
 */
#endregion

namespace Prax.OcrEngine {

	public partial class Config {
		private void RegisterServices() {
			CoreServices();

			StubUserManagement();

			DevelopmentStorage();
			AzureDocuments();

			InMemoryAzureProcessing();

			StubProcessor();

#if WEB_ROLE
			//Add website-only services here
			MvcSetup();

			ResourcesSetup();
			StandardResourceLocators();
			//DebuggingResources();
			LocalCrunchedResources();
#endif
		}


		public static Config CreateCurrent() {
			var config = new Config();
			config.RegisterServices();
			return config;
		}

		public ContainerBuilder Builder { get; private set; }
		private Config() {
			Builder = new ContainerBuilder();
		}

		List<Action<IComponentContext>> creationCallbacks = new List<Action<IComponentContext>>();
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by optional config method")]
		void AddCallback(Action<IComponentContext> callback) { creationCallbacks.Add(callback); }

		public IContainer CreateContainer() {
			var container = Builder.Build();
			creationCallbacks.ForEach(c => c(container));
			return container;
		}
	}
}
#region Website
#if WEB_ROLE
namespace Prax.OcrEngine {
	using System.Reflection;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using Autofac.Core;
	//Add website-only namespaces here (inside the conditional)
	using Autofac.Features.Indexed;
	using Autofac.Integration.Web;
	using Autofac.Integration.Web.Mvc;
	using Website;
	using Website.Resources;

	public partial class Config {
		#region Resources
		///<summary>Registers basic services used by the resources framework.</summary>
		private void ResourcesSetup() {
			Builder.RegisterGeneric(typeof(AutofacResourceService<>)).As(typeof(IResourceService<>)).SingleInstance();
		}
		///<summary>Implements the IResourceService interface using Autofac.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated by Autofac")]
		class AutofacResourceService<TService> : IResourceService<TService> {
			readonly IIndex<ResourceType, TService> index;
			public AutofacResourceService(IIndex<ResourceType, TService> index) { this.index = index; }

			public TService this[ResourceType type] { get { return index[type]; } }
		}

		///<summary>Registers the standard ResourceLocators.</summary>
		private void StandardResourceLocators() {
			Builder.RegisterInstance(new ResourceLocator(ResourceType.Javascript, folder: "~/Content/Javascript"))
				.Keyed<IResourceLocator>(ResourceType.Javascript);

			Builder.RegisterInstance(new ResourceLocator(ResourceType.Css, folder: "~/Content/CSS"))
				.Keyed<IResourceLocator>(ResourceType.Css);
		}

		///<summary>Registers debug-friendly ResourceResolvers that resolve resources uncompressed and un-combined.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Optional config method")]
		private void DebuggingResources() {
			Builder.RegisterType<ResourceDebuggingResolver>().As<IResourceResolver>();
		}

		///<summary>Registers production-ready ResourceResolvers that resolve resources using a server-side cruncher.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Optional config method")]
		private void LocalCrunchedResources() {
			Builder.RegisterInstance(new MSAjaxScriptMinifier())
				.Keyed<IMinifier>(ResourceType.Javascript);
			Builder.RegisterInstance(new MSAjaxStylesheetMinifier())
				.Keyed<IMinifier>(ResourceType.Css);

			Builder.RegisterType<ResourceCombiningResolver>().As<IResourceResolver>();
		}

		//TODO: CdnResources
		#endregion
		#region MVC Support
		///<summary>Sets up the Autofac container for the ASP.Net MVC framework.</summary>
		private void MvcSetup() {
			Builder.RegisterControllers(typeof(Website.PraxMvcApplication).Assembly);

			Builder.Register(cc => new HttpServerUtilityWrapper(HttpContext.Current.Server)).As<HttpServerUtilityBase>();

			Builder.Register(cc => requestContext).As<RequestContext>();
			Builder.RegisterType<UrlHelper>().As<UrlHelper>();
			Builder.RegisterType<HtmlHelper>().As<HtmlHelper>();
		}
		public ContainerProvider CreateProvider() {
			var provider = new ContainerProvider(CreateContainer());

			ControllerBuilder.Current.SetControllerFactory(new InjectingControllerFactory(provider));

			return provider;
		}


		[ThreadStatic]
		static RequestContext requestContext;

		///<summary>A ControllerFactory that sets the requestContext field to the current RequestContext, for use with AutoFac.</summary>
		class InjectingControllerFactory : AutofacControllerFactory {
			public InjectingControllerFactory(IContainerProvider containerProvider) : base(containerProvider) { }

			protected override IController GetControllerInstance(RequestContext context, Type controllerType) {
				requestContext = context;
				return base.GetControllerInstance(context, controllerType);
			}
			public override void ReleaseController(IController controller) {
				base.ReleaseController(controller);
				requestContext = null;
			}
		}
		#endregion
	}
}
#endif
#endregion

namespace Prax.OcrEngine {
	public partial class Config {
		///<summary>Registers core services.</summary>
		private void CoreServices() {
			Builder.RegisterType<DocumentManager>().As<IDocumentManager>();
		}

		#region Stubs
		///<summary>Registers in-memory user management services.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Optional config method")]
		private void StubUserManagement() {
			Builder.RegisterType<Stubs.InMemoryUserAccount>().As<IUserAccount>();

			Builder.RegisterType<Stubs.UserlessAuthenticator>().As<IAuthenticator>()
						.SingleInstance();
		}

		///<summary>Registers in-memory document services.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Optional config method")]
		private void StubDocuments() {
			Builder.RegisterType<Stubs.InMemoryStorage>().As<IStorageClient>()
				.SingleInstance();
			Builder.RegisterType<Stubs.SimpleProcessorController>().As<IProcessorController>()
				.SingleInstance();
		}

		///<summary>Registers a useless document processor.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Optional config method")]
		private void StubProcessor() {
			Builder.RegisterType<Stubs.UselessProcessor>().As<IDocumentProcessor>()
						.InstancePerDependency();
		}
		#endregion

		#region Azure
		///<summary>Registers a CloudStorageAccount for local development storage that requires Fiddler.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Optional config method")]
		private void FiddlerDevelopmentStorage() {
			const string baseDomain = "http://ipv4.fiddler";

			var account = new CloudStorageAccount(
				new StorageCredentialsAccountAndKey("devstoreaccount1", "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="),
				new Uri(baseDomain + ":10000/devstoreaccount1"), new Uri(baseDomain + ":10001/devstoreaccount1"), new Uri(baseDomain + ":10002/devstoreaccount1")
			);
			Builder.RegisterInstance(account);
		}
		///<summary>Registers the CloudStorageAccount for local development storage.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Optional config method")]
		private void DevelopmentStorage() {
			Builder.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
		}
		//TODO: Production storage

		///<summary>Registers document services that communicate using Azure.</summary>
		///<remarks>This must be called both by the worker roles and the web roles.</remarks>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Optional config method")]
		private void AzureDocuments() {
			Builder.RegisterType<Azure.AzureProcessorController>().As<IProcessorController>();
			Builder.RegisterType<Azure.AzureStorageClient>().As<IStorageClient>();

			Builder.RegisterType<Azure.AzureScanWorker>();
		}
		///<summary>Registers services that process documents from Azure in-memory.</summary>
		///<remarks>This is only meaningful in the web server (for debugging); worker roles don't use internal pools</remarks>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Optional config method")]
		private void InMemoryAzureProcessing() {
			Builder.RegisterType<Azure.InMemoryWorkerPool>();

			AddCallback(c => c.Resolve<Azure.InMemoryWorkerPool>().StartPool());
		}
		#endregion
	}
}
