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

		static readonly BasicTextParagraphProperties format = new BasicTextParagraphProperties("Tahoma", 13, FlowDirection.LeftToRight);
		public void SetImage() {
			var text = String.IsNullOrWhiteSpace(textBox.Text) ? "Enter some text above" : textBox.Text;
			//preview.Source = Renderer.RenderImage(text);

			const int width = 300;

			var output = new DrawingGroup();
			foreach (var line in Measurer.MeasureLines(text, width, format, output)) {
				line.Dispose();
			}
			preview.Source = new DrawingImage(output);
		}

		private void textBox_TextChanged(object sender, TextChangedEventArgs e) {
			SetImage();
		}
	}
}
