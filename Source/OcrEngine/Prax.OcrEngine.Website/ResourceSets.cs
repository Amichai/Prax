using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Prax.OcrEngine.Website {
	///<summary>A named set of external resources for a web page.</summary>
	[ImmutableObject(true)]
	public class ResourceSet {
		static List<ResourceSet> writableSets = new List<ResourceSet>();
		///<summary>Gets a collection of all of the defined resource sets.</summary>
		public static readonly ReadOnlyCollection<ResourceSet> All = new ReadOnlyCollection<ResourceSet>(writableSets);

		public static readonly ResourceSet GlobalCss = new ResourceSet(ResourceType.Css, "GlobalCss", "YUI-Reset", "Site");

		///<summary>Creates a ResourceSet instance.</summary>
		public ResourceSet(ResourceType type, string setName, params string[] names) : this(type, setName, (IEnumerable<string>)names) { }
		///<summary>Creates a ResourceSet instance.</summary>
		public ResourceSet(ResourceType type, string setName, IEnumerable<string> names) {
			Type = type;
			SetName = setName;
			Names = new ReadOnlyCollection<string>(names.ToArray());

			writableSets.Add(this);
		}

		///<summary>Gets the type of resource.</summary>
		public ResourceType Type { get; private set; }

		///<summary>Gets the name of this resource set.</summary>
		public string SetName { get; private set; }

		///<summary>Gets the names of the resources in the set.</summary>
		public ReadOnlyCollection<string> Names { get; private set; }
	}
	///<summary>A type of external resource for a web page.</summary>
	public enum ResourceType {
		///<summary>A Javascript script.</summary>
		Javascript,
		///<summary>A CSS stylesheet.</summary>
		Css
	}

	///<summary>Contains extension methods used for resources.</summary>
	public static class ResourceExtensions {
		///<summary>Gets the file extension of a resource type, including the initial dot.</summary>
		public static string Extension(this ResourceType type) {
			switch (type) {
				case ResourceType.Javascript: return ".js";
				case ResourceType.Css: return ".css";
				default: throw new InvalidEnumArgumentException("type", (int)type, typeof(ResourceType));
			}
		}
	}
}