using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Services.Stubs {
	///<summary>An IDocumentProcessor implementation that doesn't do anything.</summary>
	public class UselessProcessor : IDocumentProcessor {
		[ThreadStatic]
		static Random rand;
		///<summary>Does nothing for a while.</summary>
		public void ProcessDocument(Stream document) {
			IsProcessing = true;
			if (rand == null)
				rand = new Random(Thread.CurrentThread.ManagedThreadId ^ Environment.TickCount);

			MaximumProgress = rand.Next(5, 15);
			for (int i = 0; i < MaximumProgress; i++) {
				if (CheckCancel()) break;

				Thread.Sleep(TimeSpan.FromSeconds(rand.Next(5, 35)));
				CurrentProgress = i + 1;
				OnProgressChanged();
			}
			Results = new ReadOnlyCollection<RecognizedSegment>(new RecognizedSegment[0]);
			IsProcessing = false;
		}
		///<summary>Gets the current progress of the operation.</summary>
		public int CurrentProgress { get; private set; }
		///<summary>Gets the maximum progress of the operation.</summary>
		///<remarks>If CurrentProgress is equal to MaximumProgress, the operation has finished.</remarks>
		public int MaximumProgress { get; private set; }
		///<summary>Indicates whether this instance is currently processing a document.</summary>
		public bool IsProcessing { get; private set; }

		///<summary>Occurs when the progress of the operation changes.</summary>
		public event EventHandler ProgressChanged;
		///<summary>Raises the ProgressChanged event.</summary>
		internal protected virtual void OnProgressChanged() { OnProgressChanged(EventArgs.Empty); }
		///<summary>Raises the ProgressChanged event.</summary>
		///<param name="e">An EventArgs object that provides the event data.</param>
		internal protected virtual void OnProgressChanged(EventArgs e) {
			if (ProgressChanged != null)
				ProgressChanged(this, e);
		}

		bool CheckCancel() {
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

		public void Initialize() { }
		public ReadOnlyCollection<RecognizedSegment> Results { get; private set; }
	}
}
