using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure;

namespace Prax.OcrEngine.Services.Tests {
	[TestClass]
	public class StorageClientTests {
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
		public void InMemoryStorageTest() {
			TestClient(new Stubs.InMemoryStorage());
		}
		[Ignore]	//This test can only run if the Azure service is running
		[TestMethod]
		public void AzureStorageTest() {
			TestClient(new Azure.AzureStorageClient(CloudStorageAccount.DevelopmentStorageAccount));
		}

		static void TestClient(IStorageClient client) {
			Guid user1 = Guid.NewGuid();
			Guid user2 = Guid.NewGuid();

			var doc1Id = new DocumentIdentifier(user1, client.UploadDocument(user1, "U1Doc1.txt", "I'm the first שטר!"));
			var doc = client.GetDocument(doc1Id);
			Assert.IsNotNull(doc);
			Assert.AreEqual("U1Doc1.txt", doc.Name);
			Assert.AreEqual("I'm the first שטר!", doc.DocumentText());
			Assert.IsTrue(Math.Abs((DateTime.UtcNow - doc.DateUploaded).TotalSeconds) < 3);
			Assert.AreEqual(0, doc.ScanProgress);
			Assert.AreEqual(DocumentState.ScanQueued, doc.State);

			var doc2Id = new DocumentIdentifier(user1, client.UploadDocument(user1, "User 1 שטר ב.txt", "I'm the second שטר!"));
			doc = client.GetDocument(doc2Id);
			Assert.AreEqual("User 1 שטר ב.txt", doc.Name);

			var u2doc = new DocumentIdentifier(user2, client.UploadDocument(user2, "U2Doc1.txt", ""));

			var user1Docs = client.GetDocuments(user1).ToArray();
			Assert.AreEqual(2, user1Docs.Length);
			Assert.IsTrue(user1Docs.All(d => d.Id.UserId == user1));

			client.DeleteDocument(doc2Id);
			user1Docs = client.GetDocuments(user1).ToArray();
			Assert.AreEqual(1, user1Docs.Length);

			//TODO: SetState

			client.DeleteDocument(doc1Id);
			client.DeleteDocument(u2doc);

			Assert.AreEqual(0, client.GetDocuments(user1).Count());
			Assert.AreEqual(0, client.GetDocuments(user2).Count());
		}
	}
	static class Extensions {
		static Stream GetStream(string text) { return new MemoryStream(Encoding.UTF8.GetBytes(text), false); }
		public static Guid UploadDocument(this IStorageClient client, Guid userId, string name, string text) {
			var stream = GetStream(text);
			return client.UploadDocument(userId, name, MimeTypes.ForExtension(Path.GetExtension(name)), stream, stream.Length);
		}
		public static string DocumentText(this Document document) {
			using (var reader = new StreamReader(document.OpenRead())) {
				return reader.ReadToEnd();
			}
		}
	}
}
