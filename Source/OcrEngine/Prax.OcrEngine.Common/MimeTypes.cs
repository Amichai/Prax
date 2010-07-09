using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine {
	///<summary>Finds MIME types for content.</summary>
	public static class MimeTypes {
		static readonly Dictionary<string, string> ExtensionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
			{ ".jpg",	"image/jpeg" },
			{ ".jpeg",	"image/jpeg" },
			{ ".jpe",	"image/jpeg" },

			{ ".gif",	"image/gif" },
			{ ".png",	"image/png" },
			{ ".bmp",	"image/bmp" },

			{ ".pdf",	"application/pdf" },
		};

		///<summary>Gets the MIME type for the given file extension.</summary>
		public static string ForExtension(string extension) {
			if (extension[0] != '.')
				extension = "." + extension;
			string retVal;
			if (ExtensionMap.TryGetValue(extension, out retVal))
				return retVal;
			return "application/octet-stream";
		}
	}
}
