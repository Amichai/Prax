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
			{ ".txt",	"text/plain" },
		};

		const string DefaultType = "application/octet-stream";
		///<summary>Gets the MIME type for the given file extension.</summary>
		public static string ForExtension(string extension) {
			if (string.IsNullOrEmpty(extension))
				return DefaultType;

			if (extension[0] != '.')
				extension = "." + extension;
			string retVal;
			if (ExtensionMap.TryGetValue(extension, out retVal))
				return retVal;

			return DefaultType;
		}

		///<summary>Gets the default extension for the given MIME type.</summary>
		public static string GetExtension(string mimeType) {
			//TODO: Reverse map with preferred extension
			return ExtensionMap.FirstOrDefault(kvp => kvp.Value.Equals(mimeType, StringComparison.OrdinalIgnoreCase)).Key ?? "";
		}
	}
}
