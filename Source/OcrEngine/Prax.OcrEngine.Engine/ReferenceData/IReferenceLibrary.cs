using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Engine.ReferenceData {
	///<summary>A library of reference data.</summary>
	///<remarks>
	/// Libraries are the master collections of training data
	/// which are stored on disk or in SQL Server.
	/// 
	/// This interface violates Liskov - IReferenceSet is immutable
	/// 
	/// I recommend that we replace this a separate
	/// IReferenceUpdater interface which can return
	/// an IReferenceSet.  
	/// IReferenceUpdater would only be used by the 
	/// training system; ordinary PR invocations 
	/// would use master ReferenceSets, which may
	/// be returned by the same implementation.
	/// For example, a SqlReferenceLibrary class
	/// which implements IReferenceUpdater and
	/// has a MasterSet property.
	///</remarks>
	public interface IReferenceLibrary : IReferenceSet {
		void AppendData(IEnumerable<ReferenceItem> items);
		void RemoveData(IEnumerable<ReferenceItem> items);
		void Clear();
	}
}
