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
using Prax.OcrEngine.Engine.HeuristicGeneration;

namespace Prax.OcrEngine.Engine.DevUI.Controls {
	/// <summary>
	/// Interaction logic for HeuristicDisplay.xaml
	/// </summary>
	public partial class HeuristicDisplay : UserControl {
		public HeuristicDisplay() {
			InitializeComponent();
		}


		public HeuristicSet Source {
			get { return (HeuristicSet)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register("Source", typeof(HeuristicSet), typeof(HeuristicDisplay), new UIPropertyMetadata(null, (s, e) => ((HeuristicDisplay)s).OnSourceChanged()));



		public IEnumerable<ChartPoint> Points {
			get { return (IEnumerable<ChartPoint>)GetValue(PointsProperty); }
			set { SetValue(PointsProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Points.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PointsProperty =
			DependencyProperty.Register("Points", typeof(IEnumerable<ChartPoint>), typeof(HeuristicDisplay), new UIPropertyMetadata(null));



		void OnSourceChanged() {
			if (Source != null)
				Points = Source.Heuristics.Select((v, i) => new ChartPoint(i, v)).ToList();
		}
	}
	public class ChartPoint {
		public ChartPoint(int x, int y) { X = x; Y = y; }

		public int X { get; private set; }
		public int Y { get; private set; }
	}
}
