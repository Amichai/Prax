using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Prax.OcrEngine.Engine.ReferenceData {
	///<summary>A set of labeled items from the reference library.</summary>
	///<remarks>
	/// We will eventually create a SQL Server implementation of the 
	/// reference library.  This class should be kept SQL-friendly;
	/// prefer queries to full lookups.
	/// Ordinary ReferenceSets are immutable, but ReferenceLibrary 
	/// (which inherits this) is mutable.
	///</remarks>
	public interface IReferenceSet {
		///<summary>Creates a subset of this set matching the given criteria.</summary>
		///<remarks>This method is designed to allow subsets to be created using
		///SQL queries.  It will primarily be used to filter word sets by known letters.</remarks>
		IReferenceSet Subset(Expression<Func<string, bool>> query);

		///<summary>Gets all of the items in this set.  Avoid this property where possible.</summary>
		///<remarks>It is better to perform a query than to loop through this.</remarks>
		IEnumerable<ReferenceItem> Items { get; }
	}
}
