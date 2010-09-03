using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;

namespace Prax.OcrEngine.Website.Resources {
	///<summary>Resolves uncompressed local resources for debugging.</summary>
	public class ResourceDebuggingResolver : IResourceResolver {
		readonly UrlHelper url;
		readonly IResourceService<IResourceLocator> locators;
		public ResourceDebuggingResolver(IResourceService<IResourceLocator> locators, UrlHelper url) {
			this.url = url;
			this.locators = locators;
		}

		///<summary>Resolves a resource set to a set of URLs</summary>
		///<param name="url">The UrlHelper used to resolve client URLs.</param>
		///<param name="resourceSet">The resource set to resolve.</param>
		///<returns>A set of URLs to send to the client.</returns>
		public IEnumerable<string> Resolve(ResourceSet resourceSet) {
			if (resourceSet == null) throw new ArgumentNullException("resourceSet");

			foreach (var name in resourceSet.Names) {
				var virtualPath = locators[resourceSet.Type].GetVirtualPath(name);
				var filePath = url.RequestContext.HttpContext.Server.MapPath(virtualPath);

				var timestamp = File.GetLastWriteTimeUtc(filePath);

				yield return url.Content(virtualPath) + "?Timestamp=" + timestamp.Ticks;
			}
		}
	}
}