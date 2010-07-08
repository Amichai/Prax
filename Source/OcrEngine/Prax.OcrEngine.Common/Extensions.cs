using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Globalization;

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

		static readonly string[] sizes = { "bytes", "KB", "MB", "GB", "TB" };
		///<summary>Converts a number of bytes to a string in the appropriate unit.</summary>
		public static string ToSizeString(this long size) {
			int order = 0;
			while (size >= 1024 && order + 1 < sizes.Length) {
				order++;
				size /= 1024;
			}

			return String.Format(CultureInfo.CurrentCulture, "{0:0.#} {1}", size, sizes[order]);
		}
	}
}