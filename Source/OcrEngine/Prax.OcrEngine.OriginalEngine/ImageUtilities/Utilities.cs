﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows;
using Color = System.Drawing.Color;

namespace Prax.OcrEngine.Engine.ImageUtilities {
	static class Utilities {
		public static Stream CreateStream(this BitmapSource image) {
			var stream = new MemoryStream();
			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(image));
			encoder.Save(stream);
			stream.Position = 0;

			return stream;
		}
		public static BitmapSource ToBitmap(this DrawingGroup dg) {
			var dv = new DrawingVisual();
			using (var c = dv.RenderOpen())
				c.DrawImage(new DrawingImage(dg), new Rect(dg.Bounds.Size));
			var rtb = new RenderTargetBitmap((int)dg.Bounds.Width, (int)dg.Bounds.Height, 96, 96, PixelFormats.Pbgra32);
			rtb.Render(dv);
			return rtb;
		}


		public static int[][] BitmapToDoubleArray(this Bitmap fileBitmap, string extension) {
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
		public static void DrawBounds(this System.Drawing.Bitmap bitmap, Rectangle r) {
			Graphics g = Graphics.FromImage(bitmap);
			g.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Black), r);
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
		/// <summary>
		/// Extracts a sub-rectangle of content from a larger double array
		/// </summary>
		public static int[][] ExtractRectangularContentArea(this int[][] content, Rectangle bounds) {
			//Bitmap contentAsBitmap = content.ConvertDoubleArrayToBitmap(Color.White);
			int[][] extractedContent = new int[bounds.Width][];
			for (int i = 0; i < bounds.Width; i++) {
				extractedContent[i] = new int[bounds.Height];
			}
			for (int i = 0; i < bounds.Width; i++) {
				for (int j = 0; j < bounds.Height; j++) {
					try {
						extractedContent[i][j] = content[bounds.X + i][bounds.Y + j];
					} catch {
						if (j == content[0].Length)
							j = bounds.Height;
						if (i == content.Length)
							i = bounds.Width;
					}
				}
			}
			return extractedContent;
		}

		///
	}
}