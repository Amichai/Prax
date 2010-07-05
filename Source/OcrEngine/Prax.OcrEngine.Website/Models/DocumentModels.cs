using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prax.OcrEngine.Website.Models {
	public class DocumentListModel {
		public DocumentListModel(bool showSiteIntro, IEnumerable<Document> documents) {
			ShowSiteIntro = showSiteIntro;
		}

		///<summary>Indicates whether to display a short description of the website's functionality.</summary>
		public bool ShowSiteIntro { get; private set; }
	}
}