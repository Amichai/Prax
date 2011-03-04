using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prax.OcrEngine.Engine.ReferenceData;

namespace Prax.OcrEngine.Engine.Tests.ReferenceData {
	[TestClass]
	public class InMemoryReferenceSetTest : IReferenceSetTestBase {

		public InMemoryReferenceSetTest() {
			var rand = new Random();
			var baseArray = new byte[rand.Next(100, 2000)];

			base.ReferenceSet = new InMemoryReferenceSet(
				Enumerable.Repeat(0, rand.Next(5000, 10000))
						  .Select(__ => new ReferenceItem(
							  new String(Array.ConvertAll(new byte[rand.Next(2, 4)], _ => (char)('A' + rand.Next(7, 8)))),
							  Array.ConvertAll(baseArray, _ => rand.Next(int.MinValue, int.MaxValue))
						  )
				)
			);
		}
	}
}
