﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;


namespace Prax.Recognition
{
    static class DisplayUtility
    {
        public const int DisplayMarker1 = 509; //Blue 
        public const int DisplayMarker2 = 700; //Green
        public const int DisplayMarker3 = 900; //Red

        public static PictureBox DefaultPictureBox = null;

        static public void DisplayBitmap(Bitmap bitmapToRender, PictureBox picture)
        {
            int width = bitmapToRender.Width;
            int height = bitmapToRender.Height;
            picture.Image = bitmapToRender; 
        }

        static public Bitmap ConvertDoubleArrayToBitmap(int[][] doubleArray, Color defaultColor)
        {
            int width = doubleArray.GetLength(0);
            int height = doubleArray[0].GetLength(0);
            Bitmap bitmapReturn = new Bitmap(width, height);

            Color pixelColor;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
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

        public class DisplayMask
        {
            int width, height;
            int[][] displayMask, doubleArray;
            public DisplayMask(int[][] doubleArrayToMask)
            {
                this.doubleArray = doubleArrayToMask;
                this.width = doubleArrayToMask.Length;
                this.height = doubleArrayToMask[0].Length;
                displayMask = new int[width][];
                for (int i = 0; i < width; i++)
                    displayMask[i] = new int[height];
            }

            public void PixelToDisplay(Point pixelToAdd, int color)
            {
                displayMask[pixelToAdd.X][pixelToAdd.Y] = color;
            }

            public void RenderBitmap()
            {
                BitmapToRender = new Bitmap(width, height);

                Color pixelColor;

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        pixelColor = Color.White;
                        if (displayMask[i][j] == 0)
                        {
                            if (doubleArray[i][j] >= 0 && doubleArray[i][j] < 256)
                                pixelColor = Color.FromArgb(doubleArray[i][j], doubleArray[i][j], doubleArray[i][j]);
                            else if (doubleArray[i][j] >= 256 && doubleArray[i][j] < 512)
                                pixelColor = Color.FromArgb(0, 0, doubleArray[i][j] % 255);
                            else if (doubleArray[i][j] >= 512 && doubleArray[i][j] < 768)
                                pixelColor = Color.FromArgb(0, doubleArray[i][j] % 255, 0);
                            else if (doubleArray[i][j] >= 768 && doubleArray[i][j] < 1024)
                                pixelColor = Color.FromArgb(doubleArray[i][j] % 255, 0, 0);
                        }
                        else
                        {
                            if (displayMask[i][j] >= 0 && displayMask[i][j] < 256)
                                pixelColor = Color.FromArgb(displayMask[i][j], displayMask[i][j], displayMask[i][j]);
                            else if (displayMask[i][j] >= 256 && displayMask[i][j] < 512)
                                pixelColor = Color.FromArgb(0, 0, displayMask[i][j] % 255);
                            else if (displayMask[i][j] >= 512 && displayMask[i][j] < 768)
                                pixelColor = Color.FromArgb(0, displayMask[i][j] % 255, 0);
                            else if (displayMask[i][j] >= 768 && displayMask[i][j] < 1024)
                                pixelColor = Color.FromArgb(displayMask[i][j] % 255, 0, 0);
                        }

                        BitmapToRender.SetPixel(i, j, pixelColor);
                    }
                }
            }

            public Bitmap BitmapToRender = null;
            
            public void DisplayToUI()
            {
                RenderBitmap();
                DefaultPictureBox.Image = BitmapToRender;
            }
        }

        #region New Form Methods
        static public void NewFormForDisplay(List<Point> pointsToShow, int[][] doubleArray) {
            foreach (Point p in pointsToShow) {
                doubleArray[p.X][p.Y] = DisplayMarker2;
            }
            Bitmap bitmapToRender = DisplayUtility.ConvertDoubleArrayToBitmap(doubleArray, Color.White);
            produceNewForm(bitmapToRender, null, null);
        }

        static public void NewFormForDisplay(Bitmap bitmapToRender)
        {
            produceNewForm(bitmapToRender, null, null);
        }

        static public void NewFormForDisplay(int[][] doubleArray)
        {
            Bitmap bitmapToRender = DisplayUtility.ConvertDoubleArrayToBitmap(doubleArray, Color.White);
            produceNewForm(bitmapToRender, null, null);
        }

        static public void NewFormForDisplay(int[][] doubleArray, string displayLabel1, string displayLabel2)
        {
            Bitmap bitmapToRender = DisplayUtility.ConvertDoubleArrayToBitmap(doubleArray, Color.White);
            produceNewForm(bitmapToRender, displayLabel1, displayLabel2);
        }

        static private void produceNewForm(Bitmap bitmapToRender, string displayLabel1, string displayLabel2)
        {   
            Form tempDisplay = new Form();
            Label labelLabel = new Label();
            Label certaintyLabel1 = new Label();
            Label displayLocation = new Label();
            labelLabel.AutoSize = true;
            certaintyLabel1.AutoSize = true;
            displayLocation.AutoSize = true;
            int width = bitmapToRender.Width;
            int height = bitmapToRender.Height;

            if(displayLabel1 != null)
                labelLabel.Text = displayLabel1;
            if (displayLabel2 != null)
                certaintyLabel1.Text = displayLabel2;

            labelLabel.Location = new System.Drawing.Point(6, 7);
            certaintyLabel1.Location = new System.Drawing.Point(6, 66);
            displayLocation.Location = new System.Drawing.Point(50, 7);

            tempDisplay.Controls.Add(labelLabel);
            tempDisplay.Controls.Add(certaintyLabel1);
            tempDisplay.Controls.Add(displayLocation);

            tempDisplay.SetBounds(10, 10, width, height);
            tempDisplay.ClientSize = new System.Drawing.Size(150, 88);
            tempDisplay.BackgroundImage = bitmapToRender;
            tempDisplay.BackgroundImageLayout = ImageLayout.Center;
            tempDisplay.StartPosition = System.Windows.Forms.FormStartPosition.Manual;

            tempDisplay.Location = new System.Drawing.Point(100, 100);
            tempDisplay.ShowDialog();
        }
        #endregion
    }
}
