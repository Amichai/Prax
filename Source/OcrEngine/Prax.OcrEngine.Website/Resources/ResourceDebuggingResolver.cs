using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;

namespace Prax.OcrEngine.Website.Resources {
	///<summary>Resolves uncompressed local resources for debugging.</summary>
	public abstract class ResourceDebuggingResolver : IResourceResolver {
		///<summary>Gets or sets the RequestContext used to resolve URLs.  (Injected)</summary>
		public RequestContext RequestContext { get; set; }

		///<summary>Gets the type of resource that this resolver can resolver.</summary>
		public abstract ResourceType Type { get; }

		///<summary>Gets the virtual path that contains the files that this class will resolve.</summary>
		protected abstract string SearchPath { get; }


		///<summary>Resolves a resource set to a set of URLs</summary>
		///<param name="url">The UrlHelper used to resolve client URLs.</param>
		///<param name="set">The resource set to resolve.</param>
		///<returns>A set of URLs to send to the client.</returns>
		public IEnumerable<string> Resolve(ResourceSet set) {
			if (set == null) throw new ArgumentNullException("set");

			if (set.Type != Type) throw new ArgumentException(GetType() + " cannot resolve " + set.Type + " resources.", "set");

			var url = new UrlHelper(RequestContext);
			foreach (var name in set.Names) {
				var virtualPath = Path.Combine(SearchPath, name + Type.Extension());
				var filePath = url.RequestContext.HttpContext.Server.MapPath(virtualPath);

				var timestamp = File.GetLastWriteTimeUtc(filePath);

				yield return url.Content(virtualPath) + "?Timestamp=" + timestamp.Ticks;
			}
		}
	}

	public class ScriptDebuggingResolver : ResourceDebuggingResolver {
		public override ResourceType Type { get { return ResourceType.Javascript; } }

		protected override string SearchPath { get { return "~/Content/Javascript"; } }
	}
	public class StylesheetDebuggingResolver : ResourceDebuggingResolver {
		public override ResourceType Type { get { return ResourceType.Css; } }

		protected override string SearchPath { get { return "~/Content/CSS"; } }
	}
}