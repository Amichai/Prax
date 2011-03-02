using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections.ObjectModel;

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

		///<summary>Gets the number of heuristic elements in the items in this set (the size of the int[]s).</summary>
		int HeuristicCount { get; }

		///<summary>Gets the unique labels in this set.</summary>
		///<remarks>This property must be fast; it will be called a large number of times.</remarks>
		ReadOnlyCollection<string> Labels { get; }

		///<summary>Gets all of the items that correspond to a given label.</summary>
		///<returns>A sequence of labels, possibly streamed directly from SQL Server.</returns>
		///<remarks>This method is expected to be slow.</remarks>
		IEnumerable<ReferenceItem> GetItems(string label);
	}
}
