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
    class AlgorithmTrainer
    {
        private enum DisplayOptions { none, everySegment, segmentsAndMatch, wordSegmentsAndMatch };

        public static event DisplaySubSegmentHandler DisplaySeg;
        public static event DisplayResultHandler DisplayResult;

        public void DispaySegEvent(DisplaySegEventArgs e)
        {
            DisplaySeg(new object(), e);
        }
        public void DisplayMatchEvent(DisplayMatchResultArgs e)
        {
            DisplayResult(new object(), e);
        }

        public AlgorithmTrainer()
        {
            //Define how the segmentation should be displayed for testing:
            DisplayOptions testDisplayOptions = DisplayOptions.everySegment;

            //Generate a document image:
            //ImageAndSegmentLocations generateTrainingSeg = new ImageAndSegmentLocations();  
            //int[][] uploadedDocument = GraphicsHelper.BitmapToDoubleArray(generateTrainingSeg.TrainingImage);
            
            Bitmap FileBitmap = Bitmap.FromFile("letterByLetter.bmp") as Bitmap;
            int[][] uploadedDocument = GraphicsHelper.BitmapToDoubleArray(FileBitmap);
 
            Segmentator segmentation = new Segmentator(uploadedDocument);
            OCRHandler ocrHandler = new OCRHandler();
            foreach (OCRSegment segment in segmentation.DefineSegments())
            {

                /*
                if (testDisplayOptions == DisplayOptions.everySegment)
                {
                    Bitmap bitmapSeg = DisplayUtility.ConvertDoubleArrayToBitmap(segment.InternalPoints, Color.White);
                    DisplaySegEventArgs eventArgs = new DisplaySegEventArgs(bitmapSeg, segment.SegmentLocation);
                    DispaySegEvent(eventArgs);
                }

                Tuple<string, double> labelToTrainWith = generateTrainingSeg.LabelAtThisSegmentLocation(segment.SegmentLocation, segment.ThisSegmentIsAWord);
                if (labelToTrainWith != null)
                {
                    if ((segment.ThisSegmentIsAWord  && testDisplayOptions == DisplayOptions.wordSegmentsAndMatch)
                                    || testDisplayOptions == DisplayOptions.segmentsAndMatch || testDisplayOptions == DisplayOptions.everySegment)
                    {
                        DisplayMatchResultArgs matchArgs = new DisplayMatchResultArgs(labelToTrainWith.Item1, segment.SegmentLocation, labelToTrainWith.Item2);
                        DisplayMatchEvent(matchArgs);
                    }                    
                    //ocrHandler.TrainDoubleArray(segment.InternalPoints, labelToTrainWith.Item1);
                }
                */
            }
            ocrHandler.SaveTrainingData();
        }
    }
}
