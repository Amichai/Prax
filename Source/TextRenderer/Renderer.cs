using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace TextRenderer {
	public static class Renderer {
		public static FileStream CreateStream(this BitmapSource image, string filename) {
			var stream = new FileStream(filename, FileMode.Create);
			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(image));
			encoder.Save(stream);
			stream.Position = 0;
			return stream;
		}

		public static BitmapSource RenderImage(string text) {
			var c = CreateFlowDoc(text);
			//var c = CreateTextBlock(text);

			c.Measure(new Size(Double.IsNaN(c.Width) ? 9999 : c.Width, Double.IsNaN(c.Height) ? 9999 : c.Height));
			c.Arrange(new Rect(c.DesiredSize));

			var  renderBitmap = new RenderTargetBitmap((int)c.ActualWidth,
													   (int)c.ActualHeight, 96, 96, PixelFormats.Pbgra32);

			renderBitmap.Render(c);
			return renderBitmap;
		}

		static FrameworkElement CreateTextBlock(string text) {
			return new TextBlock(new Run(text));
		}

		static FrameworkElement CreateFlowDoc(string text) {
			var flowDoc = new FlowDocument();
			flowDoc.Blocks.Add(new Paragraph(new Run(text)));

			flowDoc.FlowDirection = FlowDirection.RightToLeft;

			flowDoc.PagePadding = new Thickness();	//Suppress the default padding
			//flowDoc.Background = Brushes.White;

			return new FlowDocumentScrollViewer {
				Width = 300,

				HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
				VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,

				Document = flowDoc,
			};
		}

	}
}
