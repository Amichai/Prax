using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Prax.OcrEngine.Services;
using System.Drawing;
using System.Diagnostics;

namespace Prax.Recognition {
    class SegmentAnalysis {
        public List<RecognizedSegment> resolvedSegmentsList = new List<RecognizedSegment>();
        OCRHandler wordOCR = new OCRHandler(TrainingDataOptions.open);
        List<RecognizedSegment> lettersResolvedFromWord = new List<RecognizedSegment>();

        private const int thresholdCertainty = 500;
        int didntReachCertaintyThreshold = 0;
        public void ProcessAndReadSegment(OCRSegment segment) {
            RecognizedSegment recognizedWord = new RecognizedSegment();
            recognizedWord = readSegment(segment);

            if (recognizedWord.Certainty > thresholdCertainty) {
                resolvedSegmentsList.Add(recognizedWord);
            } else {
                Debug.Print((didntReachCertaintyThreshold++).ToString() + " didn't reach certainty threshold");
                Debug.Print(recognizedWord.Certainty.ToString());
            }
        }

        private void assessTheLettersResolvedFromWord() {
            int currentIndex = 0;
            List<Tuple<int, int>> overlap = new List<Tuple<int, int>>();
            int counter = 0;
            foreach (RecognizedSegment seg in resolvedSegmentsList) {
                if (seg.Certainty > thresholdCertainty) {
                    if (seg.Bounds.X < currentIndex)
                        overlap.Add(new Tuple<int, int>(counter, currentIndex - seg.Bounds.X));
                    counter++;
                    currentIndex = seg.Bounds.Right;
                }
            }
            for (int i = 0; i < overlap.Count(); i++) {
                double overlapRating1 = overlap[i].Item2 / resolvedSegmentsList[overlap[i].Item1].Bounds.Width;
                double overlapRating2 = overlap[i].Item2 / resolvedSegmentsList[overlap[i].Item1 - 1].Bounds.Width;
                //If the overlap is equal or combined overlap is very small just neglect it
                if (overlapRating1 > overlapRating2)
                    resolvedSegmentsList.RemoveAt(overlap[i].Item1);
                else
                    resolvedSegmentsList.RemoveAt(overlap[i].Item1 - 1);
            }
        }

        private HashSet<int> determineIndiciesToSearchFor() {
            List<string> wordsToCheck = new List<string>();
            string wordPattern = string.Empty;
            RecognizedSegment lastSegmentSeen = new RecognizedSegment();
            int distanceThreshold = 5; //TODO: ensure that no letters will fit within the threshold (.*)

            foreach (RecognizedSegment seg in lettersResolvedFromWord) {
                wordPattern += Regex.Escape(seg.Text);
                if (lastSegmentSeen.Bounds != null && seg.Bounds.X - (lastSegmentSeen.Bounds.X + lastSegmentSeen.Bounds.Width) > distanceThreshold)
                    wordPattern += ".+";
                lastSegmentSeen = seg;
            }
            Regex regex = new Regex(wordPattern);
            return new HashSet<int>(wordOCR.listOfIndexLabels
                .Select((s, index) => new { Word = s, Index = index })
                .Where(o => regex.IsMatch(o.Word))
                .Select(o => o.Index)
            );
        }

        private RecognizedSegment readSegment(OCRSegment segment) {
            Tuple<string, double> labelAndCertainty;
            labelAndCertainty = wordOCR.ReadDoubleArray(segment.InternalPoints);

            if (labelAndCertainty != null)
                return new RecognizedSegment(segment.SegmentLocation,
                                                labelAndCertainty.Item1,
                                                labelAndCertainty.Item2);
            else return new RecognizedSegment();
        }

        public void PrintOutput() {
            ReadOnlyCollection<RecognizedSegment> readOnlyResults = resolvedSegmentsList.AsReadOnly();
            OutputRenderer outputRenderer = new OutputRenderer();
            outputRenderer.Convert(null, readOnlyResults);
        }
    }

    [Serializable]
    public struct RecognizedSegment {
        public RecognizedSegment(Rectangle bounds, string text, double certainty)
            : this() {
            Bounds = bounds;
            Text = text;
            Certainty = certainty;
        }

        ///<summary>Gets the area in the image that contains the string.</summary>
        public Rectangle Bounds { get; private set; }
        ///<summary>Gets the recognized text.</summary>
        public string Text { get; private set; }
        ///<summary>Gets the certainty of the recognition, between 0 and 1.</summary>
        public double Certainty { get; private set; }

    }
}
