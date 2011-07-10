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

namespace Prax.OcrEngine.Engine.DevUI {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		private void TrainButton_Click(object sender, RoutedEventArgs e) {
			new TrainUI() { Owner = this }.Show();
		}
		private void RecognizeButton_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show("I haven't written that yet!");
		}
	}
}
