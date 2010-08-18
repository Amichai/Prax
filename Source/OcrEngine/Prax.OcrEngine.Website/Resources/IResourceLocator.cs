using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Prax.OcrEngine.Website.Resources {
	///<summary>Resolves resource names	on disk.</summary>
	public interface IResourceResolver {
		///<summary>Gets the type of resource that this resolver can resolver.</summary>
		ResourceType Type { get; }

		///<summary>Resolves a resource set to a set of URLs.</summary>
		///<param name="resourceSet">The resource set to resolve.</param>
		///<returns>A set of URLs to send to the client.</returns>
		IEnumerable<string> Resolve(ResourceSet resourceSet);
	}
}
