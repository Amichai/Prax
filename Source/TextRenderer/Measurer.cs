using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.TextFormatting;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TextRenderer {
	class BasicTextRunProperties : TextRunProperties {
		public BasicTextRunProperties(string font, int size) {
			this.size = size;
			typeface = new Typeface(font);
		}

		public override Brush BackgroundBrush { get { return Brushes.Transparent; } }
		public override Brush ForegroundBrush { get { return Brushes.Black; } }

		readonly Typeface typeface;
		public override Typeface Typeface { get { return typeface; } }

		readonly int size;
		public override double FontRenderingEmSize { get { return size; } }
		public override double FontHintingEmSize { get { return FontRenderingEmSize; } }

		public override CultureInfo CultureInfo { get { return CultureInfo.InvariantCulture; } }
		public override TextDecorationCollection TextDecorations { get { return null; } }
		public override TextEffectCollection TextEffects { get { return null; } }
	}
	public class BasicTextParagraphProperties : TextParagraphProperties {
		public BasicTextParagraphProperties(string font, int size, FlowDirection direction = FlowDirection.LeftToRight) : this(new BasicTextRunProperties(font, size), direction) { }
		public BasicTextParagraphProperties(TextRunProperties runProperties, FlowDirection direction = FlowDirection.LeftToRight) {
			defaultRunProperties = runProperties;
			flowDirection = direction;
		}

		readonly FlowDirection flowDirection;
		readonly TextRunProperties defaultRunProperties;

		public override TextRunProperties DefaultTextRunProperties { get { return defaultRunProperties; } }

		public override bool FirstLineInParagraph { get { return false; } }

		public override FlowDirection FlowDirection { get { return flowDirection; } }

		public override double Indent { get { return 0; } }
		public override double LineHeight { get { return 0; } }

		public override TextAlignment TextAlignment { get { return TextAlignment.Left; } }
		public override TextWrapping TextWrapping { get { return TextWrapping.Wrap; } }

		public override TextMarkerProperties TextMarkerProperties { get { return null; } }
	}
	public static class Measurer {
		class BasicSource : TextSource {
			readonly string text;
			readonly TextRunProperties properties;

			public BasicSource(string text, TextRunProperties properties) { this.text = text; this.properties = properties; }

			public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit) {
				var cbr = new CharacterBufferRange(text, 0, textSourceCharacterIndexLimit);

				return new TextSpan<CultureSpecificCharacterBufferRange>(
					textSourceCharacterIndexLimit,
					new CultureSpecificCharacterBufferRange(CultureInfo.InvariantCulture, cbr)
				);
			}

			public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex) {
				throw new NotImplementedException();
			}

			public override TextRun GetTextRun(int textSourceCharacterIndex) {
				if (textSourceCharacterIndex >= text.Length)
					return new TextEndOfParagraph(1);

				return new TextCharacters(
					text,
					textSourceCharacterIndex, text.Length - textSourceCharacterIndex,
					properties
				);
			}
		}

		public static IEnumerable<TextLine> MeasureLines(string text, int width, TextParagraphProperties format, DrawingGroup drawTarget) {
			using (var dc = drawTarget.Open())
			using (var formatter = TextFormatter.Create()) {
				int index = 0;
				double y = 0;

				var source = new BasicSource(text, format.DefaultTextRunProperties);
				while (index < text.Length) {
					var line = formatter.FormatLine(source, index, width, format, null);
					line.Draw(dc, new Point(0, y), InvertAxes.None);
					y += line.Height;
					index += line.Length;

					yield return line;
				}
			}
		}
	}
}
