using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine {
	partial class Config {
		partial void RegisterLocalServies() {
			LocalThreadedDocumentExecutor();
		}
	}
}
