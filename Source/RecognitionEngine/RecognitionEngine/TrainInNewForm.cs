using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Prax.Recognition{
    class TrainInNewForm {
        public Form tempDisplay;
        public Label enterTheLabel;
        public Label segmentLabel;
        string labelToReturn = string.Empty;
        public void skip_click(object sender, EventArgs e) {
            tempDisplay.Close();
            tempDisplay.Dispose();
        }
        public void submitLabel_click(object sender, EventArgs e) {
            train = true;
            tempDisplay.Close();
            tempDisplay.Dispose();
        }

        public void close_click(object sender, EventArgs e) {
            train = false;
            tempDisplay.Close();
            tempDisplay.Dispose();
        }
        bool train = false;
        private void trainingForm(int width, int height, Bitmap bitmapToRender, Form tempDisplay) {
            tempDisplay.SetBounds(10, 10, width + 360, height);
            tempDisplay.BackgroundImage = bitmapToRender;
            tempDisplay.BackgroundImageLayout = ImageLayout.Center;
            enterTheLabel.SetBounds(0, 0, 900, 30);
            segmentLabel.SetBounds(0, 30, 20, 20);

            tempDisplay.StartPosition = System.Windows.Forms.FormStartPosition.Manual;

            Button submitLabel = new Button();
            submitLabel.Text = "Submit";
            submitLabel.SetBounds(2, 60, 60, 20);

            Button skip = new Button();
            skip.Text = "Skip";
            skip.SetBounds(64, 60, 50, 20);
            tempDisplay.Controls.Add(skip);

            tempDisplay.Controls.Add(segmentLabel);
            tempDisplay.Controls.Add(enterTheLabel);
            tempDisplay.Controls.Add(submitLabel);

            Point temp = new Point(3, 4);
            tempDisplay.Location = new Point(100, 100);

            submitLabel.Click += new System.EventHandler(submitLabel_click);
            skip.Click += new System.EventHandler(skip_click);
            tempDisplay.ShowDialog();
        }

        public bool TrainInForm(int[][] internalPoints, string dat1, string dat2, string dat3, string dat4, string text) {
            enterTheLabel = new Label();
            segmentLabel = new Label();
            Bitmap bitmapToRender = DisplayUtility.ConvertDoubleArrayToBitmap(internalPoints, Color.White);
            enterTheLabel.Text = "Segment Location: " + dat1 + "\nRendered Letter: " + dat2;// +" " + dat3 + " " + dat4;
            segmentLabel.Text = text;
            tempDisplay = new Form();
            trainingForm(internalPoints.GetLength(0) + 200, internalPoints[0].GetLength(0) + 100, bitmapToRender, tempDisplay);
            return train;
        }
    }
}
