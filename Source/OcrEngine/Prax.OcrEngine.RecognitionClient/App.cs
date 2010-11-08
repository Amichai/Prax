using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Autofac;
using Stubs = Prax.OcrEngine.Services.Stubs;
using Prax.OcrEngine.Services;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.RecognitionClient {
	class App : Application {
		static readonly ReadOnlyCollection<IResultsConverter> resultConverters = new ReadOnlyCollection<IResultsConverter>(
			new IResultsConverter[] { new Stubs.EmptyPdfConverter(), new Stubs.FixedTextConverter("OCR Results go here") }
		);

		public static readonly object bindableConverters = Converters.Select(c => new {
			ToolTip = "Open results as a " + c.OutputFormat.GetExtension().ToUpperInvariant() + " file",
			ImagePath = @"Images\" + c.OutputFormat.GetExtension().Substring(1).ToUpperInvariant() + "16.png",
			Converter = c
		}).ToList();

		public static object BindableConverters { get { return bindableConverters; } }

		public static ReadOnlyCollection<IResultsConverter> Converters { get { return resultConverters; } }

		[STAThread]
		static void Main() {
			var builder = new ContainerBuilder();

			foreach (var converter in resultConverters)
				builder.RegisterInstance(converter).As<IResultsConverter>();

			builder.RegisterType<Stubs.UselessProcessor>().As<IDocumentProcessor>().InstancePerDependency();

			builder.RegisterType<MainWindow>();

			var container = builder.Build();

			var app = new App();
			app.Run(container.Resolve<MainWindow>());
		}
	}
}
