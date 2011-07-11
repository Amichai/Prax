using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using SimpleWebDemo;

namespace Prax.OcrEngine {
	public partial class Config {
		partial void RegisterLocalServies() {
			Builder.RegisterInstance(new MsSampleImageRenderer("Times New Roman")).As<IImageCreator>();
		}
	}
}