using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace Prax.Recognition
{
    class SegmentAnalysis
    {
        List<RecognizedSegment> resolvedSegmentsList = new List<RecognizedSegment>();  
        OCRHandler wordOCR = new OCRHandler();
        List<RecognizedSegment> lettersResolvedFromWord = new List<RecognizedSegment>();

        private const int thresholdCertainty = 6000;

        public void ProcessAndReadSegment(OCRSegment segment)
        {
            if (segment.ThisSegmentIsAWord == true)
            {
                assessTheLettersResolvedFromWord();  //Handle border ambiguities between resolved letters
                RecognizedSegment recognizedWord = new RecognizedSegment();
                recognizedWord = readSegment(segment);
                if (recognizedWord.Certainty > thresholdCertainty)
                {
                    resolvedSegmentsList.Add(recognizedWord);
                }
            }
            else
            {
                RecognizedSegment recognizedLetter = new RecognizedSegment();
                recognizedLetter = readSegment(segment);
                if(recognizedLetter.Certainty > thresholdCertainty)
                {
                    //if (lettersResolvedFromWord.Count > 0 && recognizedLetter.Bounds.X - lettersResolvedFromWord[lettersResolvedFromWord.Count - 1].Bounds.X > thresholdDistance)
                      //  lettersResolvedFromWord.Add(new RecognizedSegment());
                    
                    lettersResolvedFromWord.Add(recognizedLetter);
                }
            }
        }

        private void assessTheLettersResolvedFromWord() 
        {
            int currentIndex = 0;
            List<Tuple<int, int>> overlap = new List<Tuple<int, int>>();
            int counter = 0;
            foreach(RecognizedSegment seg in resolvedSegmentsList)
            {
                if (seg.Certainty > thresholdCertainty)
                {
                    if (seg.Bounds.X < currentIndex)
                        overlap.Add(new Tuple<int, int>(counter, currentIndex - seg.Bounds.X));
                    counter++;
                    currentIndex = seg.Bounds.X + seg.Bounds.Width;
                }
            }
            for (int i = 0; i < overlap.Count(); i++)
            {
                double overlapRating1 = overlap[i].Item2 / resolvedSegmentsList[overlap[i].Item1].Bounds.Width;
                double overlapRating2 = overlap[i].Item2 / resolvedSegmentsList[overlap[i].Item1 - 1].Bounds.Width;
                //If the overlap is equal or combined overlap is very small just neglect it
                if (overlapRating1 > overlapRating2)
                    resolvedSegmentsList.RemoveAt(overlap[i].Item1);
                else
                    resolvedSegmentsList.RemoveAt(overlap[i].Item1 - 1);
            }
        }

        private HashSet<int> determineIndiciesToSearchFor()
        {
            List<string> wordsToCheck = new List<string>();
            //USE: wordOCR.listOfIndexLabels, lettersResolvedFromWord 
            string wordPattern = string.Empty;
            RecognizedSegment lastSegmentSeen = new RecognizedSegment();
            int distanceThreshold = 5;
            foreach (RecognizedSegment seg in lettersResolvedFromWord)
            {
                wordPattern += seg.Text;
                if(lastSegmentSeen.Bounds != null && seg.Bounds.X - (lastSegmentSeen.Bounds.X + lastSegmentSeen.Bounds.Width) > distanceThreshold)
                    wordPattern += ".*";
                lastSegmentSeen = seg;
            }
            wordPattern += ".*";

            string regexPattern = "^" + wordPattern + "$";
            int index = 0;
            HashSet<int> indices = new HashSet<int>();
            foreach (string s in wordOCR.listOfIndexLabels)
            {
                if (Regex.IsMatch(s, regexPattern))
                {
                    indices.Add(index);
                }
                index++;
            }
            return indices;
        }

        private HashSet<int> indiciesOfLettersOnly = new HashSet<int>();

        private RecognizedSegment readSegment(OCRSegment segment)
        {
            Tuple<string, double> labelAndCertainty;
            labelAndCertainty = wordOCR.ReadDoubleArray(segment.InternalPoints);
            
            if (!segment.ThisSegmentIsAWord)
                wordOCR.IndiciesToCheck = indiciesOfLettersOnly;
            else
                wordOCR.IndiciesToCheck = determineIndiciesToSearchFor();

            return new RecognizedSegment(segment.SegmentLocation,
                                            labelAndCertainty.Item1,
                                            labelAndCertainty.Item2);
        }

        public void PrintOutput()
        {
            ReadOnlyCollection<RecognizedSegment> readOnlyResults = resolvedSegmentsList.AsReadOnly();
            OutputRenderer outputRenderer = new OutputRenderer();
            FileStream outputFile = (FileStream)outputRenderer.RenderFile(readOnlyResults);
            outputFile.Close();
        }
    }
}
