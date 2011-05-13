/// <reference path="../../Scripts/jquery-1.5.1-vsdoc.js" />
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
	this.wizard.dialog({
		position: ['center', trigger.position().top + trigger.outerHeight() + 10],
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
			self.setStep(0, true);	//Don't animate

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

	trigger.click(function () { self.wizard.dialog("open"); });
}

TextWizard.prototype = {
	wizard: $(),
	back: $(),
	next: $(),

	stepContainer: $(),
	steps: [],
	currentStep: null,

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

			this.setLoading(); 	//We aren't ready until the translation library loads.

			google.load("language", "1", { callback: function () { self.markDirty(true); } });
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
				directionality: parent.translationStep.targetBox.css('direction'),

				width: '100%',
				height: '100%',

				theme_advanced_buttons1: "bold,italic,underline,separator,fontsizeselect,sub,sup,separator,forecolor,backcolor",
				theme_advanced_buttons2: "",
				theme_advanced_buttons3: ""
			});

		},
		onEnter: function (isBack) {
			//Work around Firefox layout bug
			var iframe = $('#formatBox_ifr');
			iframe.height(iframe.parent().height());

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
			this.imagePanel.hide();
			this.loadingPanel.show();

			var html = this.owner.formatStep.textBox.html();

			var self = this;
			$.post(basePath + "Documents/CreateFromHtml", { html: html }, function (imageId) {
				self.owner.imageId = imageId;
				self.image.attr('src', basePath + "Documents/View/" + encodeURI(imageId));
			});
		}
	}
};