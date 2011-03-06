using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prax.OcrEngine.Engine.PatternRecognition;

namespace Prax.OcrEngine.Engine.Tests.PatternRecognition {
	/// <summary>
	/// Summary description for LegacyTester
	/// </summary>
	[TestClass]
	public class LegacyTester {

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void CompareRecognizers() {
			LegacyDataLibrary library;
			using (var stream = typeof(LegacyTester).Assembly.GetManifestResourceStream(typeof(LegacyTester), "LegacyTrainingData.dat"))
				library = LegacyDataLibrary.FromStream(stream);

			var rand = new Random();
			var input = new int[library.ReferenceSet.HeuristicCount];
			for (int i = 0; i < input.Length; i++)
				input[i] = rand.Next();			//There is no reason to use secure random numbers here.

			var newResult = new PatternRecognizer().Recognize(library.ReferenceSet, input);
			var oldResult = new LegacyRecognizer().Recognize(library, input);

			if (oldResult == null)
				Assert.IsNull(newResult);
			else {
				Assert.AreEqual(oldResult.Item2, newResult.Certainty);
				Assert.AreEqual(oldResult.Item1, newResult.Label);
			}
		}
	}
}
