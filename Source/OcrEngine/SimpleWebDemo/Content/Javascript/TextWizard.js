/// <reference path="../../Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="../../Scripts/jquery-ui-1.8.11.js" />
/// <reference path="../../Scripts/modernizr-1.7.js" />
/// <reference path="../../Scripts/jquery.uploadify.v2.1.4.js" />
/// <reference path="../../Scripts/bbq.js" />
/// <reference path="../../Scripts/jquery.form.wizard.js" />


function TextWizard(container, trigger) {
	var self = this;
	trigger = $(trigger);

	this.wizard = $(container);

	var width = this.wizard.width();	//Get the desired height from the CSS
	var height = this.wizard.height();

	this.wizard.dialog({
		position: ['center', trigger.position().top + trigger.outerHeight() + 10],
		resizable: false,
		width: width, height: height
	});
	this.wizard.dialog('option', {	//Force the desired height, taking into account any padding from the dialog
		width: width + (this.wizard.dialog('option', 'width') - this.wizard.width()),
		height: height + (this.wizard.dialog('option', 'height') - this.wizard.height())
	});

	this.translationStep.setUp(this);

	this.wizard.formwizard({
		historyEnabled: true
	});
	this.wizard.bind("step_shown", function (event, data) {
		if (data.isBackNavigation)
			return;
		if ($.isFunction(self[data.currentStep].onEnter))
			self[data.currentStep].onEnter();
	});

	trigger.click(function () { self.wizard.dialog("open"); });


}

TextWizard.prototype = {
	wizard: $(),

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
		}
	}
};