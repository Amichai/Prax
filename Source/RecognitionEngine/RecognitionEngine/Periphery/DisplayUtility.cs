using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO;


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

                    doubleArray[i][j] = Math.Abs(doubleArray[i][j]);

                    if (doubleArray[i][j] >= 0 && doubleArray[i][j] < 256)
                        pixelColor = Color.FromArgb(doubleArray[i][j], doubleArray[i][j], doubleArray[i][j]);
                    else if (doubleArray[i][j] >= 256 && doubleArray[i][j] < 512)
                        pixelColor = Color.FromArgb(0, 0, doubleArray[i][j] % 255);
                    else if (doubleArray[i][j] >= 512 && doubleArray[i][j] < 768)
                        pixelColor = Color.FromArgb(0, doubleArray[i][j] % 255, 0);
                    else if (doubleArray[i][j] >= 768 && doubleArray[i][j] < 1024)
                        pixelColor = Color.FromArgb(doubleArray[i][j] % 255, 0, 0);
                    else if (doubleArray[i][j] >= 1024 && doubleArray[i][j] < 1280)
                        pixelColor = Color.FromArgb(0, 0, doubleArray[i][j] % 255);
                    else if(doubleArray[i][j] >= 1280 && doubleArray[i][j] < 1536)
                        pixelColor = Color.FromArgb(0, doubleArray[i][j] % 255, 0);
                    else if(doubleArray[i][j] >= 1536 && doubleArray[i][j] < 1792)
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
            
            tempDisplay.ClientSize = new System.Drawing.Size(150+30, 88+50);
            tempDisplay.BackgroundImage = bitmapToRender;
            tempDisplay.BackgroundImageLayout = ImageLayout.Center;
            tempDisplay.StartPosition = System.Windows.Forms.FormStartPosition.Manual;

            tempDisplay.Location = new System.Drawing.Point(100, 100);
            tempDisplay.ShowDialog();
        }
        #endregion

        

        public static int[][] NormalizeAndPrintToFile(int[][] p) {
            StreamWriter matrix  = new StreamWriter("matrix.txt", true);

            int width = p.Length;
            int height = p[0].Length;
            int[][] normalizedBitmap = new int[width][];

            for (int i = 0; i < width; i++)
                normalizedBitmap[i] = new int[height];

            for(int i= 0;i < width; i++){
                for(int j=0; j < height; j++){
                    normalizedBitmap[i][j] = Math.Abs(p[i][j]);
                    if (normalizedBitmap[i][j] > 100)
                        normalizedBitmap[i][j] = (int)Math.Sqrt(normalizedBitmap[i][j]);
                    matrix.Write(p[i][j].ToString() + " ");
                }
                matrix.Write("\n");
            }
            matrix.Write("\n");
            matrix.Close();
            return normalizedBitmap;
        }

        static private int yValueIndex = 0;
        static Bitmap segmentsToSave = new Bitmap(1, 1);
        static int counter = 1;

        public static void LogImage(int[][] bit) {
            Bitmap seg = ConvertDoubleArrayToBitmap(bit, Color.White);
            Size imageSize;
            string filename = "allImages" + counter.ToString() + "matches" + ".bmp";
            if (seg.Width > segmentsToSave.Width)
                imageSize = new Size(seg.Width, segmentsToSave.Height + seg.Height);
            else
                imageSize = new Size(segmentsToSave.Width, segmentsToSave.Height + seg.Height);
            Bitmap newBitmap = new Bitmap(imageSize.Width, imageSize.Height);
            Graphics g;
            g = Graphics.FromImage(newBitmap);
            g.DrawImage(segmentsToSave, new Point(0, 0));
            g.DrawImage(seg, new Point(0, segmentsToSave.Height));
            segmentsToSave = newBitmap;
            segmentsToSave.Save(filename);
            double fileSize = new FileInfo(filename).Length;
            Debug.Print(fileSize.ToString());
        }
    }
}
