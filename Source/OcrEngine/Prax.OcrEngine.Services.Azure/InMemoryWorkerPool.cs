using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Prax.OcrEngine.Services.Azure {
	public class InMemoryWorkerPool {
		readonly Func<AzureScanWorker> workerCreator;
		public InMemoryWorkerPool(Func<AzureScanWorker> workerCreator) {
			this.workerCreator = workerCreator;
		}

		public void StartPool() {
			for (int i = 0; i < 4; i++) {
				Thread.Sleep(1000);	//Prevent lock convoy in Autofac
				new Thread(RunWorker).Start();
			}
		}
		void RunWorker() {
			var worker = workerCreator();
			worker.RunWorker();
		}
	}
}
