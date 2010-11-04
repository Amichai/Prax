using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Prax.OcrEngine.Services;

namespace Prax.OcrEngine.RecognitionClient {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	partial class MainWindow : Window {
		static IDocumentProcessor CreateProcessor() { return new Services.Stubs.UselessProcessor(); }

		readonly ObservableCollection<DocumentModel> documents = new ObservableCollection<DocumentModel>();

		public MainWindow() {
			InitializeComponent();
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
					documents[i].CancelPending = true;
					documents.RemoveAt(i);	//If the document finishes as we speak, no harm will be done.
				}
			}
		}

		static DocumentModel StartProcessing(DocumentModel doc) {
			new Task(() => ProcessorWorker(doc)).Start();
			return doc;
		}
		static void ProcessorWorker(DocumentModel doc) {
			var processor = CreateProcessor();
			processor.ProgressChanged += (sender, e) => doc.Progress = processor.ProgressPercentage();
			processor.CheckCanceled += (sender, e) => e.Cancel = doc.CancelPending;

			doc.State = DocumentState.Scanning;
			using (var stream = File.OpenRead(doc.FilePath))
				processor.ProcessDocument(stream);

			//TODO: Results
			//foreach (var converter in resultConverters) {
			//    var stream = converter.Convert(document.OpenRead(), processor.Results);
			//    document.UploadStream(converter.OutputFormat.ToString(), stream, stream.Length);
			//}

			doc.Progress = 100;
			doc.State = DocumentState.Scanned;
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
