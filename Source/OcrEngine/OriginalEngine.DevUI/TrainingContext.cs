using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Prax.OcrEngine.Engine.AutomatedTraining;
using Prax.OcrEngine.Engine.HeuristicGeneration;
using Prax.OcrEngine.Engine.ImageUtilities;
using TextRenderer;
using System.ComponentModel;

namespace Prax.OcrEngine.Engine.DevUI {
	public class TrainingContext : INotifyPropertyChanged {

		BitmapSource image;
		///<summary>Gets or sets the rendered image containing the text.</summary>
		public BitmapSource Image {
			get { return image; }
			private set { image = value; OnPropertyChanged("Image"); }
		}
		IEnumerable<HeuristicSet> segments;
		///<summary>Gets or sets the segments containing individual characters.</summary>
		public IEnumerable<HeuristicSet> Segments {
			get { return segments; }
			private set { segments = value; OnPropertyChanged("Segments"); }
		}
		string text;
		///<summary>Gets or sets the text to render.</summary>
		public string Text {
			get { return text; }
			set { text = value; OnPropertyChanged("Text"); Update(); }
		}

		public void Update() {
			var output = new DrawingGroup();
			var format = new BasicTextParagraphProperties("Times New Roman", 34, FlowDirection.LeftToRight);
			var words = BoundedWord.GetWords(Text, Measurer.MeasureLines(Text, 200, format, output)).ToList();
			Image = output.ToBitmap();

			ImageData imageData;
			using (var stream = Image.CreateStream())
				imageData = new ImageData(stream);

			var boards = imageData.DefineIteratedBoards();
			var chars = words.SelectMany(w => w.Characters);
			Segments = chars.Select(boards.GetLetterHeuristics).ToList();
		}

		///<summary>Occurs when a property value is changed.</summary>
		public event PropertyChangedEventHandler PropertyChanged;
		///<summary>Raises the PropertyChanged event.</summary>
		///<param name="name">The name of the property that changed.</param>
		protected virtual void OnPropertyChanged(string name) { OnPropertyChanged(new PropertyChangedEventArgs(name)); }
		///<summary>Raises the PropertyChanged event.</summary>
		///<param name="e">An EventArgs object that provides the event data.</param>
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
			if (PropertyChanged != null)
				PropertyChanged(this, e);
		}
	}
}
