using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

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
				Thread.Sleep(TimeSpan.FromSeconds(rand.Next(30, 90)));
				CurrentProgress = i + 1;
				OnProgressChanged();
			}

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
	}
}
