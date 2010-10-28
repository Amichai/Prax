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
        public AlgorithmTrainer()
        {
            ImageAndSegmentLocations generateTrainingSeg = new ImageAndSegmentLocations();  //A document of text is generted for training purposes
            int[][] uploadedDocument = GraphicsHelper.BitmapToDoubleArray(generateTrainingSeg.TrainingImage);

            Segmentator segmentation = new Segmentator(uploadedDocument);
            OCRHandler ocrHandler = new OCRHandler();
            //foreach (OCRSegment segment in segmentation.DefineSegments())
            {
                //DisplayUtility.NewFormForDisplay temp = new DisplayUtility.NewFormForDisplay(segment.InternalPoints);
                //string labelToTrainWith = generateTrainingSeg.LabelAtThisSegmentLocation(segment.SegmentLocation);
                //if (labelToTrainWith != null)
                {
                    DisplayUtility.NewFormForDisplay temp2;
                   // if(segment.ThisSegmentIsAWord == true)
                     //   temp2 = new DisplayUtility.NewFormForDisplay(segment.InternalPoints, labelToTrainWith);
                    //ocrHandler.TrainDoubleArray(segment.InternalPoints, labelToTrainWith);
                }
            }
            ocrHandler.SaveTrainingData();
        }
    }
}
