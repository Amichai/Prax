using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Autofac;
using Stubs = Prax.OcrEngine.Services.Stubs;
using Prax.OcrEngine.Services;

namespace Prax.OcrEngine.RecognitionClient {
	class App : Application {

		[STAThread]
		static void Main(string[] args) {
			var builder = new ContainerBuilder();

			builder.RegisterInstance(new Stubs.FixedTextConverter("OCR Results go here")).As<IResultsConverter>();
			builder.RegisterInstance(new Stubs.EmptyPdfConverter()).As<IResultsConverter>();
			builder.RegisterType<Stubs.UselessProcessor>().As<IDocumentProcessor>().InstancePerDependency();

			builder.RegisterType<MainWindow>();

			var container = builder.Build();

			var app = new App();
			app.Run(container.Resolve<MainWindow>());
		}
	}
}
