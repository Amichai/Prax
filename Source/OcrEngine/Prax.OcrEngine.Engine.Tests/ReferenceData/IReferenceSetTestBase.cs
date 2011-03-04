using Prax.OcrEngine.Engine.ReferenceData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Engine.Tests.ReferenceData {
	[TestClass]
	public abstract class IReferenceSetTestBase {
		public TestContext TestContext { get; set; }

		protected IReferenceSet ReferenceSet { get; set; }

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion

		[TestMethod]
		public void GetAllItemsTest() {
			foreach (var group in ReferenceSet.GetAllItems()) {
				Assert.IsTrue(group.All(i => i.Label == group.Key));

			}
		}
		[TestMethod]
		public void HeuristicCountTest() {
			Assert.IsTrue(
				ReferenceSet.GetAllItems()
							.SelectMany(g => g)
							.All(i => i.Data.Count == ReferenceSet.HeuristicCount)
			);
		}
		[TestMethod]
		public void LabelsTest() {
			CollectionAssert.AreEqual(
				ReferenceSet.GetAllItems().Select(g => g.Key).OrderBy(s => s).ToList(),
				ReferenceSet.Labels.OrderBy(s => s).ToList()
			);
		}
	}
}
