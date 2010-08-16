using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.ObjectModel;
using Prax.OcrEngine.Services;

namespace Prax.OcrEngine.Website.Models {
	public class DocumentListModel {
		public DocumentListModel(bool showSiteIntro, IEnumerable<Document> documents) {
			ShowSiteIntro = showSiteIntro;
			Documents = new ReadOnlyCollection<Document>(documents.ToArray());
		}

		///<summary>Indicates whether to display a short description of the website's functionality.</summary>
		public bool ShowSiteIntro { get; private set; }

		///<summary>Gets all of the user's documents.</summary>
		public ReadOnlyCollection<Document> Documents { get; private set; }
	}
	public class ProgressBarModel {
		public int Progress { get; set; }
		public string Caption { get; set; }
	}

	public static class DocumentExtensions {
		public static TimeSpan? GetRefreshTime(this Document doc) {
			switch (doc.State) {
				case DocumentState.ScanQueued: return TimeSpan.FromMinutes(5);	//TODO: Intelligent guess?
				case DocumentState.Scanning: return TimeSpan.FromSeconds(5);
				case DocumentState.Scanned: return null;

				case DocumentState.Error:
				default:
					return TimeSpan.FromMinutes(1);
			}
		}
	}
}