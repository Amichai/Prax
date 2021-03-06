﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Prax.OcrEngine.Services.Azure {
	static class Utils {
		public static byte[] SHA512Hash(this FileInfo file) {
			using (var hasher = new SHA512Managed())
			using (var stream = file.OpenRead())
				return hasher.ComputeHash(stream);
		}
	}
	static class IdUtils {
		public static string FileName(this DocumentIdentifier id) {
			return id.UserId + "/" + id.DocumentId;
		}
		public static DocumentIdentifier ParseName(string filename) {
			if (filename == null) throw new ArgumentNullException("filename");

			var slash = filename.IndexOf('/');
			return new DocumentIdentifier(Guid.Parse(filename.Remove(slash)), Guid.Parse(filename.Substring(slash + 1)));
		}
		public static DocumentIdentifier ParseUri(Uri uri) {
			var parts = uri.Segments;
			return new DocumentIdentifier(Guid.Parse(parts[parts.Length - 2].TrimEnd('/')), Guid.Parse(parts[parts.Length - 1]));
		}
	}
}
