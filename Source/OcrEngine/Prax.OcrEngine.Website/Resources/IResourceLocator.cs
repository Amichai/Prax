using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prax.OcrEngine.Website.Resources {
	///<summary>Locates individual resource names.</summary>
	public interface IResourceLocator {
		///<summary>Gets the type of resource that this locator can locate.</summary>
		ResourceType ResourceType { get; }

		///<summary>Locates a single resource.</summary>
		///<param name="resourceSet">The name of the resource.</param>
		///<returns>An application-relative virtual path (Beginning with `~/`).</returns>
		string GetVirtualPath(string name);
	}
}