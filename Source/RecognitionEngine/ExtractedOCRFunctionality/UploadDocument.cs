using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using ExtractedOCRFunctionality;
using Prax.Recognition;
using System.Drawing.Imaging;

namespace ExtractedOCRFunctionality {
	public class Document {
		public int[][] document = null;
		private OpenFileDialog openFileDialog = new OpenFileDialog();
		private PictureBox pictureBox;
		public Bitmap documentImage { get; set; }
		private string filename;

		/// <summary>
		/// Document upload happens in the constructor.
		/// </summary>
		/// <param name="filename"></param>
		public Document(string filename) {
			this.filename = filename;
			documentToDoubleArray();
			preprocessImage();
		}
		/// <summary>
		/// Document upload happens in the constructor.
		/// </summary>
		public Document()
		{
			setDialogBoxSettings();
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				filename = openFileDialog.FileName;
				documentToDoubleArray();
				preprocessImage();
			}
			//open training data
		}

		private void setDialogBoxSettings()
		{
			//openFileDialog.InitialDirectory = "C:\\Users\\Amichai\\Pictures\\Handwritten\\";
			openFileDialog.Filter = " bmp files (*.bmp) | *.bmp | pdf files (*.pdf) | *.pdf | All files (*.*)|*.*";
			openFileDialog.FilterIndex = 3;
			openFileDialog.RestoreDirectory = true;
		}

		private void documentToDoubleArray()
		{
			#region Get Bitmap
			Bitmap FileBitmap = null;
			if (Path.GetExtension(filename) == ".pdf")
			{
				//FileBitmap = GraphicsHelper.ConvertPdfToBitmap(filename);
			}
			if (Path.GetExtension(filename) == ".png") {
				Image temp = Image.FromFile(filename);
				filename = Path.GetFileNameWithoutExtension(filename) + ".bmp";
				temp.Save(filename, ImageFormat.Bmp);
			}
			if (Path.GetExtension(filename) == ".bmp")
			{
				//FileBitmap = GraphicsHelper.CompressBitmap(openFileDialog.FileName);
				FileBitmap = Bitmap.FromFile(filename) as Bitmap;
			}
			//FileBitmap = GraphicsHelper.MakeGrayscale(FileBitmap);
			#endregion
			documentImage = FileBitmap;
			document = GraphicsHelper.BitmapToDoubleArray(FileBitmap);
		}

		private void preprocessImage()
		{
			//Intended to house noise reduction and rotation algorithm
		}

		public IteratedBoards DefineIteratedBoards() {
			IteratedBoards boards = new IteratedBoards();
			MatrixBoard currentBoard = new MatrixBoard(document);
			for (int i = 0; i < IteratedBoards.numberOfIterations; i++) {
				boards.Boards.Add(currentBoard);
				currentBoard = currentBoard.IterateBoard();
			}
			return boards;
		}
	}
}
