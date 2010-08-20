/// <reference path="jQuery/jquery-1.4.1-vsdoc.js" />
/// <reference path="ProgressBar.js" />
/// <reference path="Utilities.js" />

if (!Prax)
	var Prax = {};

Prax.DocumentRow = function DocumentRow(owner, tr) {
	/// <summary>Manages a row in the Documents table.</summary>
	/// <param name="tr" type="jQuery">The table row.</param>
	/// <param name="owner" type="Prax.DocumentTable">The DocumentTable object that owns this row.</param>
	if (!tr.length) throw Error("No row");

	this.tr = tr;
	this.owner = owner;

	this.id = tr.attr('id').substr(Prax.DocumentRow.idPrefix.length);
	this.name = $.trim(this.tr.children('.NameCell').text());
	this.date = new Date(this.tr.children('.DateCell').text());
	this.size = parseInt(this.tr.children('.SizeCell').attr('title'), 10);

	this.statusCell = this.tr.children('.StatusCell');
};
Prax.DocumentRow.idPrefix = 'document-';
Prax.DocumentRow.prototype = {
	tr: $(),
	statusCell: $(),
	owner: null,
	id: '',
	name: '',
	date: new Date(),
	size: -1,

	getViewPath: function () { return '/Documents/View/' + this.id + '/' + this.name; },
	setNameLink: function (on) {
		if (on || arguments.length === 0) {
			this.tr.children('.NameCell').empty().append(
				$('<a />', { text: this.name, href: this.getViewPath(), target: 'DocumentPreview' })
			);
		} else
			this.tr.children('.NameCell').text(this.name);
	},

	setId: function setId(newId) {
		delete this.owner.documents[this.id];
		this.id = newId;
		this.tr.attr('id', Prax.DocumentRow.idPrefix + newId);
		this.owner.documents[newId] = this;

		this.tr.find('.NameCell a').attr('href', this.getViewPath());
		this.tr.find('.DeleteCell input').val('Delete ' + newId);
	},
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
	}
};
Prax.DocumentTable = function DocumentTable(table) {
	/// <summary>Manages a documents table.</summary>
	/// <param name="table" type="jQuery">The table element.</param>
	this.table = table;

	var self = this;
	this.documents = $.map(table.find('tbody tr'), function (tr) { return new Prax.DocumentRow(self, $(tr)); });

	for (var i = 0; i < this.documents.length; i++)
		this.documents[this.documents[i].id] = this.documents[i];
}
Prax.DocumentTable.prototype = {
	table: $(),
	updateTimer: false,
	documents: [],

	createRow: function createRow(id, name, size, date) {
		/// <summary>Creates a new document row.  Does not interact with the server.</summary>
		/// <param name="id" type="String">The ID of the document.</param>
		/// <param name="name" type="String">The filename.</param>
		/// <param name="size" type="Number">The size of the file in bytes.</param>
		/// <returns type="Prax.DocumentRow" />

		var tr = $('<tr />', { id: Prax.DocumentRow.idPrefix + id })
					.append($('<td />', { 'class': "NameCell", text: name }))
					.append($('<td />', { 'class': "SizeCell Right", text: Prax.toSizeString(size), title: size + ' bytes' }))
					.append($('<td />', { 'class': "DateCell Right", text: (date || new Date()).toShortDateString() }))
					.append($('<td class="StatusCell Center">Wait...</td>'))
					.append($('<td class="DeleteCell" />').append(
						$('<input />', { type: "submit", name: "id", title: "Delete " + name, value: "Delete " + id })
					));

		//The submit button passes the ID parameter to the form.
		//Since the value attribute also controls the caption, I
		//hide the caption using CSS.  For accessibility reasons,
		//I include the word delete, which the action removes.--%>

		tr.prependTo(this.table.children('tbody'));

		var retVal = new Prax.DocumentRow(this, tr);
		this.documents.splice(0, 0, retVal);
		this.documents[id] = retVal;
		return retVal;
	},
	getRow: function getRow(id) {
		/// <summary>Gets the row for the document with the given ID.</summary>
		/// <param name="id" type="String">The document's GUID.</param>
		/// <returns type="Prax.DocumentRow" />
		return this.documents[id] || null;
	},
	removeRow: function (row) {
		/// <summary>Removes a document row from the table.  Does not interact with the server.</summary>
		/// <param name="row" type="Prax.DocumentRow">The DocumentRow instance to reomve.</param>
		row.tr.remove();
		this.documents.splice(this.documents.indexOf(row), 1);
		delete this.documents[row.id];
	},
	updateData: function updateData(noTimer) {
		var self = this;

		$.getJSON("/Documents/Data?Timestamp=" + new Date(), function (data) {
			var existingIds = {}; //Holds all of the IDs received from the server.

			for (var i = 0; i < data.documents.length; i++) {
				var doc = data.documents[i];
				existingIds[doc.id] = true;
				var docRow = self.getRow(doc.id);

				//If this document doesn't exist in
				//the table, create it.  (eg, if it
				//was created in another instance)
				if (!docRow) {
					var date = new Date(parseInt(doc.date.substr('/Date('.length), 10)); //.Net sends dates through JSON as "\/Date(1282162655583)\/"
					docRow = self.createRow(doc.id, doc.name, doc.size, date);
					docRow.setNameLink(true);
				}

				if (doc.state === 'Scanned')
					docRow.statusCell.text('Scanned');
				else
					docRow.setProgress(doc.progressCaption, doc.progress);
			}

			//Remove any documents that are no 
			//longer on the server.  (eg, they
			//were deleted in another browser)
			for (i = self.documents.length - 1; i >= 0; i--) {
				doc = self.documents[i];
				if (!existingIds[doc.id])
					self.removeRow(doc);
			}

			if (data.refreshTimeout > 0 && !noTimer) {
				var timer = self.updateTimer = setTimeout(function () {
					//If another call set a later timer, wait for its callback to set another timer.
					//Otherwise, each manual call to updateData() will start a new timer.

					self.updateData(timer !== self.updateTimer);
				}, data.refreshTimeout);
			}
		});
	}
};