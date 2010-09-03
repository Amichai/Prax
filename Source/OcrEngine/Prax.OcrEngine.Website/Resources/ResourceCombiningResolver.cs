using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace Prax.OcrEngine.Website.Resources {
	///<summary>Resolves resources that are combined & minified by server-side code.</summary>
	public class ResourceCombiningResolver : IResourceResolver {
		readonly UrlHelper url;
		readonly HttpServerUtilityBase server;
		readonly IResourceService<IResourceLocator> locators;
		public ResourceCombiningResolver(IResourceService<IResourceLocator> locators, UrlHelper url, HttpServerUtilityBase server) {
			this.url = url;
			this.locators = locators;
			this.server = server;
		}

		public IEnumerable<string> Resolve(ResourceSet resourceSet) {
			var locator = locators[resourceSet.Type];

			//If any file changes, I need a new URL.
			//Therefore, I add all of the timestamps
			//of all of the files.  Since they will 
			//never decrease, there is no risk that 
			//two changes might cancel eachother out
			var version = resourceSet.Names.Sum(vp => File.GetLastWriteTimeUtc(server.MapPath(locator.GetVirtualPath(vp))).Ticks);
			yield return url.Action(resourceSet.Type.ToString(), "Resources", new { id = resourceSet.SetName, version });
		}
	}
}