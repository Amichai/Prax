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
using System.Collections.ObjectModel;
using System.IO;
using Path = System.IO.Path;
using Prax.OcrEngine.Services;

namespace Prax.OcrEngine.RecognitionClient {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		private void AddImage_Click(object sender, RoutedEventArgs e) {

		}

		private void CancelAll_Click(object sender, RoutedEventArgs e) {

		}
	}
	///<summary>Used to provide a list of documents for the XAML designer.</summary>
	class DummyDocumentList : ObservableCollection<DocumentModel> {
		static string ImagePath {
			get {
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Web\Wallpaper");
			}
		}

		static readonly DocumentState[] eligibleStates = { DocumentState.ScanQueued, DocumentState.Scanning, DocumentState.Scanned };
		static readonly Random rand = new Random();
		public DummyDocumentList()
			: base(
				Directory.EnumerateFiles(ImagePath, "*.jpg", SearchOption.AllDirectories)
				.Select(p => new DocumentModel(p) {
					Progress = rand.Next(0, 100),
					State = eligibleStates[rand.Next(eligibleStates.Length)]
				})) { }
	}
}
