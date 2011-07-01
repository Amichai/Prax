using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
//using PdfToImage;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows;
using System.Diagnostics;
//using iTextSharp;

namespace Prax.Recognition {
	public static class GraphicsHelper {
		public static Bitmap MakeGrayscale(Bitmap original) {
			//create a blank bitmap the same size as original
			Bitmap newBitmap = new Bitmap(original.Width, original.Height);

			//get a graphics object from the new image
			Graphics g = Graphics.FromImage(newBitmap);

			//create the grayscale ColorMatrix
			ColorMatrix colorMatrix = new ColorMatrix(
			   new float[][] 
	  {
		 new float[] {.3f, .3f, .3f, 0, 0},
		 new float[] {.59f, .59f, .59f, 0, 0},
		 new float[] {.11f, .11f, .11f, 0, 0},
		 new float[] {0, 0, 0, 1, 0},
		 new float[] {0, 0, 0, 0, 1}
	  });

			//create some image attributes
			ImageAttributes attributes = new ImageAttributes();

			//set the color matrix attribute
			attributes.SetColorMatrix(colorMatrix);

			//draw the original image on the new image
			//using the grayscale color matrix
			g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
			   0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

			//dispose the Graphics object
			g.Dispose();
			return newBitmap;
		}

		private static ImageCodecInfo GetEncoderInfo(string mimeType) {
			// Get image codecs for all image formats 
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

			// Find the correct image codec 
			for (int i = 0; i < codecs.Length; i++)
				if (codecs[i].MimeType == mimeType)
					return codecs[i];
			return null;
		}

		private static Bitmap CompressImage(string imagePath) {
			EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 70L);
			ImageCodecInfo jpegCodecInfo = GetEncoderInfo("image/jpeg");
			EncoderParameters parameters = new EncoderParameters(1);
			parameters.Param[0] = qualityParam;
			string fileName = Path.GetFileNameWithoutExtension(imagePath);
			Image img = Image.FromFile(imagePath);
			string newFileName = fileName + (new Random()).Next(99999).ToString() + ".jpg";
			img.Save(newFileName, jpegCodecInfo, parameters);
			return Bitmap.FromFile(newFileName) as Bitmap;
		}

		/*
		public static Bitmap ConvertPdfToBitmap(string imagePath)//, out string newImagePath)
		{
			string newImagePath = string.Empty;
			string outputFileName = "PdfToImageTest.tiff";
			PDFConvert convert = new PDFConvert();
			convert.FirstPageToConvert = 1;
			convert.LastPageToConvert = 1;
			convert.FitPage = true;
			convert.OutputFormat = "tiff32nc";
			convert.ResolutionX = 400;
			convert.ResolutionY = 400;

			if (File.Exists(outputFileName))
				File.Delete(outputFileName);

			if (!convert.Convert(imagePath, outputFileName)) {
				MessageBox.Show("File not converted");
				return null;
			}

			Image tempImg = Image.FromFile(outputFileName);
			Bitmap bp = tempImg as Bitmap;

			outputFileName = "PdfToImage.bmp";

			if (File.Exists(outputFileName))
				File.Delete(outputFileName);

			bp.Save(outputFileName);
			newImagePath = outputFileName;

			//return CompressBitmap(outputFileName);
			return bp;

		}
*/
		public static Bitmap CompressBitmap(string fileName) {
			string newFileName = string.Empty;
			if (Path.GetExtension(fileName).ToLower() != "jpg" || Path.GetExtension(fileName).ToLower() != "jpeg") {
				Image.FromFile(fileName).Save(Path.GetFileNameWithoutExtension(fileName) + ".jpg", ImageFormat.Jpeg);
				newFileName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
			}

			if (String.IsNullOrEmpty(newFileName))
				return CompressImage(fileName);
			else
				return CompressImage(newFileName);
		}


		public static int[][] BitmapToDoubleArray(Bitmap fileBitmap, string extension) {
			int[][] uploadedDocument;
			int width = fileBitmap.Width;
			int height = fileBitmap.Height;
			uploadedDocument = new int[width][];
			for (int i = 0; i < width; i++)
				uploadedDocument[i] = new int[height];

			Color pixelColor;
			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					switch(extension){
						case ".bmp":
						pixelColor = fileBitmap.GetPixel(i, j);
						uploadedDocument[i][j] = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);
							break;
						case ".png":
							uploadedDocument[i][j] = (255 - (int)fileBitmap.GetPixel(i,j).A);
							break;
					}
				}
			}
			return uploadedDocument;
		}
		static public Bitmap ConvertDoubleArrayToBitmap(this int[][] doubleArray, Color defaultColor) {
			int width = doubleArray.GetLength(0);
			int height = doubleArray[0].GetLength(0);
			Bitmap bitmapReturn = new Bitmap(width, height);

			Color pixelColor;

			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					pixelColor = defaultColor;

					if (doubleArray[i][j] >= 0 && doubleArray[i][j] < 256)
						pixelColor = Color.FromArgb(doubleArray[i][j], doubleArray[i][j], doubleArray[i][j]);
					else if (doubleArray[i][j] >= 256 && doubleArray[i][j] < 512)
						pixelColor = Color.FromArgb(0, 0, doubleArray[i][j] % 255);
					else if (doubleArray[i][j] >= 512 && doubleArray[i][j] < 768)
						pixelColor = Color.FromArgb(0, doubleArray[i][j] % 255, 0);
					else if (doubleArray[i][j] >= 768 && doubleArray[i][j] < 1024)
						pixelColor = Color.FromArgb(doubleArray[i][j] % 255, 0, 0);

					bitmapReturn.SetPixel(i, j, pixelColor);
				}
			}
			return bitmapReturn;
		}

		static public System.Drawing.Bitmap BitmapSourceToBitmap2(this System.Windows.Media.Imaging.BitmapSource srs) {
			System.Drawing.Bitmap btm = null;
			int width = srs.PixelWidth;
			int height = srs.PixelHeight;
			int stride = width * ((srs.Format.BitsPerPixel + 7) / 8);
			IntPtr ptr = Marshal.AllocHGlobal(height * stride);
			srs.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height * stride, stride);
			btm = new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format1bppIndexed, ptr);
			return btm;
		}
		public static void DrawBounds(Rectangle r, System.Drawing.Bitmap bitmap) {
			Graphics g = Graphics.FromImage(bitmap);
			g.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Black), r);
		}
		/// <summary>
		/// Extracts a sub-rectangle of content from a larger double array
		/// </summary>
		public static int[][] ExtractRectangularContentArea(this int[][] content, Rectangle bounds) {
			Bitmap contentAsBitmap = content.ConvertDoubleArrayToBitmap(Color.White);
			int[][] extractedContent = new int[bounds.Width][];
			for (int i = 0; i < bounds.Width; i++) {
				extractedContent[i] = new int[bounds.Height];
			}
			for (int i = 0; i < bounds.Width; i++) {
				for (int j = 0; j < bounds.Height; j++) {
					try{
					extractedContent[i][j] = content[bounds.X + i][bounds.Y + j];
					}
					catch {
						if (j == content[0].Length)
							j = bounds.Height;
						if (i == content.Length)
							i = bounds.Width;
					}
				}
			}
			return extractedContent;
		}
	}
}