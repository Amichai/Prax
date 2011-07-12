using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using SLaks.Progression;

namespace Prax.OcrEngine.Services.Stubs {
	///<summary>An IDocumentRecognizer implementation that doesn't do anything.</summary>
	public class UselessRecognizer : IDocumentRecognizer {
		[ThreadStatic]
		static Random rand;

		///<summary>Does nothing for a while.</summary>

		public IEnumerable<RecognizedSegment> Recognize(Stream document, IProgressReporter progress) {
			if (rand == null)
				rand = new Random(Thread.CurrentThread.ManagedThreadId ^ Environment.TickCount);

			progress.Maximum = rand.Next(5, 9);
			for (int i = 0; i < progress.Maximum; i++) {
				if (progress.WasCanceled) break;

				Thread.Sleep(TimeSpan.FromSeconds(rand.Next(1, 3)));
				progress.Progress = i + 1;
			}

			yield break;
		}
	}
}
