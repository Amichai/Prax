using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Services {
	///<summary>Maintains a local cache of versioned data in the filesystem.</summary>
	///<remarks>This service is used by the scan workers to cache training data.</remarks>
	public interface IDataCache {
		///<summary>Gets the version of the data currently in the cache, or null if no data has been cached.</summary>
		Version LocalVersion { get; }

		///<summary>Gets the path to the contents of the local cache.</summary>
		///<remarks>This path is typically passed as a constructor parameter.</remarks>
		string LocalPath { get; }

		///<summary>Updates the contents of the local cache if necessary.</summary>
		///<returns>True if the cache was updated; false it it was already up-to-date.</returns>
		bool Update();
	}
}
