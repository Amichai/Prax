using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Globalization;
using System.IO;
using Prax.OcrEngine.Services;
using System.ComponentModel;
using System.Text;

namespace Prax.OcrEngine {
	///<summary>Contains useful extension methods.</summary>
	///<remarks>After all, it wouldn't contain useless extension methods...</remarks>
	public static class Extensions {
		#region Reflection
		///<summary>Checks whether a given type is a nullable type.</summary>
		///<returns>True if the type is a nullable value type.</returns>
		public static bool IsNullable(this Type type) { return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>); }

		///<summary>Gets a custom attribute defined on a member.</summary>
		///<typeparam name="TAttribute">The type of attribute to return.</typeparam>
		///<param name="provider">The object to get the attribute for.</param>
		///<returns>The first attribute of the type defined on the member, or null if there aren't any</returns>
		public static TAttribute GetCustomAttribute<TAttribute>(this ICustomAttributeProvider provider) where TAttribute : Attribute {
			return provider.GetCustomAttribute<TAttribute>(false);
		}
		///<summary>Gets the first custom attribute defined on a member, or null if there aren't any.</summary>
		///<typeparam name="TAttribute">The type of attribute to return.</typeparam>
		///<param name="provider">The object to get the attribute for.</param>
		///<param name="inherit">Whether to look up the hierarchy chain for attributes.</param>
		///<returns>The first attribute of the type defined on the member, or null if there aren't any</returns>
		public static TAttribute GetCustomAttribute<TAttribute>(this ICustomAttributeProvider provider, bool inherit) where TAttribute : Attribute {
			return provider.GetCustomAttributes<TAttribute>(inherit).FirstOrDefault();
		}
		///<summary>Gets the custom attributes defined on a member.</summary>
		///<typeparam name="TAttribute">The type of attribute to return.</typeparam>
		///<param name="provider">The object to get the attribute for.</param>
		public static TAttribute[] GetCustomAttributes<TAttribute>(this ICustomAttributeProvider provider) where TAttribute : Attribute {
			return provider.GetCustomAttributes<TAttribute>(false);
		}
		///<summary>Gets the custom attributes defined on a member.</summary>
		///<typeparam name="TAttribute">The type of attribute to return.</typeparam>
		///<param name="provider">The object to get the attribute for.</param>
		///<param name="inherit">Whether to look up the hierarchy chain for attributes.</param>
		public static TAttribute[] GetCustomAttributes<TAttribute>(this ICustomAttributeProvider provider, bool inherit) where TAttribute : Attribute {
			if (provider == null) throw new ArgumentNullException("provider");

			return (TAttribute[])provider.GetCustomAttributes(typeof(TAttribute), inherit);
		}
		#endregion

		#region Streams
		///<summary>Fills a byte array from a stream.</summary>
		///<returns>The number of bytes read.  If the end of the stream was reached, this will be less than the size of the array.</returns>
		///<remarks>Stream.Read is not guaranteed to read length bytes even if it doesn't hit the end of the stream, so I wrote this method, which is.</remarks>
		public static int ReadFill(this Stream stream, byte[] buffer) { return stream.ReadFill(buffer, buffer.Length); }
		///<summary>Reads a given number of bytes into a byte array from a stream.</summary>
		///<returns>The number of bytes read.  If the end of the stream was reached, this will be less than the length.</returns>
		///<remarks>Stream.Read is not guaranteed to read length bytes even if it doesn't hit the end of the stream, so I wrote this method, which is.</remarks>
		public static int ReadFill(this Stream stream, byte[] buffer, int length) {
			if (stream == null) throw new ArgumentNullException("stream");
			if (buffer == null) throw new ArgumentNullException("buffer");

			int position = 0;
			while (position < length) {
				var bytesRead = stream.Read(buffer, position, length - position);
				if (bytesRead == 0) break;
				position += bytesRead;
			}
			return position;
		}
		#endregion

		#region Collections
		///<summary>Filters a set of strings to those that match elements of an enum.</summary>
		public static IEnumerable<TEnum> AsEnum<TEnum>(this IEnumerable<string> source, bool ignoreCase = false) where TEnum : struct {
			foreach (var str in source) {
				TEnum value;
				if (Enum.TryParse<TEnum>(str, ignoreCase, out value))
					yield return value;
			}
		}
		///<summary>Adds zero or more items to a collection.</summary>
		public static void AddRange<TItem, TElement>(this ICollection<TElement> collection, params TItem[] items) where TItem : TElement { collection.AddRange((IEnumerable<TItem>)items); }
		///<summary>Adds zero or more items to a collection.</summary>
		public static void AddRange<TItem, TElement>(this ICollection<TElement> collection, IEnumerable<TItem> items)
			where TItem : TElement {
			if (collection == null) throw new ArgumentNullException("collection");
			if (items == null) throw new ArgumentNullException("items");

			foreach (var item in items)
				collection.Add(item);
		}
		#endregion

		static readonly string[] sizes = { "bytes", "KB", "MB", "GB", "TB" };
		///<summary>Converts a number of bytes to a string in the appropriate unit.</summary>
		public static string ToSizeString(this long size) {
			double shrunkenSize = size;		//Switch to double to preserve the decimal.
			int order = 0;
			while (shrunkenSize >= 1024 && order + 1 < sizes.Length) {
				order++;
				shrunkenSize /= 1024;
			}

			return String.Format(CultureInfo.CurrentCulture, "{0:0.#} {1}", shrunkenSize, sizes[order]);
		}

		///<summary>Gets the file extension of a result format.</summary>
		public static string GetExtension(this ResultFormat format) {
			switch (format) {
				case ResultFormat.PlainText:
					return ".txt";
				case ResultFormat.Pdf:
					return ".pdf";
				default:
					throw new InvalidEnumArgumentException("format", (int)format, typeof(ResultFormat));
			}
		}
		///<summary>Gets the MIME type of a result format.</summary>
		public static string GetMimeType(this ResultFormat format) { return MimeTypes.ForExtension(format.GetExtension()); }

		public static void UploadString(this Document doc, string name, string content) {
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
				doc.UploadStream(name, stream, stream.Length);
		}
	}
}