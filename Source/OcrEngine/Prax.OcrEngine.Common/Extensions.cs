using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Globalization;
using System.IO;

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


		///<summary>Gets the progress of an IDocumentProcesor as a number between 0 and 100.</summary>
		public static int ProgressPercentage(this Services.IDocumentProcessor processor) {
			if (processor == null) throw new ArgumentNullException("processor");
			return (int)(100 * (double)processor.CurrentProgress / processor.MaximumProgress);
		}
	}

}