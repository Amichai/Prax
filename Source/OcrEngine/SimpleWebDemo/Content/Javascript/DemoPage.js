/// <reference path="../../Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="../../Scripts/jquery-ui-1.8.11.js" />
/// <reference path="../../Scripts/modernizr-1.7.js" />
/// <reference path="../../Scripts/jquery.uploadify.v2.1.4.js" />
/// <reference path="../../Scripts/bbq.js" />
/// <reference path="../../Scripts/jquery.form.wizard.js" />
/// <reference path="TextWizard.js" />

$('button, .Button').button();

$('#upload input:file').uploadify({
	uploader: basePath + 'Content/uploadify.swf',
	script: basePath + 'Documents/UploadImage',
	fileDataName: 'image',
	auto: true,

	height: $('#upload').outerHeight(),
	width: $('#upload').outerWidth(),
	wmode: 'transparent',
	hideButton: true,
	queueID: 'I-Dont-Exist' //I don't want their default queue at all
});
$('#upload input:file').before("Upload your<br />own image");

var textWizard = new TextWizard("#textWizard", "#showTextWizard");

var buttonArea = $('.LargeButtons');

$('#dropTarget').droppable({
	hoverClass: "ui-state-hover",
	tolerance: 'pointer',
	drop: function (event, ui) {
		alert($(ui.draggable).html());
		textWizard.wizard.dialog("close");
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