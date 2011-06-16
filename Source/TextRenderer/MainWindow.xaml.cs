using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextRenderer {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
			SetImage();
		}

		///<summary>Describes a single string recognized in an image.</summary>
		public struct TextSegment {
			///<summary>Creates a RecognizedSegment value.</summary>
			public TextSegment(Rect bounds, string text)
				: this() {
				Bounds = bounds;
				Text = text;
			}

			///<summary>Gets the area in the image that contains the string.</summary>
			public Rect Bounds { get; private set; }
			///<summary>Gets the recognized text.</summary>
			public string Text { get; private set; }
		}

		static readonly char[] whitespaceChars = " \t\r\n".ToCharArray();	//TODO: More chars

		static readonly BasicTextParagraphProperties format = new BasicTextParagraphProperties("Tahoma", 13, FlowDirection.LeftToRight);
		public void SetImage() {
			var text = String.IsNullOrWhiteSpace(textBox.Text) ? "Enter some text above" : textBox.Text;
			//preview.Source = Renderer.RenderImage(text);

			const int width = 300;

			var words = new List<TextSegment>();

			var output = new DrawingGroup();

			//TextLines always include a line terminator, even for the last line in the string.
			string fullText = text + "\n";

			int lineStart = 0;	//In characters
			double top = 0;		//In pixels
			foreach (var line in Measurer.MeasureLines(text, width, format, output)) {
				int lastSpace = lineStart;
				while (lastSpace < lineStart + line.Length - 1) {
					while (lastSpace < lineStart + line.Length && Char.IsWhiteSpace(fullText[lastSpace]))
						lastSpace++;	//Skip over the previous chunk of whitespace

					if (lastSpace == lineStart + line.Length)
						continue;		//If the line ends in whitespace, skip it entirely

					//Find the next space within this line
					int nextSpace = fullText.IndexOfAny(whitespaceChars, lastSpace + 1, line.Length - (lastSpace + 1 - lineStart));

					if (nextSpace < 0)		//Include the last word, even if it doesn't end with a space.
						nextSpace = lineStart + line.Length - 1;

					//if (nextSpace == lastSpace) continue;	//Entirely Skip double spaces

					var word = text.Substring(lastSpace, nextSpace - lastSpace);
					var bounds = line.GetTextBounds(lastSpace, word.Length);

					//bounds is relative to the line
					words.Add(new TextSegment(Rect.Offset(bounds[0].Rectangle, 0, top), word));

					lastSpace = nextSpace;
				}
				lineStart += line.Length;
				top += line.Height;
				line.Dispose();
			}

			using (var dc = output.Append()) {
				var pen = new Pen(new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)), 1);
				foreach (var word in words) {
					dc.DrawRectangle(null, pen, word.Bounds);
				}
			}

			preview.Source = new DrawingImage(output);
		}

		private void textBox_TextChanged(object sender, TextChangedEventArgs e) {
			SetImage();
		}
	}
}
