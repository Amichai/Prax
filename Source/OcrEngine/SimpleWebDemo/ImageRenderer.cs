using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Documents;
using System.Threading;
using System.Reflection;
using System.Xml.Linq;
using SimpleWebDemo.HtmlToXaml;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Controls.Primitives;

namespace SimpleWebDemo {
	//If you make an IImageCreator that doesn't use a hand-built HTML renderer, you MUST whitelist the HTML!

	public interface IImageCreator {
		string ContentType { get; }
		Stream CreateImage(string html);
	}

	public abstract class WpfImageRenderer : IImageCreator {
		public string ContentType { get { return "image/png"; } }

		public Stream CreateImage(string html) {
			Stream retVal = null;
			Exception innerEx = null;

			var thread = new Thread(() => {
				try {
					var c = CreateControl(html);
					retVal = RenderImage(c);
				} catch (Exception ex) { innerEx = ex; }
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
			if (innerEx != null)
				throw new TargetInvocationException(innerEx);
			retVal.Position = 0;
			return retVal;
		}

		private Stream RenderImage(FrameworkElement c) {
			c.Measure(new Size(c.Width, Double.IsNaN(c.Height) ? 9999 : c.Height));
			c.Arrange(new Rect(c.DesiredSize));

			RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)c.ActualWidth,
																	 (int)c.ActualHeight, 96, 96, PixelFormats.Pbgra32);

			renderBitmap.Render(c);
			//DrawingVisual visual = new DrawingVisual();
			//using (DrawingContext context = visual.RenderOpen()) {
			//    VisualBrush brush = new VisualBrush(c);
			//    context.DrawRectangle(brush, null, new Rect(c.RenderSize));
			//}

			//visual.Transform = new ScaleTransform(width / c.ActualWidth, height / c.ActualHeight);
			//renderBitmap.Render(visual);

			var stream = new MemoryStream();
			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
			encoder.Save(stream);
			return stream;
		}

		protected abstract FrameworkElement CreateControl(string html);
	}
	class PlainTextWpfRenderer : WpfImageRenderer {
		protected override FrameworkElement CreateControl(string html) {
			return new Label { Content = XElement.Parse("<div>" + html + "</div>").Value, Background = Brushes.White, Width = 300 };
		}
	}
	//I started writing an HTML to XAML converter before
	//finding the MS sample.  It's barely started and it
	//should be ignored.
#if Incomplete_Code	
	class ComplexWpfRenderer : WpfImageRenderer {
		static readonly HashSet<string> blockElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
			"p", "div"
		};
		static readonly Dictionary<string, Func<XElement, TextElement>> elementCreators = new Dictionary<string, Func<XElement, TextElement>>(StringComparer.OrdinalIgnoreCase) {
			{ "br", br => new LineBreak() },
			{ "p", br => new Paragraph() },
			{ "div", br => new Paragraph() },
		};
		static readonly Dictionary<string, Action<XElement, TextElement>> elementStylers = new Dictionary<string, Action<XElement, TextElement>>(StringComparer.OrdinalIgnoreCase) {
			{ "b", (b, run) => run.FontWeight = FontWeights.Bold },
			{ "i", (i, run) => run.FontStyle = FontStyles.Italic }
		};

		static TextElement CreateElement(XElement html) {
			TextElement element;
			Func<XElement, TextElement> preset;

			if (elementCreators.TryGetValue(html.Name.LocalName, out preset))
				element = preset(html);
			else
				element = new Run();

			Action<XElement, TextElement> style;

			if (elementStylers.TryGetValue(html.Name.LocalName, out style))
				style(html, element);

			//TODO: style attribute

			foreach (var node in html.Nodes()) {
				var text = node as XText;
				if(text!=null)
					element.
			}

			return element;
		}

		protected override Control CreateControl(XElement html) {

			var run = new Run();
		}
	}
#endif

	public class MsSampleImageRenderer : WpfImageRenderer {
		//This uses a Microsoft sample to convert HTML 
		//to a FlowDocument.
		//The sample doesn't support external resources
		//(images, CSS files, etc...), so there are no 
		//security issues.  (except DDOS by complexity)

		protected override FrameworkElement CreateControl(string html) {
			var xaml = HtmlToXamlConverter.ConvertHtmlToXaml(html, asFlowDocument: true);

			var flowDoc = (FlowDocument)XamlReader.Load(XmlReader.Create(new StringReader(xaml)));

			flowDoc.PagePadding = new Thickness();
			flowDoc.Background = Brushes.White;

			return new FlowDocumentScrollViewer {
				Width = 300,

				HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
				VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,

				Document = flowDoc,
			};
		}
	}
}