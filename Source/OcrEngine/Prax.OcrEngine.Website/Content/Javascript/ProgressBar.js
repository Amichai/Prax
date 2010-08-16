/// <reference path="jquery-1.4.1-vsdoc.js" />
(function ($) {
	function ProgressBar(container, outerCaption, bar, innerCaption) {
		this.container = container;
		this.outerCaption = outerCaption;
		this.bar = bar;
		this.innerCaption = innerCaption;
		this.captions = $(innerCaption).add(outerCaption);
	}
	ProgressBar.prototype = {
		container: $(),
		outerCaption: $(),
		bar: $(),
		innerCaption: $(),
		captions: $(),

		text: function (newVal) {
			if (arguments.length === 0)
				return this.outerCaption.text();
			this.captions.text(newVal);
		},
		val: function (newVal, max) {
			if (arguments.length === 0)
				return this.bar.width();
			if (max)
				newVal = 100 * newVal / max;

			this.bar.width(newVal + '%');
			if (newVal === 0)
				this.innerCaption.width(0);
			else
				this.innerCaption.width(100 * 100 / newVal + '%');
		}
	};

	jQuery.fn.progressBar = function () {
		var container = this;
		if (!container.length) return null;

		if (container.children().length) {
			var outerCaption = container.children(".OuterText");
			var bar = container.children(".ProgressBar");
			var innerCaption = bar.children();
		} else {
			container.addc('ProgressContainer');
			var outerCaption = container.append('<span class="OuterText" />');
			var bar = container.append('<div class="ProgressBar" />');
			var innerCaption = container.append('<span />');
		}

		return new ProgressBar(container, outerCaption, bar, innerCaption);
	};
})(jQuery);