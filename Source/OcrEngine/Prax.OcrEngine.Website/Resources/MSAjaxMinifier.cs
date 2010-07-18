using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Ajax.Utilities;
using System.IO;

namespace Prax.OcrEngine.Website.Resources {
	abstract class MSAjaxMinifier : IMinifier {
		[ThreadStatic]
		static Minifier minifier;
		protected static Minifier Minifier { get { return minifier = minifier ?? new Minifier(); } }

		public string Minify(string sourcePath) { return ProcessSource(File.ReadAllText(sourcePath)); }
		protected abstract string ProcessSource(string source);

		public abstract ResourceType Type { get; }
	}

	class MSAjaxScriptMinifier : MSAjaxMinifier {
		static readonly CodeSettings Settings = new CodeSettings { InlineSafeStrings = false, CombineDuplicateLiterals = true, };

		public override ResourceType Type { get { return ResourceType.Javascript; } }
		protected override string ProcessSource(string source) { return Minifier.MinifyJavaScript(source, Settings); }
	}
	class MSAjaxStylesheetMinifier : MSAjaxMinifier {
		public override ResourceType Type { get { return ResourceType.Css; } }
		protected override string ProcessSource(string source) { return Minifier.MinifyStyleSheet(source); }
	}
}