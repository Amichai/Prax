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
			if (from in this && this[from] === item)
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
	};
})();
var Path = {
	getFileName: function getFileName(path) {
		/// <summary>Gets the filename of a path.</summary>
		/// <param name="path" type="String">The path.</param>
		/// <returns type="String" />
		if (!path) return '';

		for (var i = path.length; --i >= 0; ) {
			var ch = path[i];
			if (ch === '\\' || ch === '/' || ch === ':')
				return path.Substring(i + 1, path.length - i - 1);

		}
		return path; //No folder
	},

	getFileNameWithoutExtension: function getFileNameWithoutExtension(path) {
		/// <summary>Gets the filename of a path without its extension.</summary>
		/// <param name="path" type="String">The path.</param>
		/// <returns type="String" />
		if (!path) return '';

		path = Path.getFileName(path);

		var i;
		if ((i = path.lastIndexOf('.')) < 0)
			return path; // No path extension found
		else
			return path.substring(0, i);
	},
	getExtension: function getExtension(path) {
		/// <summary>Gets the extension of a path or filename.</summary>
		/// <param name="path" type="String">The filename.</param>
		/// <returns type="String" />
		if (!path) return '';

		for (var i = path.length; --i >= 0; ) {
			var ch = path.charAt(i);
			if (ch === '.') {
				if (i != path.length - 1)
					return path.substring(i);
				else
					return '';
			}
			if (ch === '\\' || ch === '/' || ch === ':')
				break;
		}
		return '';
	}
};

Date.prototype.toShortDateString = function toShortDateString() {
	return (this.getMonth() + 1) + '/' + this.getDate() + '/' + this.getFullYear();
};