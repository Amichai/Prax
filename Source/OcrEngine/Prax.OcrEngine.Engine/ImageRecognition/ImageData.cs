using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Prax.OcrEngine.Engine.ImageRecognition {
	///<summary>Stores image data and recognized information.</summary>
	///<remarks>
	/// This class passes through the ImageWorkflow and is
	/// updated by the various steps to include OCR results.
	/// Some steps, such as noise removal and preliminary 
	/// rotation correction, will modify the raw image data;
	/// others will generate additional information (such
	/// as line information) and store it in other properties
	/// in this class.
	/// Add properties to this class to store all intermediary
	/// and final output from OCR.
	///</remarks>
	class ImageData {
		Bitmap UploadedImage;
		int[][] ImageMatrix;
		//TODO: Color or B&W?
	}
}
