/// <reference path="jQuery/jquery-1.4.1-vsdoc.js" />
/// <reference path="jQuery/swfobject.js" />
/// <reference path="jQuery/jquery.uploadify.v2.1.0.js" />
/// <reference path="Utilities.js" />
/// <reference path="ProgressBar.js" />
/// <reference path="DocumentTable.js" />

Prax.DocumentUploader = function DocumentUploader(table, selector) {
	/// <summary>Manages an AJAX-enabled upload UI.</summary>
	/// <param name="table" type="Prax.DocumentTable">The table to add the uploaded files to.</param>
	/// <param name="buttonSelector" type="String">The jQuery selector of the element to add the upload button to.</param>
	this.uploadButton = $(selector);
	this.table = table;

	var self = this;
	var handle = function (handler) {
		return function () { return handler.apply(self, arguments); }
	};

	this.uploadButton.uploadify({
		uploader: '/Content/Uploadify/uploadify.swf',
		script: '/Documents/UploadAjax',
		fileDataName: 'file',

		auto: true, 	//Upload files immediately after selection
		multi: true, 	//Allow multi-selection

		queueID: 'I-Dont-Exist', //I don't want their default queue at all

		//File type dropdown
		fileDesc: 'Image Files',
		fileExt: '*.jpg;*.jpe;*.jpeg;*.tiff;*.tif;*.bmp;*.pdf;*.png;*.gif',

		buttonText: 'Browse',

		//Events
		onCancel: handle(this.onCancel),
		onSelect: handle(this.onUploadStart),
		onProgress: handle(this.onProgressChange),
		onComplete: handle(this.onComplete),
		onError: handle(this.onError)
	});
};
Prax.DocumentUploader.prototype = {
	uploadButton: $(),
	table: new Prax.DocumentTable($()),

	onUploadStart: function (e, queueId, file) {
		$('#noDocumentsMessage').hide();
		this.table.table.show();

		var row = this.table.createRow(queueId, file.name, file.size);
		row.setProgress('Waiting to upload', 0);
	},
	onProgressChange: function (e, queueId, file, data) {
		var row = this.table.getRow(queueId);
		row.setProgress('Uploading: ' + data.percentage + '%', data.percentage);
	},
	onCancel: function (e, queueId, file, data) {
		this.table.getRow(queueId).tr.remove();
	},
	onComplete: function (e, queueId, file, guid, data) {
		//If an updateData call is made as the upload 
		//finishes (before this callback executes), it
		//will receive the new document and add it to 
		//the table.  (Since it doesn't exist yet with
		//the correct ID).
		//If this happens, we will see an existing row
		// with the new GUID.  In that case, we should 
		//remove our existing row.

		var row = this.table.getRow(queueId);
		if (this.table.getRow(guid))
			this.table.removeRow(row);
		else {
			row.setId(guid);
			row.setNameLink(true);
			row.setProgress('Queued', 0);

			this.table.updateData();
		} 
	},
	onError: function (e, queueId, file, error) {
		if (console)
			console.log(arguments);
		else
			alert(error);
	}
};