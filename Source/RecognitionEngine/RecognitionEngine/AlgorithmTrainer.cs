using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Prax.Recognition
{
    public enum TrainingDataOptions { open, reset };

    class AlgorithmTrainer
    {
        private enum DisplayOptions { none, everySegment, segmentsAndMatch, wordSegmentsAndMatch };

        public static event DisplaySubSegmentHandler DisplaySeg;
        public static event DisplayResultHandler DisplayResult;

        public void DispaySegEvent(DisplaySegEventArgs e){
            if(DisplaySeg != null)
                DisplaySeg(new object(), e);
        }
        
        public void DisplayMatchEvent(DisplayMatchResultArgs e){
            if(DisplayResult != null)
                DisplayResult(new object(), e);
        }

        public AlgorithmTrainer(){
            DisplayOptions testDisplayOptions = DisplayOptions.segmentsAndMatch;
            TrainingDataOptions openOptions = TrainingDataOptions.reset;

            int[][] uploadedDocument = null;

            ImageAndSegmentLocations generateTrainingSeg = new ImageAndSegmentLocations();
            uploadedDocument = GraphicsHelper.BitmapToDoubleArray(generateTrainingSeg.TrainingImage);

            SegmentatorV2 segmentation = new SegmentatorV2(uploadedDocument);
            OCRHandler ocrHandler = new OCRHandler(openOptions);
            foreach (OCRSegment segment in segmentation.DefineSegments()){
                
                if (testDisplayOptions == DisplayOptions.everySegment){
                    Bitmap bitmapSeg = DisplayUtility.ConvertDoubleArrayToBitmap(segment.InternalPoints, Color.White);
                    DisplaySegEventArgs eventArgs = new DisplaySegEventArgs(bitmapSeg, segment.SegmentLocation);
                    DispaySegEvent(eventArgs);
                }

                Tuple<string, double> labelToTrainWith = generateTrainingSeg.LabelAtThisSegmentLocation(segment.SegmentLocation, segment.IsAWord);
                if (labelToTrainWith != null) {
                    if ((segment.IsAWord && testDisplayOptions == DisplayOptions.wordSegmentsAndMatch)
                                    || testDisplayOptions == DisplayOptions.segmentsAndMatch || testDisplayOptions == DisplayOptions.everySegment) {
                        DisplayMatchResultArgs matchArgs = new DisplayMatchResultArgs(labelToTrainWith.Item1, segment.SegmentLocation, labelToTrainWith.Item2);
                        DisplayMatchEvent(matchArgs);
                        Bitmap bitmapSeg = DisplayUtility.ConvertDoubleArrayToBitmap(segment.InternalPoints, Color.White);
                        DisplaySegEventArgs eventArgs = new DisplaySegEventArgs(bitmapSeg, segment.SegmentLocation);
                        DispaySegEvent(eventArgs);
                    }
                    ocrHandler.TrainDoubleArray(segment.InternalPoints, labelToTrainWith.Item1);
                } else {
                    //DisplayUtility.NewFormForDisplay(segment.InternalPoints);
                }
            }
            ocrHandler.SaveTrainingData();
        }
    }
}
