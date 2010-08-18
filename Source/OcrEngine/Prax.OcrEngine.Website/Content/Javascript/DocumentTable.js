/// <reference path="jQuery/jquery-1.4.1-vsdoc.js" />
/// <reference path="ProgressBar.js" />

if (!Prax)
	var Prax = {};

Prax.DocumentRow = function DocumentRow(tr) {
	/// <summary>Manages a row in the Documents table.</summary>
	/// <param name="tr" type="jQuery">The table row.</param>
	if (!tr.length) throw Error("No row");

	this.tr = tr;
	this.statusCell = this.tr.children('.StatusCell');
};
Prax.DocumentRow.prototype = {
	tr: $(),
	statusCell: $(),
	setProgress: function setProgress(caption, percent) {
		if (percent < 0) {
			this.statusCell.text(caption);
		} else {
			var bar = this.statusCell.children('.ProgressContainer').progressBar();
			if (bar == null)
				bar = $('<div />').appendTo(this.statusCell.empty()).progressBar();
			bar.val(percent);
			bar.text(caption);
		}
	},
	updateFrom: function updateFrom(doc) {
		if (doc.state === 'Scanned')
			this.statusCell.text('Scanned');
		else
			this.setProgress(doc.progressCaption, doc.progress);
	}
};

Prax.DocumentTable = function DocumentTable(table) {
	/// <summary>Manages a documents table.</summary>
	/// <param name="table" type="jQuery">The table element.</param>
	this.table = table;
}
Prax.DocumentTable.prototype = {
	table: $(),
	getRow: function getRow(id) {
		/// <summary>Gets the row for the document with the given ID.</summary>
		/// <param name="id" type="String">The document's GUID.</param>
		/// <returns type="Prax.DocumentRow" />
		return new Prax.DocumentRow(this.table.find('#document-' + id));
	},
	updateData: function updateData() {
		var self = this;

		$.getJSON("/Documents/Data", function (data) {
			for (var i = 0; i < data.documents.length; i++) {
				self.getRow(data.documents[i].id).updateFrom(data.documents[i]);
			}

			if (data.refreshTimeout > 0)
				setTimeout(function () { self.updateData(); }, data.refreshTimeout);
		});
	}
};