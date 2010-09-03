using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prax.OcrEngine.Website.Resources;

namespace Prax.OcrEngine.Website.Controllers {
	public class ResourcesController : Controller {
		//The actions in this controller are called
		//by ResourceCombiningResolver.  The action
		//names must match the resource types.

		IResourceService<IResourceLocator> locators;
		IResourceService<IMinifier> minifiers;
		public ResourcesController(IResourceService<IResourceLocator> locators, IResourceService<IMinifier> minifiers) {
			this.locators = locators;
			this.minifiers = minifiers;
		}

		IEnumerable<string> Combine(ResourceSet set) {
			var locator = locators[set.Type];
			var minifier = minifiers[set.Type];

			return set.Names.Select(vp => minifier.Minify(Server.MapPath(locator.GetVirtualPath(vp))));
		}

		[CLSCompliant(false)]	//Differs only in case
		public ActionResult Javascript(string id) {
			var set = ResourceSet.Get(id);
			if (set == null || set.Type != ResourceType.Javascript)
				throw new HttpException(404, "Not found");

			return new FarFutureContentResult("text/javascript", Combine(set));
		}
		public ActionResult Css(string id) {
			var set = ResourceSet.Get(id);
			if (set == null || set.Type != ResourceType.Css)
				throw new HttpException(404, "Not found");

			return new FarFutureContentResult("text/css", Combine(set));
		}

		class FarFutureContentResult : ActionResult {
			readonly string contentType;
			readonly IEnumerable<string> contents;
			public FarFutureContentResult(string contentType, IEnumerable<string> contents) {
				this.contentType = contentType;
				this.contents = contents;
			}

			public override void ExecuteResult(ControllerContext context) {
				var response = context.HttpContext.Response;

				//TODO: ETag? GZip?

				response.ContentType = contentType;
				response.ExpiresAbsolute = DateTime.UtcNow.AddYears(10);
				response.Cache.SetCacheability(HttpCacheability.Public);
				response.Cache.SetMaxAge(TimeSpan.FromDays(3650));

				foreach (var s in contents) {
					response.Write(s);
					response.Write(Environment.NewLine);
				}
			}
		}
	}
}
