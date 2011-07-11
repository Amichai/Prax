using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Services.Stubs {
	///<summary>An IDataCache implementation that points to a static folder and does not update at all.</summary>
	public class StaticDataCache : IDataCache {
		public StaticDataCache(string path) { LocalPath = path; }

		public Version LocalVersion { get { return new Version(0, 0); } }

		public string LocalPath { get; private set; }

		public bool Update() { return false; }
	}
}
