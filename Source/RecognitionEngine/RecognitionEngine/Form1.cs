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

namespace Prax.Recognition {
	public partial class Form1 : Form {
		public Form1() {
			InitializeComponent();
		}
		private UploadDocument uploadDocument = null;

		private void uploadFile(object sender, EventArgs e) {
			upload.Enabled = false;
			uploadDocument = new UploadDocument();
			upload.Enabled = true;
		}

		private enum segmentsToDisplay { segmentation, trainingSegments }

		private void trainAlgorithm(object sender, EventArgs e) {
			segmentsToDisplay displayOptions = segmentsToDisplay.trainingSegments;
			train.Enabled = false;
			if (displayOptions == segmentsToDisplay.trainingSegments)
				AlgorithmTrainer.DisplaySegment += ShowSeg;
			if (displayOptions == segmentsToDisplay.segmentation)
				SegmentatorV2.DisplaySegment += ShowSeg;
			AlgorithmTrainer.DisplayResult += ShowResult;
			AlgorithmTrainer trainHandler = new AlgorithmTrainer();
			train.Enabled = true;
		}

		private void readDocument(object sender, EventArgs e) {
			read.Enabled = false;
			SegmentatorV2.DisplaySegment += ShowSeg;
			DocumentReader readHandler = new DocumentReader(uploadDocument.uploadedDocument);
			//DocumentReader readHandler = new DocumentReader();
			read.Enabled = true;
		}

		private void clearTrainingData(object sender, EventArgs e) {
			File.Delete("TrainingData.dat");
		}

		private int yValueIndex = 0;

		Bitmap segmentsToSave = new Bitmap(1, 1);
		int counter = 1;
		private void SaveSegmentsToFile(Bitmap seg) {
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

		private void ShowSeg(object o, DisplaySegEventArgs e) {
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
			//SaveSegmentsToFile(e.BitmapToDisplay);
		}

		private void ShowResult(object o, DisplayMatchResultArgs e) {
			Label lbl = new Label();
			lbl.AutoSize = true;
			lbl.Text = e.MatchingString + Environment.NewLine + e.MatchCertainty.ToString();
			lbl.Location = new Point(1, yValueIndex);
			yValueIndex += lbl.Size.Height + 6;
			segView.Controls.Add(lbl);
		}
	}

	public class DisplaySegEventArgs : EventArgs {
		public readonly Bitmap BitmapToDisplay;
		public readonly Rectangle Location;

		public DisplaySegEventArgs(Bitmap bitmap, Rectangle location) {
			BitmapToDisplay = bitmap;
			Location = location;
		}
	}

	public class DisplayMatchResultArgs : EventArgs {
		public readonly string MatchingString;
		public readonly Rectangle MatchingCoordinates;
		public readonly double MatchCertainty;

		public DisplayMatchResultArgs(string match, Rectangle rect, double certainty) {
			MatchingString = match;
			MatchingCoordinates = rect;
			MatchCertainty = certainty;
		}
	}
}