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
        private enum ReaderOptions { readDocument, readSavedOutput, saveOutput }

        public DocumentReader(int[][] uploadedDocument)
        {
            ReaderOptions readerOptions = ReaderOptions.readDocument;
            DisplayUtility.NewFormForDisplay(uploadedDocument);
            SegmentAnalysis segmentAnalysis = new SegmentAnalysis();
            if (readerOptions == ReaderOptions.readDocument || readerOptions == ReaderOptions.saveOutput) {
                SegmentatorV2 segmentation = new SegmentatorV2(uploadedDocument);

                foreach (OCRSegment segment in segmentation.DefineSegments()) {
                    segmentAnalysis.ProcessAndReadSegment(segment);
                    //DisplayUtility.NewFormForDisplay(segment.InternalPoints);
                }
            } if(readerOptions == ReaderOptions.readSavedOutput) {
                segmentAnalysis.resolvedSegmentsList = SaveAndOpenUtility.OpenRecognizedSegments().ToList();
            }
            if (readerOptions == ReaderOptions.saveOutput) {
                SaveAndOpenUtility.SaveRecognizedSegments(segmentAnalysis.resolvedSegmentsList.AsReadOnly());
            }

            segmentAnalysis.PrintOutput();
        }
    }
}