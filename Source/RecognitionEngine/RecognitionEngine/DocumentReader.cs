using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.ObjectModel;
using System.IO;

namespace Prax.Recognition
{
    class DocumentReader
    {
        public DocumentReader(int[][] uploadedDocument)
        {
            SegmentAnalysis segmentAnalysis = new SegmentAnalysis();
            Segmentator segmentation = new Segmentator(uploadedDocument);
            
            foreach (OCRSegment segment in segmentation.DefineSegments())
            {
                segmentAnalysis.ProcessAndReadSegment(segment);
            }

            segmentAnalysis.PrintOutput();
        }
    }
}