﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PdfToImage;
using System.Windows.Forms;
using iTextSharp;

namespace Prax.Recognition
{   
    static class GraphicsHelper
    {
        public static Bitmap MakeGrayscale(Bitmap original)
        {
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

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }

        private static Bitmap CompressImage(string imagePath)
        {
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

            if (!convert.Convert(imagePath, outputFileName))
            {
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

        public static Bitmap CompressBitmap(string fileName)
        {
            string newFileName = string.Empty;
            if (Path.GetExtension(fileName).ToLower() != "jpg" || Path.GetExtension(fileName).ToLower() != "jpeg")
            {
                Image.FromFile(fileName).Save(Path.GetFileNameWithoutExtension(fileName) + ".jpg", ImageFormat.Jpeg);
                newFileName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
            }

            if (String.IsNullOrEmpty(newFileName))
                return CompressImage(fileName);
            else
                return CompressImage(newFileName);
        }

        public static int[][] BitmapToDoubleArray(Bitmap fileBitmap)
        {
            int[][] uploadedDocument;
            int width = fileBitmap.Width;
            int height = fileBitmap.Height;
            uploadedDocument = new int[width][];
            for (int i = 0; i < width; i++)
                uploadedDocument[i] = new int[height];

            Color pixelColor;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    pixelColor = fileBitmap.GetPixel(i, j);
                    uploadedDocument[i][j] = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);
                }
            }
            return uploadedDocument;
        }
    }
}
