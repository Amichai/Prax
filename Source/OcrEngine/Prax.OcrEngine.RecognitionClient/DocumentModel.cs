using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using Prax.OcrEngine.Services;

namespace Prax.OcrEngine.RecognitionClient {
	///<summary>The document model class bound to the ListView in the UI.</summary>
	class DocumentModel : INotifyPropertyChanged {
		int progress;

		public DocumentModel(string path) {
			if (path == null) throw new ArgumentNullException("path");
			FilePath = path;
			Size = new FileInfo(path).Length;
		}

		public long Size { get; private set; }
		public string FilePath { get; private set; }

		///<summary>Gets or sets the scan progress.</summary>
		public int Progress {
			get { return progress; }
			set { progress = value; OnPropertyChanged("Progress"); }
		}

		DocumentState state;
		///<summary>Gets or sets the state of the document.</summary>
		public DocumentState State {
			get { return state; }
			set { state = value; OnPropertyChanged("State"); }
		}


		//Databinding properties
		public string FileName { get { return Path.GetFileName(FilePath); } }
		public string SizeString { get { return Size.ToSizeString(); } }

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
