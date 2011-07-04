using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace Prax.OcrEngine.Services {
	///<summary>A base class for IDocumentProcessor implementations that handles events.</summary>
	public abstract class DocumentProcessorBase : IDocumentProcessor {
		public abstract void ProcessDocument(Stream document);

		int currentProgress;
		public int CurrentProgress {
			get { return currentProgress; }
			set {
				if (currentProgress < 0 || currentProgress > MaximumProgress)
					throw new ArgumentOutOfRangeException("value");
				currentProgress = value;
				OnProgressChanged();
			}
		}
		public int MaximumProgress { get; protected set; }

		///<summary>Occurs when the progress changes.</summary>
		public event EventHandler ProgressChanged;
		///<summary>Raises the ProgressChanged event.</summary>
		internal protected virtual void OnProgressChanged() { OnProgressChanged(EventArgs.Empty); }
		///<summary>Raises the ProgressChanged event.</summary>
		///<param name="e">An EventArgs object that provides the event data.</param>
		internal protected virtual void OnProgressChanged(EventArgs e) {
			if (ProgressChanged != null)
				ProgressChanged(this, e);
		}

		///<summary>Checks whether the operation has been canceled by the client.</summary>
		protected bool CheckCancel() {
			var args = new CancelEventArgs();
			OnCheckCanceled(args);
			return args.Cancel;
		}
		///<summary>Occurs when the processor checks whether the operation has been canceled.</summary>
		public event CancelEventHandler CheckCanceled;
		///<summary>Raises the CheckCanceled event.</summary>
		///<param name="e">A CancelEventArgs object that provides the event data.</param>
		internal protected virtual void OnCheckCanceled(CancelEventArgs e) {
			if (CheckCanceled != null)
				CheckCanceled(this, e);
		}

		public ReadOnlyCollection<RecognizedSegment> Results { get; protected set; }
	}
}
