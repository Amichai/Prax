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
	public class UselessProcessor : DocumentProcessorBase {
		[ThreadStatic]
		static Random rand;

		///<summary>Does nothing for a while.</summary>
		public override void ProcessDocument(Stream document) {
			if (rand == null)
				rand = new Random(Thread.CurrentThread.ManagedThreadId ^ Environment.TickCount);

			MaximumProgress = rand.Next(5, 9);
			for (int i = 0; i < MaximumProgress; i++) {
				if (CheckCancel()) break;

				Thread.Sleep(TimeSpan.FromSeconds(rand.Next(1, 3)));
				CurrentProgress = i + 1;
				OnProgressChanged();
			}
			Results = new ReadOnlyCollection<RecognizedSegment>(new RecognizedSegment[0]);
		}
	}
}
