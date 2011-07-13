using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac;
using Microsoft.WindowsAzure;
using Prax.OcrEngine.Services;
using Azure = Prax.OcrEngine.Services.Azure;
using Stubs = Prax.OcrEngine.Services.Stubs;
using Prax.OcrEngine.Engine.ReferenceData;

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
 * that other projects do not have, it should be placed 
 * in a LocalConfig.cs file in the RegisterLocalServies 
 * partial method.
 */
#endregion

namespace Prax.OcrEngine {

	public partial class Config {
		private void RegisterServices() {
			CoreServices();

			StubUserManagement();

			StubDocuments();

			//StubRecognizer();

			//DevelopmentStorage();
			//AzureDocuments();

			//if (!RoleEnvironment.IsAvailable)
			    //InMemoryAzureProcessing();	//If we're not running in Azure, start some fake workers.

			OriginalProcessor();
			StubConverters();

#if WEB
			//Add services used by all websites here
			MvcSetup();
#endif
			RegisterLocalServies();
		}
		partial void RegisterLocalServies();


		public static Config CreateCurrent() {
			var config = new Config();
			config.RegisterServices();
			return config;
		}

		public ContainerBuilder Builder { get; private set; }
		private Config() {
			Builder = new ContainerBuilder();
		}

		List<Action<IContainer>> creationCallbacks = new List<Action<IContainer>>();
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by optional config method")]
		void AddCallback(Action<IContainer> callback) { creationCallbacks.Add(callback); }

		public IContainer CreateContainer() {
			var container = Builder.Build();
			creationCallbacks.ForEach(c => c(container));
			return container;
		}
	}
}

#region Website
#if WEB
namespace Prax.OcrEngine {
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using Autofac.Integration.Mvc;

	public partial class Config {
		///<summary>Sets up the Autofac container for the ASP.Net MVC framework.</summary>
		private void MvcSetup() {
			Builder.RegisterControllers(typeof(Config).Assembly);

			Builder.RegisterModule<AutofacWebTypesModule>();

			ControllerBuilder.Current.SetControllerFactory(new InjectingControllerFactory());
			Builder.Register(cc => requestContext).As<RequestContext>();

			Builder.RegisterType<UrlHelper>().As<UrlHelper>();
			Builder.RegisterType<HtmlHelper>().As<HtmlHelper>();

			AddCallback(c => DependencyResolver.SetResolver(new AutofacDependencyResolver(c)));
		}

		[ThreadStatic]
		static RequestContext requestContext;

		///<summary>A ControllerFactory that sets the requestContext field to the current RequestContext, for use with AutoFac.</summary>
		class InjectingControllerFactory : DefaultControllerFactory {
			protected override IController GetControllerInstance(RequestContext context, Type controllerType) {
				requestContext = context;
				return base.GetControllerInstance(context, controllerType);
			}

			public override void ReleaseController(IController controller) {
				base.ReleaseController(controller);
				requestContext = null;
			}
		}
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

		///<summary>Registers a DocumentExecutor that performs OCR in-process.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by optional config method")]
		private void LocalSimpleDocumentExecutor() {
			Builder.RegisterType<Stubs.SimpleDocumentExecutor>().As<IDocumentExecutor>().SingleInstance();
		}
		///<summary>Registers a DocumentExecutor that performs OCR in-process.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by optional config method")]
		private void LocalThreadedDocumentExecutor() {
			Builder.RegisterType<ThreadedDocumentExecutor>().As<IDocumentExecutor>().SingleInstance();
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
		}

		///<summary>Registers a useless document recognizer.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Optional config method")]
		private void StubRecognizer() {
			Builder.RegisterType<Stubs.UselessRecognizer>().As<IDocumentRecognizer>()
						.InstancePerDependency();
		}

		///<summary>Registers useless result converters.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Optional config method")]
		private void StubConverters() {
			Builder.RegisterInstance(new Stubs.FixedTextConverter("OCR Results go here")).As<IResultsConverter>();
			Builder.RegisterInstance(new Stubs.EmptyPdfConverter()).As<IResultsConverter>();
		}
		#endregion

		#region Original Engine
		private void OriginalProcessor() {
			Builder.RegisterInstance(new Stubs.StaticDataCache(Environment.ExpandEnvironmentVariables(@"%TEMP%\PadOcrTraining")))
			//Builder.RegisterInstance(new Stubs.StaticDataCache(Environment.ExpandEnvironmentVariables(@"C:\Users\Amichai\Documents\Prax\PadOcrTraining")))
				   .As<IDataCache>()
				   .Named("TrainingData", typeof(IDataCache));

			Builder.Register(c => {
				var cache = c.Resolve<IDataCache>();// Named<IDataCache>("TrainingData", null);
				cache.Update();
				var set = new MutableReferenceSet();
				set.ReadFrom(cache.LocalPath);
				return set;
			}).As<IReferenceSet>().SingleInstance();

			Builder.RegisterType<ReferenceSearcher>().As<IReferenceSearcher>().SingleInstance();

			Builder.RegisterType<Engine.OriginalDocumentRecognizer>().As<IDocumentRecognizer>()
					.InstancePerDependency();
		}
		#endregion

		#region Azure
		///<summary>Registers a DocumentExecuter that executes OCR remotely using an Azure queue.  Must not be called on the worker role.</summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Optional config method")]
		private void AzureRemoteDocumentExecutor() {
			Builder.RegisterType<Services.Azure.AzureDocumentExecutor>().As<IDocumentExecutor>()
					.SingleInstance();
		}

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
