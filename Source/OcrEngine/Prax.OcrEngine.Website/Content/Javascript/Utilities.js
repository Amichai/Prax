if (!Prax)
	var Prax = {};

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