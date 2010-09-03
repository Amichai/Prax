using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Prax.OcrEngine.Website.Resources {
	///<summary>Resolves resource sets to client URLs.</summary>
	public interface IResourceResolver {
		///<summary>Resolves a resource set to a set of URLs.</summary>
		///<param name="resourceSet">The resource set to resolve.</param>
		///<returns>A set of URLs to send to the client.</returns>
		IEnumerable<string> Resolve(ResourceSet resourceSet);
	}
}
