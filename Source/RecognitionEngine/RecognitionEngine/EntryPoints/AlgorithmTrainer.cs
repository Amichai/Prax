﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Prax.Recognition {
	public enum TrainingDataOptions { openAndAddTo, reset };

	class AlgorithmTrainer {
		private enum DisplayOptions { none, everySegment, segmentsAndMatch, wordSegmentsAndMatch };
		private enum TrainFunction { trainToo, justRenderImage }

		public static event EventHandler<DisplaySegEventArgs> DisplaySegment;
		public static event EventHandler<DisplayMatchResultArgs> DisplayResult;

		void OnDisplaySegment(DisplaySegEventArgs e) {
			var copy = DisplaySegment;
			if (copy != null) copy(this, e);
		}
		void OnDisplayMatch(DisplayMatchResultArgs e) {
			var copy = DisplayResult;
			if (copy != null) copy(this, e);
		}

		private enum ImageToSegment { fromRendering, fromScreenShot };

		public AlgorithmTrainer() {
			DisplayOptions testDisplayOptions = DisplayOptions.segmentsAndMatch;
			TrainingDataOptions openOptions = TrainingDataOptions.openAndAddTo;
			TrainFunction trainFunction = TrainFunction.justRenderImage;
			ImageToSegment imageToSegment = ImageToSegment.fromRendering;

			int[][] uploadedDocument = null;

			ImageAndSegmentLocations generateTrainingSeg = new ImageAndSegmentLocations();

			string fileName = @"C:\Users\Amichai\Pictures\Handwritten\letterByLetter (2).bmp";
			if (imageToSegment == ImageToSegment.fromScreenShot) {
				Bitmap FileBitmap = Bitmap.FromFile(fileName) as Bitmap;
				uploadedDocument = GraphicsHelper.BitmapToDoubleArray(FileBitmap);
			}
			if (imageToSegment == ImageToSegment.fromRendering) {
				uploadedDocument = GraphicsHelper.BitmapToDoubleArray(generateTrainingSeg.TrainingImage);
			}

			SegmentatorV2 segmentation = new SegmentatorV2(uploadedDocument);
			OCRHandler ocrHandler = new OCRHandler(openOptions);
			foreach (OCRSegment segment in segmentation.DefineSegments()) {

				if (testDisplayOptions == DisplayOptions.everySegment) {
					Bitmap bitmapSeg = DisplayUtility.ConvertDoubleArrayToBitmap(segment.InternalPoints, Color.White);
					OnDisplaySegment(new DisplaySegEventArgs(bitmapSeg, segment.SegmentLocation));
				}

				Tuple<string, double> labelToTrainWith = null;
				if(trainFunction == TrainFunction.trainToo)
					labelToTrainWith = generateTrainingSeg.LabelAtThisSegmentLocation(segment.SegmentLocation, segment.IsAWord, segment.InternalPoints);
				if (labelToTrainWith != null ) {
					Debug.Print(labelToTrainWith.Item1 + " " + labelToTrainWith.Item2.ToString());
					//DisplayUtility.NewFormForDisplay(segment.InternalPoints);
					if (testDisplayOptions != DisplayOptions.none && (segment.IsAWord || testDisplayOptions != DisplayOptions.wordSegmentsAndMatch)) {
						OnDisplayMatch(new DisplayMatchResultArgs(labelToTrainWith.Item1, segment.SegmentLocation, labelToTrainWith.Item2));

						Bitmap bitmapSeg = DisplayUtility.ConvertDoubleArrayToBitmap(segment.InternalPoints, Color.White);
						OnDisplaySegment(new DisplaySegEventArgs(bitmapSeg, segment.SegmentLocation));
					}
					ocrHandler.TrainDoubleArray(segment.InternalPoints, labelToTrainWith.Item1);
				}
			}
			ocrHandler.SaveTrainingData();
		}
	}
}