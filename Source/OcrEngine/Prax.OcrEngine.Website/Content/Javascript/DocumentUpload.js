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

		simUploadLimit: 3,
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
	cancelledUploads: {},

	onUploadStart: function (e, queueId, file) {
		$('#noDocumentsMessage').hide();
		this.table.table.show();

		var row = this.table.createRow(queueId, file.name, file.size);
		row.state = Prax.DocumentState.uploading;
		row.setProgress('Waiting to upload', 0);

		var uploader = this;
		row.tr.addClass('Uploading');
		row.deleteDocument = function deleteUploadingDocument() {
			//Override the handler to remove the file from the queue.
			//In this method, `this` refers to the DocumentRow object
			uploader.cancelledUploads[queueId] = true;
			uploader.uploadButton.uploadifyCancel(this.id);
		};
	},
	onProgressChange: function (e, queueId, file, data) {
		var row = this.table.getRow(queueId);
		row.setProgress('Uploading: ' + data.percentage + '%', data.percentage);
	},
	onCancel: function (e, queueId, file, data) {
		this.table.getRow(queueId).tr.remove();
		this.cancelledUploads[queueId] = true;
	},
	onComplete: function (e, queueId, file, guid, data) {
		//If this upload was cancelled, but
		//finished anyway, delete the file.
		//I don't know whether this can happen.
		if (this.cancelledUploads[queueId]) {
			alert("I was cancelled!");
			var newRow = this.table.getRow(guid);
			if (newRow)
				newRow.deleteDocument();
			this.table.removeRow(row); 	//Remove our uploading row
			return;
		}

		//If an updateData call is made as the upload 
		//finishes (before this callback executes), it
		//will receive the new document and add it to 
		//the table.  (Since it doesn't exist yet with
		//the correct ID).
		//If this happens, we will see an existing row
		// with the new GUID.  In that case, we should 
		//remove our existing row.

		var row = this.table.getRow(queueId);
		if (this.table.getRow(guid))	//If there is a completed row from the server,
			this.table.removeRow(row); 	//Remove our uploading row
		else {
			row.setId(guid);
			row.setNameLink(true);
			row.setProgress('Queued', 0);
			row.state = Prax.DocumentState.scanning;
			row.tr.removeClass('Uploading');

			delete row.deleteDocument; //Remove our overridden delete handler and revert to the standard method from the prototype.

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