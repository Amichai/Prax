using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Prax.OcrEngine.Engine.ImageUtilities;
using Prax.OcrEngine.Engine.HeuristicGeneration;

namespace Prax.OcrEngine.Engine {
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
	public class ImageData {
		public ImageData(Stream stream) {
			UploadedImage = (Bitmap)Bitmap.FromStream(stream);
			this.ImageMatrix = UploadedImage.BitmapToDoubleArray(".png");
		}
		Bitmap UploadedImage;
		//ImageWpf UploadedImage = new ImageWpf();
		int[][] ImageMatrix;
		//TODO: Color or B&W?
		public void SaveFile(string filename) {
			UploadedImage.Save(filename);
		}
		public IterateBoards DefineIteratedBoards() {
			var boards = new IterateBoards();
			var currentBoard = new MatrixBoard(ImageMatrix);
			for (int i = 0; i < IterateBoards.numberOfIterations; i++) {
				boards.Boards.Add(currentBoard);
				currentBoard = currentBoard.IterateBoard();
			}
			return boards;
		}
	}

}
