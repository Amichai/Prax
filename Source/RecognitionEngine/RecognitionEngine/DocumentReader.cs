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
        private enum ReaderOptions { readDocument, readSavedOutput }

        public DocumentReader(int[][] uploadedDocument)
        {
            ReaderOptions readerOptions = ReaderOptions.readDocument;
            SegmentAnalysis segmentAnalysis = new SegmentAnalysis();
            if (readerOptions == ReaderOptions.readDocument) {
                SegmentatorV2 segmentation = new SegmentatorV2(uploadedDocument);

                foreach (OCRSegment segment in segmentation.DefineSegments()) {
                    segmentAnalysis.ProcessAndReadSegment(segment);
                }
            } if(readerOptions == ReaderOptions.readSavedOutput) {
                segmentAnalysis.resolvedSegmentsList = SaveAndOpenUtility.OpenRecognizedSegments().ToList();
            }

            segmentAnalysis.PrintOutput();
        }
    }
}