﻿/// <reference path="jQuery/jquery-1.4.1-vsdoc.js" />
/// <reference path="ProgressBar.js" />
/// <reference path="Utilities.js" />

if (!Prax)
	var Prax = {};

Prax.DocumentState = function DocumentState(name) { this.name = name; };
Prax.DocumentState.prototype = { toString: function () { return this.name; } };

Prax.DocumentState.uploading = new Prax.DocumentState("Uploading");
Prax.DocumentState.scanning = new Prax.DocumentState("Scanning");
Prax.DocumentState.complete = new Prax.DocumentState("Complete");

Prax.DocumentRow = function DocumentRow(owner, tr) {
	/// <summary>Manages a row in the Documents table.</summary>
	/// <param name="tr" type="jQuery">The table row.</param>
	/// <param name="owner" type="Prax.DocumentTable">The DocumentTable object that owns this row.</param>
	if (!tr.length) throw Error("No row");

	this.tr = tr;
	this.extension = tr.attr('extension');
	this.deleteButton = this.tr.find('.DeleteCell input');
	this.owner = owner;

	this.id = tr.attr('id').substr(Prax.DocumentRow.idPrefix.length);
	this.name = $.trim(this.tr.children('.NameCell').text());
	this.date = new Date(this.tr.children('.DateCell').text());
	this.size = parseInt(this.tr.children('.SizeCell').attr('title'), 10);

	this.statusCell = this.tr.children('.StatusCell');

	var self = this;

	this.renameButton = $('<a class="RenameLink Sprite16" href="#" title="Rename document" />')
		.click(function () { self.interactiveRename(); return false; })
		.appendTo(this.tr.children('.NameCell'));

	this.deleteButton.click(function () {
		if (!confirm("Are you sure you want to delete " + self.name + "?"))
			return;
		self.deleteDocument();
		return false;
	});
};
Prax.DocumentRow.idPrefix = 'document-';
Prax.DocumentRow.prototype = {
	tr: $(),
	statusCell: $(),
	deleteButton: $(),
	renameButton: $(),
	owner: null,
	id: '',
	name: '',
	extension: '',
	date: new Date(),
	size: -1,
	state: null,
	formats: [],

	getViewPath: function () { return '/Documents/View/' + this.id + '/' + this.name + this.extension; },
	setNameLink: function (on) {
		var newChild;

		if (on || arguments.length === 0)
			newChild = $('<a />', { text: this.name, href: this.getViewPath(), target: 'DocumentPreview' });
		else
			newChild = $('<span />').text(this.name);

		this.tr.find('.NameCell :first-child').replaceWith(newChild);
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
			if (bar === null)	//If there is no .ProgressContainer element
				bar = $('<div />').appendTo(this.statusCell.empty()).progressBar();
			bar.val(percent);
			bar.text(caption);
		}
	},
	addDownloadLinks: function addDownloadLinks() {
		this.statusCell.children('.DownloadIcon').remove();
		for (var i = 0; i < this.formats.length; i++)
			this.statusCell.append(this.createDownloadLink(this.formats[i]));
	},
	createDownloadLink: function createDownloadLink(format) {
		var shortName = format.extension.substring(1).toUpperCase();
		return $('<a />', {
			"class": "Sprite16 DownloadIcon " + shortName,
			title: "Download OCR results as a " + shortName + " file",
			target: "DocumentPreview",
			href: "/Documents/" + this.id + "/" + format.name + "/" + this.name + format.extension.toLowerCase()
		});
	},
	deleteDocument: function deleteDocument() {
		/// <summary>Deletes this document from the server.</summary>
		/// <remarks>This method is replaced by DocumentUploader for rows that are still uploading.</remarks>

		var self = this;
		this.deleteButton.attr({ disabled: true, title: "Please wait..." });

		this.deleteButton.addClass('LoadingButton');
		$.post('/Documents/Delete', { id: this.id }, function (response) {
			self.owner.removeRow(self);
		});
	},
	interactiveRename: function interactiveRename() {
		if (this.renameButton.hasClass('LoadingButton')) return;

		var newName = prompt('Please enter a new name', this.name);
		if (!newName || newName === name) return;

		this.renameButton.addClass('LoadingButton');
		var self = this;
		$.post('/Documents/Rename', { id: this.id, newName: newName }, function (response) {
			self.renameButton.removeClass('LoadingButton');
			self.name = newName;

			self.tr.find('.NameCell :first-child').text(newName);
			//TODO: Update links & tooltips
		});
	}
};
Prax.DocumentTable = function DocumentTable(table) {
	/// <summary>Manages a documents table.</summary>
	/// <param name="table" type="jQuery">The table element.</param>
	this.table = table;

	var self = this;
	this.documents = $.map(table.find('tbody tr'), function (tr) {
		var retVal = new Prax.DocumentRow(self, $(tr));
		if (retVal.statusCell.children('.ProgressContainer').length)
			retVal.state = Prax.DocumentState.scanning;
		else
			retVal.state = Prax.DocumentState.complete;
		return retVal;
	});

	for (var i = 0; i < this.documents.length; i++)
		this.documents[this.documents[i].id] = this.documents[i];
}
Prax.DocumentTable.prototype = {
	table: $(),
	updateTimer: false,
	documents: [],

	createRow: function createRow(id, name, extension, size, date) {
		/// <summary>Creates a new document row.  Does not interact with the server.</summary>
		/// <param name="id" type="String">The ID of the document.</param>
		/// <param name="name" type="String">The filename.</param>
		/// <param name="extension" type="String">The document's extension.</param>
		/// <param name="size" type="Number">The size of the file in bytes.</param>
		/// <returns type="Prax.DocumentRow" />

		var tr = $('<tr />', { id: Prax.DocumentRow.idPrefix + id, extension: extension })
					.append(
						$('<td class="NameCell" />').append($('<span />').text(name))
					)
					.append($('<td />', { 'class': "SizeCell Right", text: Prax.toSizeString(size), title: size + ' bytes' }))
					.append($('<td />', { 'class': "DateCell Right", text: (date || new Date()).toShortDateString() }))
					.append($('<td class="StatusCell Center">Wait...</td>'))
					.append($('<td class="DeleteCell" />').append(
						$('<input />', { type: "submit", name: "id", title: "Delete " + name, value: "Delete " + id, "class": "Sprite16" })
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
		/// <param name="row" type="Prax.DocumentRow">The DocumentRow instance to remove.</param>
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
					docRow = self.createRow(doc.id, doc.name, doc.extension, doc.size, date);
					docRow.setNameLink(true);
				}

				if (doc.state === 'Scanned') {
					docRow.statusCell.text('Scanned');
					docRow.state = Prax.DocumentState.complete;
					docRow.formats = doc.formats;
					docRow.addDownloadLinks();
				} else {
					docRow.setProgress(doc.progressCaption, doc.progress);
					docRow.state = Prax.DocumentState.scanning;
				}
			}

			//Remove any documents that are no 
			//longer on the server.  (eg, they
			//were deleted in another browser)
			for (i = self.documents.length - 1; i >= 0; i--) {
				doc = self.documents[i];
				//Don't remove rows that are still uploading, 
				//even though they're not on the server yet.
				if (!existingIds[doc.id] && doc.state != Prax.DocumentState.uploading)
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