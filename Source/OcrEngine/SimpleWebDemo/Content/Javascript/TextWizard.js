/// <reference path="../../Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="../../Scripts/jquery-ui-1.8.11.js" />
/// <reference path="../../Scripts/modernizr-1.7.js" />
/// <reference path="../../Scripts/jquery.uploadify.v2.1.4.js" />
/// <reference path="../../Scripts/bbq.js" />
/// <reference path="../../Scripts/jquery.form.wizard.js" />


function TextWizard(container, trigger) {
	var self = this;
	trigger = $(trigger);

	this.wizard = $(container)
		.dialog({
			autoOpen: false,
			position: ['center', trigger.position().top + trigger.outerHeight() + 10],
			resizable: false
		});

	this.translationStep.setUp(this);

	trigger.click(function () { self.wizard.dialog("open"); });
}

TextWizard.prototype = {
	wizard: $(),

	translationStep: {
		sourceBox: $(),
		targetBox: $(),

		setUp: function (parent) {
			var self = this;

			this.targetBox = parent.wizard.find('#translatedText');
			this.sourceBox = parent.wizard.find('#sourceText')
				.bind('mousemove mousedown mouseup', function (e) {
					if (e.which === 1) {
						self.targetBox.width(self.sourceBox.width());
						$('#translationBranding').width(self.sourceBox.outerWidth());
						self.targetBox.height(self.sourceBox.height());
					}
				})
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