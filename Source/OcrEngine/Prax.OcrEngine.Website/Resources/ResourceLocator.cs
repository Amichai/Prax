using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace Prax.OcrEngine.Website.Resources {
	///<summary>Locates resources in the Content folder.</summary>
	public class ResourceLocator : IResourceLocator {
		public ResourceLocator(ResourceType type, string folder) {
			ParentFolder = folder;
			ResourceType = type;
		}

		public string ParentFolder { get; private set; }
		public ResourceType ResourceType { get; private set; }

		public string GetVirtualPath(string name) {
			var path = VirtualPathUtility.Combine(VirtualPathUtility.AppendTrailingSlash(ParentFolder), name) + ResourceType.Extension();

			Debug.Assert(File.Exists(HttpContext.Current.Server.MapPath(path)));
			return path;
		}
	}
}