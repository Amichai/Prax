using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using Autofac.Integration.Web;
using System.Web.Mvc;
using Prax.OcrEngine.Website.Resources;
using System.Text;
using System.ComponentModel;

namespace Prax.OcrEngine.Website.Views {
	///<summary>Contains extensions methods used by MVC views.</summary>
	public static class ViewsExtensions {
		static Autofac.IComponentContext container;

		///<summary>Gets the Autofac container used to resolve dependencies.</summary>
		///<remarks>I added this property since Autofac cannot build up a static class.
		///In general, manual use of the Autofac container should be avoided.</remarks>
		public static Autofac.IComponentContext Container {
			get {
				return container ??	//I might want to explicitly set this in Global.asax.cs
					   (container = ((IContainerProviderAccessor)HttpContext.Current.ApplicationInstance).ContainerProvider.RequestLifetime);
			}
			set { container = value; }	//Useful for unit tests, where there is no HttpContext
		}

		///<summary>Returns tags that reference a resource set.</summary>
		public static MvcHtmlString Scripts(this HtmlHelper html, ResourceSet set) { return html.ResourceSet(set); }
		///<summary>Returns tags that reference a resource set.</summary>
		public static MvcHtmlString Stylesheets(this HtmlHelper html, ResourceSet set) { return html.ResourceSet(set); }

		///<summary>Returns tags that reference a resource set.</summary>
		public static MvcHtmlString ResourceSet(this HtmlHelper html, ResourceSet set) {
			if (html == null) throw new ArgumentNullException("html");
			if (set == null) throw new ArgumentNullException("set");

			var resolver = Container.Resolve<IEnumerable<IResourceResolver>>().First(r => r.Type == set.Type);

			return MvcHtmlString.Create(
				String.Join(Environment.NewLine,
					resolver.Resolve(set).Select(url => set.Type.CreateTag(url))
				)
			);
		}

		///<summary>Creates an HTML tag referencing an external resource.</summary>
		///<param name="type">The type of resource to reference.</param>
		///<param name="url">The URL to the external resource.</param>
		static string CreateTag(this ResourceType type, string url) {
			TagBuilder builder;
			switch (type) {
				case ResourceType.Javascript:
					builder = new TagBuilder("script");
					builder.Attributes.Add("type", "text/javascript");
					builder.Attributes.Add("src", url);
					return builder.ToString();

				case ResourceType.Css:
					builder = new TagBuilder("link");
					builder.Attributes.Add("rel", "stylesheet");
					builder.Attributes.Add("type", "text/css");
					builder.Attributes.Add("href", url);
					return builder.ToString();

				default: throw new InvalidEnumArgumentException("type", (int)type, typeof(ResourceType));
			}
		}
	}
}