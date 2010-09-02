using System;
using System.Linq;
using System.Net;
using Autofac;
using Microsoft.WindowsAzure;
using Prax.OcrEngine.Services;
using Azure = Prax.OcrEngine.Services.Azure;
using Stubs = Prax.OcrEngine.Services.Stubs;
using Autofac.Core;

#region Explanation
/* This file contains all common configuration settings.
 * It has a single entry point which should be called to
 * set up the Autofac container.  (The Configure method)
 * 
 * The Configure method should in turn call various more
 * specific configuration methods, in the Configuration 
 * namespace.  These methods should configure single the
 * individual components or groups of components used by
 * the projects.
 * 
 * If a component has multiple implementations (eg, for 
 * debugging), each implementation should get a separate
 * extension method to configure it.  The configuration 
 * methods for the currently active componenents should 
 * be called by the Configure method.
 * 
 * This file is linked to by each project that needs to 
 * be configured. If a project's configuration has types
 * that other projects do not have, it should be wrapped
 * in #if blocks.  This creates a central repository of 
 * all configuation settings.
 */
#endregion

namespace Prax.OcrEngine {
	using Prax.OcrEngine.Configuration;	//Import the extension methods for specific configurations

	public static class CurrentConfig {
		///<summary>Configures an Autofac ContainerBuilder to use the current set of services and components.</summary>
		public static void Configure(this ContainerBuilder builder) {
			builder.StubUserManagement();

			builder.DevelopmentStorage();
			builder.AzureDocuments();

			//builder.InMemoryAzureProcessing();

			builder.StubProcessor();

			builder.RegisterType<DocumentManager>().As<IDocumentManager>();

#if WEB_ROLE
			//Add website-only services here
			builder.DebuggingResources();
#endif
		}
	}
}
#region Website
#if WEB_ROLE
namespace Prax.OcrEngine.Configuration {
	//Add website-only namespaces here (inside the conditional
	using Resources = Website.Resources;

	public static class WebConfigurations {
		///<summary>Registers debug-friendly ResourceResolvers that resolve resources uncompressed and un-combined.</summary>
		public static void DebuggingResources(this ContainerBuilder builder) {
			builder.RegisterInstance(new Resources.ScriptDebuggingResolver()).As<Resources.IResourceResolver>()
				.PropertiesAutowired();
			builder.RegisterInstance(new Resources.StylesheetDebuggingResolver()).As<Resources.IResourceResolver>()
				.PropertiesAutowired();
		}

		///<summary>Registers production-ready ResourceResolvers that resolve resources using a server-side cruncher.</summary>
		public static void LocalCrunchedResources(this ContainerBuilder builder) {
			builder.RegisterInstance(new Website.Resources.MSAjaxScriptMinifier()).As<Website.Resources.IMinifier>();
			builder.RegisterInstance(new Website.Resources.MSAjaxStylesheetMinifier()).As<Website.Resources.IMinifier>();

			//TODO: Cruncher
		}

		//TODO: CdnResources
	}
}
#endif
#endregion

namespace Prax.OcrEngine.Configuration {
	public static class StubConfigurations {
		///<summary>Registers in-memory user management services.</summary>
		public static void StubUserManagement(this ContainerBuilder builder) {
			builder.RegisterType<Stubs.InMemoryUserAccount>().As<IUserAccount>();

			builder.RegisterType<Stubs.UserlessAuthenticator>().As<IAuthenticator>()
						.SingleInstance();
		}

		///<summary>Registers in-memory document services.</summary>
		public static void StubDocuments(this ContainerBuilder builder) {
			builder.RegisterType<Stubs.InMemoryStorage>().As<IStorageClient>()
				.SingleInstance();
			builder.RegisterType<Stubs.SimpleProcessorController>().As<IProcessorController>()
				.SingleInstance();
		}

		public static void StubProcessor(this ContainerBuilder builder) {
			builder.RegisterType<Stubs.UselessProcessor>().As<IDocumentProcessor>()
						.InstancePerDependency();
		}
	}
	public static class AzureConfigurations {
		///<summary>Registers a CloudStorageAccount for local development storage that requires Fiddler.</summary>
		public static void FiddlerDevelopmentStorage(this ContainerBuilder builder) {
			const string baseDomain = "http://ipv4.fiddler";

			var account = new CloudStorageAccount(
				new StorageCredentialsAccountAndKey("devstoreaccount1", "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="),
				new Uri(baseDomain + ":10000/devstoreaccount1"), new Uri(baseDomain + ":10001/devstoreaccount1"), new Uri(baseDomain + ":10002/devstoreaccount1")
			);
			builder.RegisterInstance(account);
		}
		///<summary>Registers the CloudStorageAccount for local development storage.</summary>
		public static void DevelopmentStorage(this ContainerBuilder builder) {
			builder.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
		}
		//TODO: Production storage

		///<summary>Registers document services that communicate using Azure.</summary>
		///<remarks>This must be called both by the worker roles and the web roles.</remarks>
		public static void AzureDocuments(this ContainerBuilder builder) {
			builder.RegisterType<Azure.AzureProcessorController>().As<IProcessorController>();
			builder.RegisterType<Azure.AzureStorageClient>().As<IStorageClient>();

			builder.RegisterType<Azure.AzureScanWorker>();
		}
		///<summary>Registers services that process documents from Azure in-memory.</summary>
		///<remarks>This is only meaningful in the web server (for debugging); worker roles don't use internal pools</remarks>
		public static void InMemoryAzureProcessing(this ContainerBuilder builder) {
			builder.RegisterType<Azure.InMemoryWorkerPool>();

			//When the first ProcessorController is created, start the pool.
			builder.RegisterDependency<IProcessorController>("In-memory pool starter",
				c => c.Resolve<Azure.InMemoryWorkerPool>().StartPool()
			);
		}
	}
	#region Utils
	static class ConfigUtils {
		class Initializer {
			public Initializer(string name) { Name = name; }
			public string Name { get; private set; }
			public override string ToString() { return Name; }
		}
		///<summary>Registers an initializer method that will be called when first resolved.</summary>
		///<returns>A key object that can be resolved to run the initializer.</returns>
		private static Initializer RegisterInitializer(this ContainerBuilder builder, string name, Action<IComponentContext> initializer) {
			var key = new Initializer(name);

			builder.RegisterInstance(key)
				.Keyed<Initializer>(key)
				.OnPreparing(e => initializer(e.Context))
				.SingleInstance();

			return key;
		}
		///<summary>Registers a dependency for the given service type, running a delegate before the first instance is resolved.</summary>
		///<typeparam name="TService">The type that depends on the initializer.</typeparam>
		public static void RegisterDependency<TService>(this ContainerBuilder builder, string name, Action<IComponentContext> initializer) {
			var key = builder.RegisterInitializer(name, initializer);
			builder.RegisterCallback(cr => {
				foreach (var registration in cr.RegistrationsFor(new TypedService(typeof(TService)))) {
					registration.Preparing += (s, e) => e.Context.Resolve<Initializer>(key);
				}

				cr.Registered += (s, e) => {
					if (e.ComponentRegistration.Services.Contains(new TypedService(typeof(TService))))
						e.ComponentRegistration.Preparing += (s2, e2) => e2.Context.Resolve<Initializer>(key);
				};
			});
		}
	#endregion
	}
}
