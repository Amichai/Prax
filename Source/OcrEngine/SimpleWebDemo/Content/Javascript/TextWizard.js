﻿/// <reference path="../../Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="../../Scripts/jquery-ui-1.8.11.js" />
/// <reference path="../../Scripts/modernizr-1.7.js" />
/// <reference path="../../Scripts/tiny_mce/jquery.tinymce.js" />

//The wizard layout is completely controlled by CSS.
//All layout done in Javascript gets values from the
//existing CSS layout.
//Text direction for the translation is specified in
//the CSS for #translatedText and in the server-side
//image renderer.  
//It also needs to be specified for the result view.

function TextWizard(container, trigger) {
	var self = this;
	trigger = $(trigger);

	this.wizard = $(container);

	var width = this.wizard.width(), height = this.wizard.height(); //Get the desired height from the CSS

	var hasShown = false;
	this.wizard.dialog({	//The vertical position whould line up with the button, after collapsing the logo.  The collapsed logo is 225 pixels shorter
		position: ['center', trigger.position().top + trigger.outerHeight() + 5 - 225],
		resizable: false,
		autoOpen: false,

		buttons: {
			"Back": function () { self.setStep(self.currentStep.index - 1); },
			"Next": function () { self.setStep(self.currentStep.index + 1); }
		},

		width: width, height: height,

		open: function myfunction() {
			if (hasShown) return;

			//After the dialog becomes visible, force 
			//the desired height, taking into account
			//any padding from the dialog itself.   I
			//cannot do this when creating it because
			//invisible elements have no size.
			self.wizard.dialog('option', {
				width: width + (self.wizard.dialog('option', 'width') - self.wizard.width()),
				height: height + (self.wizard.dialog('option', 'height') - self.wizard.height())
			});

			//Only set up the steps after we get a size.
			for (var i = 0; i < self.steps.length; i++)
				self.steps[i].setUp(self);
			self.setStep(0, true); //Don't animate

			hasShown = true;
		}
	});

	var buttons = this.wizard.parent().find(".ui-dialog-buttonset button");
	this.back = buttons.filter(":contains('Back')");
	this.next = buttons.filter(":contains('Next')");

	this.wizard.bind("step_shown", function (event, data) {
		if ($.isFunction(self[data.currentStep].onEnter))
			self[data.currentStep].onEnter(data.isBackNavigation);
	});

	var stepElems = this.wizard.children('.step')
		.css({ width: width, height: height })
		.wrapAll('<div class="StepContainer"></div>');

	this.steps = $.map(stepElems, function (elem, index) {
		var step = self[elem.id];
		step.id = elem.id;
		step.element = $(elem);
		step.index = index;
		return step;
	});
	this.stepContainer = this.steps[0].element.parent();

	trigger.click(function () {
		self.wizard.dialog("open");
		$('header img').addClass("Compact");
	});
}

TextWizard.prototype = {
	wizard: $(),
	back: $(),
	next: $(),

	stepContainer: $(),
	steps: [],
	currentStep: null,

	html: null,
	text: null,

	setStep: function (index, dontAnimate) {
		if (index < 0 || index >= this.steps.length)
			throw new Error("Bad step index " + index);

		var isBack = this.currentStep && this.currentStep.index > index;
		var targetStep = this.steps[index];
		targetStep.onEnter(isBack);

		var animSpeed = dontAnimate ? 0 : 600;
		if (index == 0)
			this.back.fadeOut(animSpeed);
		else
			this.back.fadeIn(animSpeed);

		//To prevent the Back button from ending up
		//where the Next button belongs, don't hide
		//Next completely.  Instead, make sure that
		//it occupies space, but isn't visible.
		if (index == this.steps.length - 1)
			this.next.fadeTo(animSpeed, 0);
		else
			this.next.fadeTo(animSpeed, 1);

		this.currentStep = targetStep;
		var targetPos = -targetStep.element.position().left;
		if (dontAnimate)
			this.stepContainer.stop().css({ left: targetPos }, animSpeed);
		else
			this.stepContainer.stop().animate({ left: targetPos }, animSpeed);
	},

	hide: function () { this.wizard.dialog("close"); },

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

			this.markDirty(true);
		},
		onEnter: function (isBack) { },

		updateTimer: false,
		stopTimer: function () {
			if (this.updateTimer) {
				clearTimeout(this.updateTimer);
				this.updateTimer = false;
			}
		},
		setLoading: function () {
			this.targetBox.addClass("Loading");
			this.owner.next.button("option", "disabled", true);
		},
		markDirty: function (updateNow) {
			this.stopTimer();

			var text = $.trim(this.sourceBox.val());
			if (text === this.lastSourceText) return;

			var self = this;
			this.setLoading();
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

			$.getJSON(
				"http://api.microsofttranslator.com/V2/Ajax.svc/Translate?oncomplete=?",
				{
					appId: "1D5D705DC49FBC716892B81D6455001C5732735E",
					text: text,
					to: "ar",
					contentType: "text/plain",
					category: "general"
				},
				function (result) {
					self.lastSourceText = text;
					self.targetBox.text(result)

					self.owner.html = self.targetBox.html();
					self.owner.text = result;

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
	//TODO: Uncomment for formatting
	//formatStep: {
	//	owner: null,
	//	textBox: $(),
	//	fontSizes: [10, 12, 14, 16, 20, 24, 28],
	//	fontUnit: "px",

	//	setUp: function (parent) {
	//		this.owner = parent;
	//		var self = this;

	//		var sizesString = this.fontSizes.join(this.fontUnit + ",") + this.fontUnit;

	//		this.textBox = $('#formatBox');
	//		this.textBox.tinymce({
	//			script_url: basePath + 'Scripts/tiny_mce/tiny_mce_src.js',
	//			theme: "advanced",
	//			directionality: parent.translationStep.targetBox.css('direction'),

	//			width: '100%',
	//			height: '100%',

	//			//Removed sup,sub - WPF can't render them in Arabic
	//			theme_advanced_buttons1: "bold,italic,underline,separator,fontsizeselect,separator,forecolor,backcolor",
	//			theme_advanced_buttons2: "",
	//			theme_advanced_buttons3: "",
	//			theme_advanced_font_sizes: sizesString,
	//			font_size_style_values: sizesString
	//		});
	//		this.textBox.addClass("OcrFont");
	//		this.element.hide();
	//	},
	//	setText: function (text) {
	//		this.textBox.html(
	//			$('<span></span>', { text: text, css: { fontSize: this.fontSizes[2] + this.fontUnit} }).wrap('<p></p>').parent().html()
	//		);
	//	},
	//	onEnter: function (isBack) {
	//		//Work around Firefox layout bug
	//		var iframe = $('#formatBox_ifr');
	//		iframe.height(iframe.parent().height());

	//		if (!isBack)
	//			this.setText(this.owner.translationStep.targetBox.text());

	//		this.owner.setStep(this.index + (isBack ? -1 : +1));
	//	}
	//},
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
			this.imagePanel.hide();
			this.loadingPanel.show();

			if (this.owner.formatStep) {
				this.owner.html = this.owner.formatStep.textBox.html();
				this.owner.text = this.owner.formatStep.textBox.text();
			}
			var self = this;
			$.post(basePath + "Documents/CreateFromHtml", { html: this.owner.html }, function (imageId) {
				self.owner.imageId = imageId;
				self.image.attr('src', basePath + "Documents/View/" + encodeURI(imageId));
			});
		}
	}
};