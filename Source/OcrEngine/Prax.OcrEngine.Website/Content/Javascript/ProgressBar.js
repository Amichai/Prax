/// <reference path="jQuery/jquery-1.4.1-vsdoc.js" />

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
		/// <summary>Gets or sets the caption displayed on the progress bar.</summary>
		if (arguments.length === 0)
			return this.outerCaption.text();
		this.captions.text(newVal);
	},
	val: function (newVal, max) {
		/// <summary>Gets or sets the progress bar's value.</summary>
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
	/// <summary>Creates and manages an HTML progress bar.</summary>
	/// <returns type="ProgressBar" />
	var container = this;
	if (!container.length) return null;

	var outerCaption, innerCaption, bar;

	if (container.children().length) {
		outerCaption = container.children(".OuterText");
		bar = container.children(".ProgressBar");
		innerCaption = bar.children();
	} else {
		container.addClass('ProgressContainer');
		outerCaption = $('<span class="OuterText" />').appendTo(container);
		bar = $('<div class="ProgressBar" />').appendTo(container);
		innerCaption = $('<span />').appendTo(bar);
	}

	return new ProgressBar(container, outerCaption, bar, innerCaption);
};
