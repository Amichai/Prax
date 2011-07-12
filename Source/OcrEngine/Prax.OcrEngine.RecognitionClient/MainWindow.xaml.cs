using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Prax.OcrEngine.Services;

namespace Prax.OcrEngine.RecognitionClient {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	partial class MainWindow : Window {
		readonly IDocumentRecognizer recognizer;
		readonly ObservableCollection<DocumentModel> documents = new ObservableCollection<DocumentModel>();

		public MainWindow(IDocumentRecognizer recognizer) {
			InitializeComponent();
			this.recognizer = recognizer;
			filesList.ItemsSource = documents;
		}

		private void AddImage_Click(object sender, RoutedEventArgs e) {
			var dialog = new OpenFileDialog {
				Filter = "Image files|*.jpg;*.jpeg;*.jpe;*.png;*.bmp;*.tif;*.tiff|All files|*",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
				Multiselect = true,
				Title = "Open image"
			};
			if (!dialog.ShowDialog(this) ?? false) return;
			documents.AddRange(dialog.FileNames.Select(p => StartProcessing(new DocumentModel(p))));
		}

		private void CancelAll_Click(object sender, RoutedEventArgs e) {
			for (int i = documents.Count - 1; i >= 0; i--) {
				if (documents[i].State != DocumentState.Scanned) {
					documents[i].WasCanceled = true;
					documents.RemoveAt(i);	//If the document finishes as we speak, no harm will be done.
				}
			}
		}

		static readonly LimitedConcurrencyLevelTaskScheduler scheduler = new LimitedConcurrencyLevelTaskScheduler(Environment.ProcessorCount);
		DocumentModel StartProcessing(DocumentModel doc) {
			new Task(() => ProcessorWorker(doc)).Start(scheduler);
			return doc;
		}
		void ProcessorWorker(DocumentModel doc) {
			if (doc.WasCanceled) return;	//In case the document was cancelled while queued

			doc.State = DocumentState.Scanning;
			using (var stream = File.OpenRead(doc.FilePath))
				doc.Results = new ReadOnlyCollection<RecognizedSegment>(recognizer.Recognize(stream, doc).ToList());

			doc.Progress = doc.Maximum;
			doc.State = DocumentState.Scanned;
		}

		static T FindAncestor<T>(DependencyObject child) where T : DependencyObject {
			var elem = VisualTreeHelper.GetParent(child);
			T retVal;
			while (null == (retVal = elem as T) && elem != null)
				elem = VisualTreeHelper.GetParent(elem);

			return retVal;
		}

		private void ResultLink_Click(object sender, RoutedEventArgs e) {
			var button = (Button)sender;
			var converter = (IResultsConverter)button.Tag;	//The Tag is databound to a property on an anonymous type

			var item = FindAncestor<ListViewItem>(button);
			var doc = (DocumentModel)item.Content;

			var fileName = Path.GetTempFileName();
			File.Delete(fileName);
			fileName = Path.ChangeExtension(fileName, converter.OutputFormat.GetExtension());

			using (var originalFile = File.OpenRead(doc.FilePath))
			using (var source = converter.Convert(originalFile, doc.Results))
			using (var outputFile = File.Create(fileName)) {
				source.CopyTo(outputFile);
			}
			Process.Start(fileName);
		}

		private void FilesList_KeyUp(object sender, KeyEventArgs e) {
			switch (e.Key) {
				case Key.Delete:
					var models = filesList.SelectedItems.Cast<DocumentModel>().ToList();
					foreach (var d in models) {
						d.WasCanceled = true;
						documents.Remove(d);
					}
					break;
			}
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
		static IEnumerable<DocumentModel> CreateDummyList() {
			return Directory.EnumerateFiles(ImagePath, "*.jpg", SearchOption.AllDirectories)
							.Select(p => new DocumentModel(p) {
								Progress = rand.Next(0, 100),
								State = eligibleStates[rand.Next(eligibleStates.Length)]
							});
		}

		public DummyDocumentList()
			: base(LicenseManager.UsageMode == LicenseUsageMode.Runtime ? Enumerable.Empty<DocumentModel>() : CreateDummyList()) { }
	}
}
