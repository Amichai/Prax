using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Prax.Recognition
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private UploadDocument uploadDocument = null;

        private void uploadFile(object sender, EventArgs e)
        {
            upload.Enabled = false;
            DisplayUtility.DefaultPictureBox = pictureBox;
            uploadDocument = new UploadDocument(pictureBox);
            upload.Enabled = true;
        }

        private void trainAlgorithm(object sender, EventArgs e)
        {
            train.Enabled = false;
            DisplayUtility.DefaultPictureBox = pictureBox;
            //AlgorithmTrainer.DisplaySeg += new DisplaySubSegmentHandler(ShowSeg);
            SegmentatorV2.DisplaySeg += new DisplaySubSegmentHandler(ShowSeg);
            AlgorithmTrainer.DisplayResult += new DisplayResultHandler(ShowResult);
            AlgorithmTrainer trainHandler = new AlgorithmTrainer();
            train.Enabled = true;
        }

        private void readDocument(object sender, EventArgs e)
        {
            read.Enabled = false;
            DocumentReader readHandler = new DocumentReader(uploadDocument.uploadedDocument);
            read.Enabled = true;
        }

        private void clearTrainingData(object sender, EventArgs e)
        {
            File.Delete("TrainingData.dat");
        }

        private int yValueIndex = 0;

        private void ShowSeg(object o, DisplaySegEventArgs e)
        {
            yValueIndex += 3;
            PictureBox seg = new PictureBox();
            seg.Size = new Size(e.BitmapToDisplay.Width, e.BitmapToDisplay.Height);
            seg.Location = new Point(1, yValueIndex);
            yValueIndex += e.BitmapToDisplay.Height + 3;
            seg.BackgroundImage = e.BitmapToDisplay;
            segView.Controls.Add(seg);
            Label lbl = new Label();
            lbl.AutoSize = true;
            lbl.Text = e.Location.ToString();
            lbl.Location = new Point(1, yValueIndex);
            yValueIndex += lbl.Size.Height;
            segView.Controls.Add(lbl);
            yValueIndex -= 10;
        }

        private void ShowResult(object o, DisplayMatchResultArgs e)
        {
            Label lbl = new Label();
            lbl.AutoSize = true;
            lbl.Text = e.MatchingString + Environment.NewLine + e.MatchCertainty.ToString();
            lbl.Location = new Point(1, yValueIndex);
            yValueIndex += lbl.Size.Height + 6;
            segView.Controls.Add(lbl);
        }
    }

    public delegate void DisplaySubSegmentHandler(object o, DisplaySegEventArgs e);
    public delegate void DisplayResultHandler(object o, DisplayMatchResultArgs e);

    public class DisplaySegEventArgs : EventArgs
    {
        public readonly Bitmap BitmapToDisplay;
        public readonly Rectangle Location;

        public DisplaySegEventArgs(Bitmap bitmap, Rectangle location)
        {
            BitmapToDisplay = bitmap;
            Location = location;
        }
    }

    public class DisplayMatchResultArgs : EventArgs
    {
        public readonly string MatchingString;
        public readonly Rectangle MatchingCoordinates;
        public readonly double MatchCertainty;

        public DisplayMatchResultArgs(string match, Rectangle rect, double certainty)
        {
            MatchingString = match;
            MatchingCoordinates = rect;
            MatchCertainty = certainty;
        }

    }
}