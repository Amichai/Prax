/// <reference path="../../Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="../../Scripts/jquery-ui-1.8.11.js" />
/// <reference path="../../Scripts/modernizr-1.7.js" />
/// <reference path="../../Scripts/jquery.uploadify.v2.1.4.js" />
/// <reference path="../../Scripts/bbq.js" />
/// <reference path="../../Scripts/jquery.form.wizard.js" />
/// <reference path="../../Scripts/tiny_mce/jquery.tinymce.js" />


function TextWizard(container, trigger) {
	var self = this;
	trigger = $(trigger);

	this.wizard = $(container);

	var width = this.wizard.width(), height = this.wizard.height(); //Get the desired height from the CSS

	this.wizard.dialog({
		position: ['center', trigger.position().top + trigger.outerHeight() + 10],
		resizable: false,
		autoOpen: false,

		buttons: {
			"Back": function () { self.wizard.formwizard("back"); },
			"Next": function () { self.wizard.formwizard("next"); }
		},

		width: width, height: height
	});
	this.wizard.dialog('option', {	//Force the desired height, taking into account any padding from the dialog
		width: width + (this.wizard.dialog('option', 'width') - this.wizard.width()),
		height: height + (this.wizard.dialog('option', 'height') - this.wizard.height())
	});

	var buttons = this.wizard.parent().find(".ui-dialog-buttonset button");
	this.back = buttons.filter(":contains('Back')");
	this.next = buttons.filter(":contains('Next')");

	this.translationStep.setUp(this);
	this.formatStep.setUp(this);
	this.finalStep.setUp(this);

	this.wizard.formwizard({
		inDuration: 0,
		outDuration: 0
	});
	this.wizard.bind("step_shown", function (event, data) {
		if ($.isFunction(self[data.currentStep].onEnter))
			self[data.currentStep].onEnter(data.isBackNavigation);
	});

	trigger.click(function () { self.wizard.dialog("open"); });
	this.translationStep.onEnter();
}

TextWizard.prototype = {
	wizard: $(),
	back: $(),
	next: $(),

	translationStep: {
		sourceBox: $(),
		targetBox: $(),

		owner: null,

		setUp: function (parent) {
			this.owner = parent;
			var self = this;

			this.targetBox = parent.wizard.find('#translatedText');
			this.sourceBox = parent.wizard.find('#sourceText')
				.bind('input propertychange', function () { self.markDirty(); });

			google.setOnLoadCallback(function () { self.markDirty(true); });
			google.load("language", "1");
		},
		onEnter: function (isBack) {
			this.owner.back.hide();
		},
		updateTimer: false,
		stopTimer: function () {
			if (this.updateTimer) {
				clearTimeout(this.updateTimer);
				this.updateTimer = false;
			}
		},
		markDirty: function (updateNow) {
			this.stopTimer();

			var text = $.trim(this.sourceBox.val());
			if (text === this.lastSourceText) return;

			var self = this;
			this.targetBox.addClass("Loading");
			this.owner.next.button("option", "disabled", true);
			if (updateNow)
				this.updateTranslation();
			else
				this.updateTimer = setTimeout(function () {
					self.updateTimer = false;
					self.updateTranslation();
				}, 500);
		},
		lastSourceText: null,
		updateTranslation: function () {
			var text = $.trim(this.sourceBox.val());
			if (text === this.lastSourceText) {
				this.markTranslated();
				return;
			}

			var self = this;
			google.language.translate(
				{ text: text, type: "text" },
				"", //Detect source language
				"ar",
				function (result) {
					self.lastSourceText = text;
					self.targetBox.text(result.translation)

					if (text === $.trim(self.sourceBox.val()))
						self.markTranslated();
					else //If the text changed since we sent the call, we're still dirty
						self.markDirty();
				}
			);
		},
		markTranslated: function () {
			this.targetBox.removeClass("Loading");
			this.owner.next.button("option", "disabled", false);
		}
	},
	formatStep: {
		owner: null,
		textBox: $(),

		setUp: function (parent) {
			this.owner = parent;
			var self = this;

			this.textBox = $('#formatBox');
			this.textBox.tinymce({
				script_url: basePath + 'Scripts/tiny_mce/tiny_mce_src.js',
				theme: "advanced",

				width: '100%',
				height: this.textBox.parent().height() - 1,

				theme_advanced_buttons1: "bold,italic,underline,separator,fontsizeselect,sub,sup,separator,forecolor,backcolor",
				theme_advanced_buttons2: "",
				theme_advanced_buttons3: ""

			});
		},
		onEnter: function (isBack) {
			this.owner.back.show();
			this.owner.next.show();
			if (!isBack)
				this.textBox.text(this.owner.translationStep.targetBox.text());
		}
	},
	finalStep: {
		owner: null,
		loadingPanel: $(),
		imagePanel: $(),
		image: $(),

		setUp: function (parent) {
			this.owner = parent;
			var self = this;

			this.loadingPanel = $('#loadingPanel');
			this.imagePanel = $('#imagePanel');
			this.image = $('#generatedImage');

			this.image.bind('load', function () {
				self.loadingPanel.hide();
				self.imagePanel.show();
			}); 	//When the image finishes loading, hide the loading icon and show the image.
			this.image.draggable({
				appendTo: 'body',
				revert: 'invalid', 	//Only revert if it wasn't dropped
				helper: 'clone',
				zIndex: 1256	//Above the dialog
			});
		},

		onEnter: function () {	//It is not possible to go back into the final step
			this.owner.back.show();
			this.owner.next.hide();

			this.imagePanel.hide();
			this.loadingPanel.show();

			var html = this.owner.formatStep.textBox.html();

			var self = this;
			$.post(basePath + "Documents/CreateFromHtml", { html: html }, function (imageId) {
				self.image.attr('src', basePath + "Documents/View/" + encodeURIComponent(imageId));
			});
		}
	}
};