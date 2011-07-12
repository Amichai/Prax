using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Prax.OcrEngine.Services;
using System.Collections.ObjectModel;
using SLaks.Progression;
using SLaks.Progression.Display;

namespace Prax.OcrEngine.RecognitionClient {
	///<summary>The document model class bound to the ListView in the UI.</summary>
	class DocumentModel : IProgressReporter, INotifyPropertyChanged {
		readonly SynchronizationContext syncContext;
		DocumentState state;
		volatile bool cancelPending;

		public DocumentModel(string path) {
			if (path == null) throw new ArgumentNullException("path");
			FilePath = path;
			Size = new FileInfo(path).Length;
			syncContext = SynchronizationContext.Current;
		}

		///<summary>Gets the results of the OCR.</summary>
		public ReadOnlyCollection<RecognizedSegment> Results { get; set; }

		///<summary>Gets or sets whether the scan operation should be cancelled.</summary>
		public bool WasCanceled {	//Volatile backing field
			get { return cancelPending; }
			set { cancelPending = value; }
		}

		public long Size { get; private set; }
		public string FilePath { get; private set; }

		///<summary>Gets or sets the state of the document.</summary>
		public DocumentState State {
			get { return state; }
			set { state = value; OnPropertyChanged("State"); }
		}

		long? progress = 0;
		long maximum = 100;

		///<summary>Gets or sets the progress value at which the operation will be completed.</summary>
		///<remarks>Setting this property will reset Progress to 0.</remarks>
		public virtual long Maximum {
			get { return maximum; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException("value");

				maximum = value;
				progress = 0;
				OnPropertyChanged("Maximum");
			}
		}

		///<summary>Gets or sets the current progress, between 0 and Maximum, or null to display a marquee.</summary>
		public virtual long? Progress {
			get { return progress; }
			set {
				if (value < 0 || value > Maximum)
					throw new ArgumentOutOfRangeException("value", "Progress must be between 0 and " + Maximum);
				progress = value;
				OnPropertyChanged("Progress");
			}
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
			//Try to raise the event on the UI thread
			var handler = PropertyChanged;
			if (handler == null) return;

			//If we have a UI thread and we're not on it, get to it.
			if (SynchronizationContext.Current != syncContext && syncContext != null)
				syncContext.Post(delegate { handler(this, e); }, null);
			else
				handler(this, e);
		}


		public bool AllowCancellation { get { return true; } set { } }
		string IProgressReporter.Caption { get; set; }
	}
}
