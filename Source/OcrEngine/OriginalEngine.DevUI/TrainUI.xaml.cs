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
using System.Windows.Shapes;
using Prax.OcrEngine.Engine.AutomatedTraining;
using TextRenderer;
using Prax.OcrEngine.Engine.HeuristicGeneration;
using Prax.OcrEngine.Engine.ImageUtilities;

namespace Prax.OcrEngine.Engine.DevUI {
	/// <summary>
	/// Interaction logic for TrainUI.xaml
	/// </summary>
	public partial class TrainUI : Window {
		public TrainUI() {
			InitializeComponent();
		}

		public HeuristicSet SelectedItem {
			get { return (HeuristicSet)GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		// Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register("SelectedItem", typeof(HeuristicSet), typeof(TrainUI), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
	}
}
