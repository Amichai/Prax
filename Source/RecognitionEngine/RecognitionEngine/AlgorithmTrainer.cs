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
            DisplayOptions testDisplayOptions = DisplayOptions.none;

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

                Tuple<string, double> labelToTrainWith = generateTrainingSeg.LabelAtThisSegmentLocation(segment.SegmentLocation);
                if (labelToTrainWith != null)
                {
                    if ((segment.ThisSegmentIsAWord == true && testDisplayOptions == DisplayOptions.wordSegmentsAndMatch)
                                    || testDisplayOptions == DisplayOptions.segmentsAndMatch)
                    {
                        temp = new DisplayUtility.NewFormForDisplay(segment.InternalPoints, labelToTrainWith.Item1, labelToTrainWith.Item2.ToString());
                    }
                    ocrHandler.TrainDoubleArray(segment.InternalPoints, labelToTrainWith.Item1);
                }
            }
            ocrHandler.SaveTrainingData();
        }
    }
}
