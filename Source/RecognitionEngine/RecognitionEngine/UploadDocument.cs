using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace Prax.Recognition
{
    class UploadDocument
    {
        public int[][] uploadedDocument;
        private OpenFileDialog openFileDialog = new OpenFileDialog();
        private PictureBox pictureBox;

        public UploadDocument(PictureBox picture)
        {
            pictureBox = picture;
            setDialogBoxSettings();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                documentToDoubleArray();
                preprocessImage();
            }
            //open training data
        }

        private void setDialogBoxSettings()
        {
            openFileDialog.InitialDirectory = "C:\\Users\\Amichai\\Pictures\\Handwritten\\";
            openFileDialog.Filter = " bmp files (*.bmp) | *.bmp | pdf files (*.pdf) | *.pdf | All files (*.*)|*.*";
            openFileDialog.FilterIndex = 3;
            openFileDialog.RestoreDirectory = true;
        }

        private void documentToDoubleArray()
        {
            #region Get Bitmap
            Bitmap FileBitmap = null;
            
            if (Path.GetExtension(openFileDialog.FileName) == ".pdf")
            {
                FileBitmap = GraphicsHelper.ConvertPdfToBitmap(openFileDialog.FileName);
            }
            else
            {
                //FileBitmap = GraphicsHelper.CompressBitmap(openFileDialog.FileName);
                FileBitmap = Bitmap.FromFile(openFileDialog.FileName) as Bitmap;
            }
            //FileBitmap = GraphicsHelper.MakeGrayscale(FileBitmap);
            #endregion

            //Display Bitmap
            DisplayUtility.DisplayBitmap(FileBitmap, pictureBox);

            uploadedDocument = GraphicsHelper.BitmapToDoubleArray(FileBitmap);
        }

        private void preprocessImage()
        {
            //Intended to house noise reduction and rotation algorithm
        }
    }
}
