using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows.Media;
using TextRenderer;
using System.IO;
using System.Windows;
using Prax.OcrEngine.Engine.ImageUtilities;
using System.Xml.Linq;
using System.Windows.Media.Imaging;

namespace SimpleWebDemo {
	class MeasuredTextRenderer : IImageCreator {
		readonly string font;
		public MeasuredTextRenderer(string font) { this.font = font; }

		public string ContentType { get { return "image/png"; } }

		const int Width = 300;

		public Stream CreateImage(string html) {
			if (String.IsNullOrWhiteSpace(html))
				return new RenderTargetBitmap(Width, 1, 0, 0, PixelFormats.Pbgra32).CreateStream();

			string text = HttpUtility.HtmlDecode(html);

			var output = new DrawingGroup();
			var format = new BasicTextParagraphProperties(font, 14, FlowDirection.RightToLeft);
			var words = Measurer.MeasureLines(text, Width, format, output).ToList();

			var dv = new DrawingVisual();
			using (var c = dv.RenderOpen())
				c.DrawImage(new DrawingImage(output), new Rect(new Point(Width - output.Bounds.Width, 0), output.Bounds.Size));
			var rtb = new RenderTargetBitmap(Width, (int)output.Bounds.Height, 96, 96, PixelFormats.Pbgra32);
			rtb.Render(dv);

			return rtb.CreateStream();
		}
	}
}
