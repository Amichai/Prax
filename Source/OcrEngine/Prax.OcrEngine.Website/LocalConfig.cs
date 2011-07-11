using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac.Core;

using Autofac.Features.Indexed;
using Autofac;
using System.Diagnostics.CodeAnalysis;
using Prax.OcrEngine.Website.Resources;
using Prax.OcrEngine.Website;
using System;

namespace Prax.OcrEngine {
	partial class Config {
		partial void RegisterLocalServies() {
			ResourcesSetup();
			StandardResourceLocators();
			DebuggingResources();
			//LocalCrunchedResources();

		}

		#region Resources
		///<summary>Registers basic services used by the resources framework.</summary>
		private void ResourcesSetup() {
			Builder.RegisterGeneric(typeof(AutofacResourceService<>)).As(typeof(IResourceService<>)).SingleInstance();
			AddCallback(c => Website.Views.ResourcesExtensions.Container = c);
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
	}
}