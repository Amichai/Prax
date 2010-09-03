using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac.Builder;

namespace Prax.OcrEngine.Website.Resources {
	///<summary>Locates services that have different implementations for different resource types.</summary>
	public interface IResourceService<out TService> : IHideObjectMembers {
		TService this[ResourceType type] { get; }
	}
}