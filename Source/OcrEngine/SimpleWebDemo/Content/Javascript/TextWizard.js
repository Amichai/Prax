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
			var translationUpdateTimer = false;
			this.targetBox = parent.wizard.find('#translatedText');
			this.sourceBox = parent.wizard.find('#sourceText')
				.bind('mousemove mousedown mouseup', function (e) {
					if (e.which === 1) {
						self.targetBox.width(self.sourceBox.width());
						$('#translationBranding').width(self.sourceBox.outerWidth());
						self.targetBox.height(self.sourceBox.height());
					}
				})
				.bind('input propertychange', function () {
					if (translationUpdateTimer)
						clearInterval(translationUpdateTimer);

					var text = $.trim(self.sourceBox.val());
					if (text === self.lastSourceText) return;
					self.targetBox.addClass("Loading");

					translationUpdateTimer = setTimeout(function () {
						self.updateTranslation();
						translationUpdateTimer = false;
					}, 500);
				});
			google.setOnLoadCallback(function () {
				self.updateTranslation();
			});
			google.load("language", "1");
		},

		lastSourceText: null,
		updateTranslation: function () {
			var text = $.trim(this.sourceBox.val());
			if (text === this.lastSourceText) {
				this.targetBox.removeClass("Loading");
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
								  .removeClass("Loading");
				}
			);
		}
	}
};