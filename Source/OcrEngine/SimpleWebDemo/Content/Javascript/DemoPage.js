/// <reference path="../../Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="../../Scripts/jquery-ui-1.8.11.js" />
/// <reference path="../../Scripts/modernizr-1.7.js" />
/// <reference path="../../Scripts/jquery.uploadify.v2.1.4.js" />
/// <reference path="../../Scripts/jquery.filedrop.js" />
/// <reference path="../../Scripts/jsdiff.js" />
/// <reference path="../../Scripts/wDiff.js" />
/// <reference path="TextWizard.js" />

$('button, .Button').button();

var uploadPane = {
	pane: $('#uploadPane'),
	progress: $("#uploadProgress").progressbar(),
	show: function () {
		textWizard.hide();
		$('#initialButtons').slideUp();
		this.pane.slideDown();
	},
	hide: function () { this.pane.slideUp(); },

	setProgress: function (val) { this.progress.progressbar("option", "value", val); }
};

$('#upload input:file').uploadify({
	uploader: basePath + 'Content/uploadify.swf',
	script: basePath + 'Documents/UploadImage',
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

	onSelect: function (e, id, file) {
		console.log(arguments);
		uploadPane.show();
	},
	onProgress: function (e, id, file, data) {
		uploadPane.setProgress(data.percentage);
	},
	onComplete: function (e, id, file, response, data) {
		scanPane.start(response);
	}
});
$('#upload input:file').before("Upload your<br />own image");

var textWizard = new TextWizard("#textWizard", "#showTextWizard");

var buttonArea = $('#initialButtons');
var dropTarget = $('#dropTarget');

$('.GreenShade').css('opacity', .5);

//Handle drag&drop from the file system; this uploads the image.
dropTarget.filedrop({
	url: basePath + 'Documents/UploadImage',
	paramname: "image",
	maxfiles: 1,
	maxfilesize: 10, //Megabyte

	dragOver: function () { dropTarget.addClass('ui-state-hover'); },
	dragLeave: function () { dropTarget.removeClass('ui-state-hover'); },

	docOver: function () { buttonArea.addClass('Dragging'); },
	docLeave: function () { buttonArea.removeClass('Dragging'); },

	drop: function () { },

	uploadStarted: function (i, file, len) { uploadPane.show(); },

	progressUpdated: function (i, file, progress) {
		console.log(arguments);
		uploadPane.setProgress(progress);
	},
	uploadFinished: function (i, file, response, time) {
		scanPane.start(response);
	}
});
//Handle drag&drop from the text wizard
dropTarget.droppable({
	hoverClass: "ui-state-hover",
	tolerance: 'pointer',
	drop: function (event, ui) {
		scanPane.start(textWizard.imageId);
	}
});
$('.DragImage').bind({
	dragstart: function () { buttonArea.addClass('Dragging'); },
	dragstop: function () { buttonArea.removeClass('Dragging'); }
});

//This object controls the recognition phase.
//It polls the server for status and shows a 
//progress bar.
var scanPane = {
	pane: $('#scanPane'),
	progress: $('#scanProgress').progressbar(),
	image: $('#scanPreview'),
	id: '',

	start: function (id) {
		var self = this;

		//If we got here from an upload, hide it.
		uploadPane.hide();
		//If we got here from the wizard, the upload pane 
		//won't have been shown, so we still need to hide
		//the stuff that it usually hides.
		textWizard.hide();
		$('#initialButtons').slideUp();

		this.id = id;
		this.image.attr('src', basePath + "Documents/View/" + encodeURI(id));

		if (!this.image[0].complete) {	//If the image was cached, don't show Loading at all.
			this.image.addClass("Loading")
					  .bind("load", function () { self.image.removeClass("Loading"); })
		}

		this.pane.slideDown();

		setTimeout(function () { self.update(); }, 5000);
	},
	update: function () {
		var self = this;
		$.getJSON(basePath + "Documents/Status/" + encodeURI(self.id), function (doc) {
			switch (doc.state) {
				case "Queued":
				case "Scanning":
					self.progress.progressbar("option", "value", doc.progress);

					setTimeout(function () { self.update(); }, 3000);
					break;

				case "Error":
					alert($.trim("An error occurred.\r\nPlease try again later.\r\n\r\n" + doc.message));
					break;

				case "Complete":
					//Show a full bar as it slides up
					self.progress.progressbar("option", "value", 100);
					showResults(doc.text);
					break;

				default: alert("Unknown state: " + doc.state);
			}
		});
	}
};

function showResults(result) {
	if (!textWizard.text) {
		$('#resultTabs').tabs('option', 'selected', 1); //Select the Results tab
		$('.GenerationResult').hide();
	} else {
		$('#originalText').text(textWizard.text);
		$('#diff').html(WDiffString(textWizard.text, result));
		$('#resultTabs').tabs('option', 'selected', 2); //Select the Diff tab
		//TODO: Stats
	}

	$('#resultText').text(result);
	$('#resultImage img').attr('src', basePath + "Documents/View/" + encodeURI(scanPane.id));
	scanPane.pane.slideUp();

	$('#resultTabs').slideDown();
}
$('#resultTabs').tabs();