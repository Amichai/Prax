using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using Prax.OcrEngine.Website.Resources;

namespace Prax.OcrEngine.Website.Views {
	///<summary>Contains extensions methods that render resource links in MVC views.</summary>
	public static class ResourcesExtensions {
		///<summary>Gets the Autofac container used to resolve dependencies.</summary>
		///<remarks>I added this property since Autofac cannot build up a static class.
		///In general, manual use of the Autofac container should be avoided.</remarks>
		public static Autofac.IComponentContext Container { get; set; }

		///<summary>Returns tags that reference a resource set.</summary>
		public static MvcHtmlString Scripts(this HtmlHelper html, ResourceSet set) { return html.ResourceSet(set); }
		///<summary>Returns tags that reference a resource set.</summary>
		public static MvcHtmlString Stylesheets(this HtmlHelper html, ResourceSet set) { return html.ResourceSet(set); }

		///<summary>Returns tags that reference a resource set.</summary>
		public static MvcHtmlString ResourceSet(this HtmlHelper html, ResourceSet set) {
			if (html == null) throw new ArgumentNullException("html");
			if (set == null) throw new ArgumentNullException("set");

			var resolver = Container.Resolve<IResourceResolver>();

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