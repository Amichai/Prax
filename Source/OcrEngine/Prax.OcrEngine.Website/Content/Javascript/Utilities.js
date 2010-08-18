if (!Prax)
	var Prax = {};

//Copied from https://developer.mozilla.org/En/Core_JavaScript_1.5_Reference/Objects/Array/IndexOf
if (!Array.prototype.indexOf) {
	Array.prototype.indexOf = function (item /*, from*/) {
		var len = this.length >>> 0;

		var from = Number(arguments[1]) || 0;

		from = (from < 0) ? Math.ceil(from) : Math.floor(from);
		if (from < 0)
			from += len;

		for (; from < len; from++) {
			if (from in this && this[from] === elt)
				return from;
		}
		return -1;
	};
}

(function () {
	var units = ["bytes", "KB", "MB", "GB", "TB"];
	Prax.toSizeString = function toSizeString(size) {
		/// <summary>Converts a file size in bytes to a string in the appropriate unit.</summary>
		/// <returns type="String" />
		var order = 0;
		while (size >= 1024 && order + 1 < units.length) {
			order++;
			size /= 1024;
		}

		var integral = Math.floor(size);
		var decimal = size - integral;
		if (decimal < .1)
			return integral + " " + units[order];
		return integral + "." + Math.floor(decimal * 10) + " " + units[order];
	}
})();

Date.prototype.toShortDateString = function toShortDateString() {
	return (this.getMonth() + 1) + '/' + this.getDate() + '/' + this.getFullYear();
};