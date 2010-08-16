/// <reference path="jquery-1.4.1-vsdoc.js" />
/// <reference path="ProgressBar.js" />

if (!Prax)
	var Prax = {};

Prax.DocumentList = {
	start: function (timeout) {
		if (timeout > 0)
			setTimeout(Prax.DocumentList.updateBars, timeout);
	},
	updateBars: function () {
		$.getJSON("/Documents/Data", function (data) {
			for (var i = 0; i < data.documents.length; i++) {
				var doc = data.documents[i];
				var row = $('#document-' + doc.id);

				if (doc.state === 'Scanned')
					row.children('.StatusCell').text('Scanned');
				else {
					var bar = row.find('.StatusCell .ProgressContainer').progressBar();
					bar.val(doc.progress);
					bar.text(doc.progressCaption);
				}
			}

			if (data.refreshTimeout > 0)
				setTimeout(Prax.DocumentList.updateBars, data.refreshTimeout);
		});
	}
};