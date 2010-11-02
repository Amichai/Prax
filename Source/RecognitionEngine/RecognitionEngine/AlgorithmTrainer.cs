using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Prax.Recognition
{
    class AlgorithmTrainer
    {
        private enum DisplayOptions { none, everySegment, segmentsAndMatch, wordSegmentsAndMatch };

        public AlgorithmTrainer()
        {
            //Define how the segmentation should be displayed for testing:
            DisplayOptions testDisplayOptions = DisplayOptions.segmentsAndMatch;

            DisplayUtility.NewFormForDisplay temp;
            //Generate a document image:
            ImageAndSegmentLocations generateTrainingSeg = new ImageAndSegmentLocations();  
            int[][] uploadedDocument = GraphicsHelper.BitmapToDoubleArray(generateTrainingSeg.TrainingImage);
            
            Segmentator segmentation = new Segmentator(uploadedDocument);
            OCRHandler ocrHandler = new OCRHandler();
            foreach (OCRSegment segment in segmentation.DefineSegments())
            {
                if (testDisplayOptions == DisplayOptions.everySegment)
                    temp = new DisplayUtility.NewFormForDisplay(segment.InternalPoints);

                string labelToTrainWith = generateTrainingSeg.LabelAtThisSegmentLocation(segment.SegmentLocation);
                if (labelToTrainWith != null)
                {
                    if ((segment.ThisSegmentIsAWord == true && testDisplayOptions == DisplayOptions.wordSegmentsAndMatch)
                                    || testDisplayOptions == DisplayOptions.segmentsAndMatch)
                    {
                        //Debug.Print("xDiscrep: " + labelToTrainWith.XDiscrep + " yDiscrep: " + labelToTrainWith.YDiscrep + " overlap: " + labelToTrainWith.OverlapRatio);
                        Debug.Print("X: " + segment.SegmentLocation.X + " Y: " + segment.SegmentLocation.Y + " width: " + segment.SegmentLocation.Width);
                        temp = new DisplayUtility.NewFormForDisplay(segment.InternalPoints, labelToTrainWith);
                    }
                    //ocrHandler.TrainDoubleArray(segment.InternalPoints, labelToTrainWith);
                }
            }
            ocrHandler.SaveTrainingData();
        }
    }
}
