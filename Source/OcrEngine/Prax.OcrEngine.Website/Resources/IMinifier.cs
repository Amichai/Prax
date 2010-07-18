using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prax.OcrEngine.Website.Resources {
	///<summary>Compresses external resources.</summary>
	public interface IMinifier {
		///<summary>Gets the type of resource that this minifier can process.</summary>
		ResourceType Type { get; }


		///<summary>Minifies a file.</summary>
		///<param name="sourcePath">The path to the input file on disk.</param>
		///<returns>A string containing the shrunken contents of the file.</returns>
		string Minify(string sourcePath);
	}
}