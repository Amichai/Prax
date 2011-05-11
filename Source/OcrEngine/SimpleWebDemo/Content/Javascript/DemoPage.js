/// <reference path="../../Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="../../Scripts/jquery-ui-1.8.11.js" />
/// <reference path="../../Scripts/modernizr-1.7.js" />
/// <reference path="../../Scripts/jquery.uploadify.v2.1.4.js" />
/// <reference path="../../Scripts/jquery.form.wizard.js" />
/// <reference path="../../Scripts/jquery.filedrop.js" />
/// <reference path="TextWizard.js" />

$('button, .Button').button();

var uploadPane = {
	pane: $('#uploadPane'),
	progress: $("#uploadProgress").progressbar(),
	show: function () {
		textWizard.wizard.dialog("close");
		$('#initialButtons').slideUp();
		this.pane.slideDown();
		//$.fn.show.apply(this.pane, arguments);
	},
	hide: function () { $.fn.hide.apply(this.pane, arguments); },

	setProgress: function (val) { this.progress.progressbar("option", "value", val); }
};

$('#upload input:file').uploadify({
	//uploader: basePath + 'Content/uploadify.swf',
	//script: basePath + 'Documents/UploadImage',
	uploader: '/Content/uploadify.swf',
	script: '/Documents/UploadImage',
	fileDataName: 'image',
	auto: true,
	folder: 'abc',

	fileDesc: 'Image Files',
	fileExt: '*.jpg;*.jpe;*.jpeg;*.bmp;*.png;*.gif',

	height: $('#upload').outerHeight(),
	width: $('#upload').outerWidth(),
	wmode: 'transparent',
	hideButton: true,
	//queueID: 'I-Dont-Exist', //I don't want their default queue at all

	onSelect: function (e, queueId, file) {
		console.log(arguments);
		uploadPane.show();
	},
	onProgress: function (e, queueId, file, data) {
		uploadPane.setProgress(data.percentage);
	},
	onComplete: function (e, queueId, file) {
		uploadPane.hide();
	}
});
$('#upload input:file').before("Upload your<br />own image");

var textWizard = new TextWizard("#textWizard", "#showTextWizard");

var buttonArea = $('#initialButtons');

var dropTarget = $('#dropTarget').droppable({
	hoverClass: "ui-state-hover",
	tolerance: 'pointer',
	drop: function (event, ui) {
		alert($(ui.draggable).html());
		textWizard.wizard.dialog("close");
	}
});
dropTarget.filedrop({
	url: basePath + 'Documents/UploadImage',
	paramname: "image",
	maxfiles: 1,
	maxfilesize: 10, //Megabyte

	dragOver: function () { dropTarget.addClass('ui-state-hover'); },
	dragLeave: function () { dropTarget.removeClass('ui-state-hover'); },
	docOver: function () { buttonArea.addClass('Dragging'); },
	docLeave: function () { buttonArea.removeClass('Dragging'); },

	drop: function () { uploadPane.show(); },
	uploadStarted: function (i, file, len) {

	},
	progressUpdated: function (i, file, progress) {
		console.log(arguments);
		uploadPane.setProgress(progress);
	},
	uploadFinished: function (i, file, response, time) {
		uploadPane.hide();
	}
});

$('.DragImage').bind({
	dragstart: function () {
		buttonArea.addClass('Dragging');
	},
	dragstop: function () {
		buttonArea.removeClass('Dragging');
	}
});