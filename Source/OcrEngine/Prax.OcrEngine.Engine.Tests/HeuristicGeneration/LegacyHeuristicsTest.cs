using Prax.OcrEngine.Engine.HeuristicGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Engine.Tests.HeuristicGeneration {


	/// <summary>
	///This is a test class for HeuristicGeneratorTest and is intended
	///to contain all HeuristicGeneratorTest Unit Tests
	///</summary>
	[TestClass]
	public class LegacyHeuristicsTest {
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion



		/// <summary>
		///A test for BuildData
		///</summary>
		[TestMethod()]
		public void CompareGenerators() {
			Random rand = new Random();

			int[][] data = new int[rand.Next(100, 500)][];
			int height = rand.Next(100, 500);
			for (int x = 0; x < data.Length; x++) {
				data[x] = new int[height];
				for (int y = 0; y < height; y++)
					data[x][y] = rand.Next(-65536, +65537);
			}

			var oldResult = new LegacyHeuristics().trainTestPreprocess(data);
			var newResult = new HeuristicGenerator(data).BuildData();

			CollectionAssert.AreEqual(oldResult, newResult);
		}
	}
}
