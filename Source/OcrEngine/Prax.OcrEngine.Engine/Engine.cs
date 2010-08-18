using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using Prax.OcrEngine;

namespace Prax.OcrEngine.Engine
{
    public struct ImageSegmentElement
    {
        public Rectangle rect;
        public string label;
    }

    public class TestAlgorithm 
    {
        public void Test(int imageWidth, int imageHeight, int[][] arrayBitmap)
        {
            int pixelsToSkip = 5;
            initializeLabeledPixelBitmap(imageWidth, imageHeight);
            if (TrainingData.TrainingDataIndex.Count > 0) //make sure their is training data to test against
            {
                IterateInput UploadedFile = new IterateInput(imageWidth, imageHeight, arrayBitmap);
                HeuristicArray currentHArray = new HeuristicArray();

                for (int i = 0; i < imageWidth; i += pixelsToSkip)
                {
                    for (int j = 0; j < imageHeight; j += pixelsToSkip)
                    {
                        currentHArray.DefineHeuristicArray(i, j, UploadedFile.SetOfIteratedBoards, imageWidth, imageHeight);
                        testArrayAgainstTrainingData(i, j, currentHArray.currentHeuristicArray);
                    }
                }
                printResultToFile(imageWidth, imageHeight, pixelsToSkip);
            }
            //Iterate through the entire bitmap (with a skip function)
            //For each pixel define the heuristic array
            //Test each heuristic array against the training Data
        }

        public List<string>[][] labeledPixelBitmap;
        static public int counter = 0;
        public void initializeLabeledPixelBitmap(int width, int height)
        {
            labeledPixelBitmap = new List<string>[width][];
            for (int i = 0; i < width; i++)
            {
                labeledPixelBitmap[i] = new List<string>[height];
                for (int j = 0; j < height; j++)
                {
                    labeledPixelBitmap[i][j] = new List<string>();
                }
            }
        }

        static public int[][] labeledSegmentBitmap_test;
        private void testArrayAgainstTrainingData(int xCoordinate, int yCoordinate, int[] heuristicArray)
        {
            int numberOfUniqueLabels = TrainingData.TrainingDataIndex.Count();
            double[][] probabilityFromEachHeuristic = new double[numberOfUniqueLabels][];
            double[][] lblComparisonResults = new double[numberOfUniqueLabels][];

            double[] labelProbability;

            double[] totalComparison_test = new double[numberOfUniqueLabels];
            //Initialize the labelProbability array. One probability per label.
            for (int i = 0; i < numberOfUniqueLabels; i++)
            {
                probabilityFromEachHeuristic[i] = new double[HeuristicArray.numberOfHeuristics];
                lblComparisonResults[i] = new double[HeuristicArray.numberOfHeuristics];
            }
            Debug.Print(labeledSegmentBitmap_test[xCoordinate][yCoordinate].ToString());

            for (int heurIdx = 0; heurIdx < HeuristicArray.numberOfHeuristics; heurIdx++)
            {
                double labelElementsCompared = 0;
                for (int inspectionLbl = 0; inspectionLbl < numberOfUniqueLabels; inspectionLbl++) //This is the label we are trying to get information about. We want to know how feasible this label is given our current array
                {
                    for (int lblTrialIdx = 0; lblTrialIdx < TrainingData.TrainingDataIndex[inspectionLbl].indexLocations.Count(); lblTrialIdx++)
                    { //Iterate through every heuristic array within our given label
                        if (lblTrialIdx < TrainingData.TrainingDataIndex[inspectionLbl].indexLocations.Count() - TrainingData.TrainingDataIndex[inspectionLbl].indexLocations.Count() / 70)
                            lblTrialIdx += TrainingData.TrainingDataIndex[inspectionLbl].indexLocations.Count() / 70;
                        if (TrainingData.TrainingDataLibrary[TrainingData.TrainingDataIndex[inspectionLbl].indexLocations[lblTrialIdx]].heuristicArray[heurIdx] == heuristicArray[heurIdx] && heuristicArray[heurIdx] != 0 && heuristicArray[heurIdx] != -1)
                        {
                            lblComparisonResults[inspectionLbl][heurIdx]++;
                        }
                        labelElementsCompared++;
                    }
                    totalComparison_test[inspectionLbl] += lblComparisonResults[inspectionLbl][heurIdx];
                }
                for (int labelIndex = 0; labelIndex < numberOfUniqueLabels; labelIndex++)
                {
                    lblComparisonResults[labelIndex][heurIdx] = lblComparisonResults[labelIndex][heurIdx] / labelElementsCompared;
                    //    totalComparison_test[labelIndex] = totalComparison_test[labelIndex] / labelElementsCompared;
                }
                //This data structure represents:
            }       //What percentage of the time the current heuristics on the current heuristic array were equal to the heuristics on the training data for every given number

            double heuristicProbabilisticIndication;
            double multiplicativeOffset = .05; //make this number variable
            labelProbability = new double[numberOfUniqueLabels];
            double sumOfLabelProbabilities = 0;
            for (int inspectionLbl = 0; inspectionLbl < numberOfUniqueLabels; inspectionLbl++)
            {
                labelProbability[inspectionLbl] = 1;
                for (int heurIdx = 0; heurIdx < HeuristicArray.numberOfHeuristics; heurIdx++)
                {
                    double comparisonToThisLabel = lblComparisonResults[inspectionLbl][heurIdx];
                    double comparisonToOtherLabels = 0;
                    for (int comparisonLbl = 0; comparisonLbl < numberOfUniqueLabels; comparisonLbl++)
                    {
                        if (inspectionLbl != comparisonLbl)
                            comparisonToOtherLabels += lblComparisonResults[comparisonLbl][heurIdx];
                    }
                    //comparisonToOtherLabels = comparisonToOtherLabels / 2;
                    if (comparisonToThisLabel + comparisonToOtherLabels == 0)
                        heuristicProbabilisticIndication = .5;
                    else
                        heuristicProbabilisticIndication = comparisonToThisLabel / (comparisonToThisLabel + comparisonToOtherLabels);
                    if (heuristicProbabilisticIndication == 1 || heuristicProbabilisticIndication == 0)
                        labelProbability[inspectionLbl] *= (heuristicProbabilisticIndication + multiplicativeOffset) / (1 - heuristicProbabilisticIndication + multiplicativeOffset);
                    else
                        labelProbability[inspectionLbl] *= heuristicProbabilisticIndication / (1 - heuristicProbabilisticIndication);
                    if (double.IsInfinity(labelProbability[inspectionLbl]) || labelProbability[inspectionLbl] == 0)
                        heurIdx = HeuristicArray.numberOfHeuristics;
                }
                sumOfLabelProbabilities += labelProbability[inspectionLbl];
                Debug.Print("Label: {0} has certainity {1}", inspectionLbl, labelProbability[inspectionLbl]);
            }
            double maxValue = 0;
            int maxIndex = 0;

            for (int i = 0; i < numberOfUniqueLabels; i++)
            {
                if (labelProbability[i] / sumOfLabelProbabilities > maxValue)
                {
                    maxValue = labelProbability[i] / sumOfLabelProbabilities;
                    maxIndex = i;
                }
            }
            if (maxValue > .4)
            {
                labeledPixelBitmap[xCoordinate][yCoordinate].Add(TrainingData.TrainingDataIndex[maxIndex].dataLabel);
            }
            Debug.Print(" Max percentage: {0}", maxValue);
        }

        private void printResultToFile(int width, int height, int pxlsToSkip)
        {
            StreamWriter outputFile = new StreamWriter(@"C:\Users\Amichai\Documents\Prax\output1.txt");
            for (int i = 0; i < width; i += pxlsToSkip)
            {
                for (int j = 0; j < height; j += pxlsToSkip)
                {
                    if (labeledPixelBitmap[i][j] != null)
                    {
                        for (int k = 0; k < labeledPixelBitmap[i][j].Count; k++)
                        {
                            outputFile.Write(labeledPixelBitmap[i][j][k]);
                        }
                        outputFile.Write(" ");
                    }
                }
                outputFile.Write("\n");
            }
            outputFile.Flush();
            outputFile.Close();
        }
    }

    public class TrainAlgorithm 
    {
        public void Train(int imageWidth, int imageHeight, int[][] arrayBitmap, List<ImageSegmentElement> ImageSegmentData)
        {

            IterateInput UploadedFile = new IterateInput(imageWidth, imageHeight, arrayBitmap);
            int pixelsToSkip = 5;
            bool trainWithTheArray;
            initializeLabeledSegmentBitmap(imageWidth, imageHeight);
            for (int i = 0; i < ImageSegmentData.Count; i++)
            {
                for (int xOffset = 0; xOffset < ImageSegmentData[i].rect.Width; xOffset++)
                {
                    for (int yOffset = 0; yOffset < ImageSegmentData[i].rect.Height; yOffset++)
                    {
                        HeuristicArray currentHArray = new HeuristicArray();
                        trainWithTheArray = currentHArray.DefineHeuristicArray(ImageSegmentData[i].rect.X + xOffset, ImageSegmentData[i].rect.Y + yOffset,
                                                    UploadedFile.SetOfIteratedBoards, imageWidth, imageHeight);
                        if (trainWithTheArray)
                        {
                            currentHArray.CommitArrayToTrainingData(ImageSegmentData[i].label, ImageSegmentData[i].rect);
                            labeledSegmentBitmap[ImageSegmentData[i].rect.X + xOffset][ImageSegmentData[i].rect.Y + yOffset] = i;
                            TestAlgorithm.labeledSegmentBitmap_test[ImageSegmentData[i].rect.X + xOffset][ImageSegmentData[i].rect.Y + yOffset] = i;
                        }
                    }
                }
            }
            printLabeledSegementBitmap(imageWidth, imageHeight, pixelsToSkip);
            UploadedFile.reset();
        }
        int[][] labeledSegmentBitmap;

        private void initializeLabeledSegmentBitmap(int width, int height)
        {
            TestAlgorithm.labeledSegmentBitmap_test = new int[width][];
            labeledSegmentBitmap = new int[width][];

            for (int i = 0; i < width; i++)
            {
                TestAlgorithm.labeledSegmentBitmap_test[i] = new int[height];
                labeledSegmentBitmap[i] = new int[height];
                for (int j = 0; j < height; j++)
                {
                    TestAlgorithm.labeledSegmentBitmap_test[i][j] = -1;
                    labeledSegmentBitmap[i][j] = -1;
                }
            }
        }
        private void printLabeledSegementBitmap(int width, int height, int pxlsToSkip)
        {
            StreamWriter outputFile = new StreamWriter(@"C:\Users\Amichai\Documents\Prax\output2.txt");
            for (int i = 0; i < width; i += pxlsToSkip)
            {
                for (int j = 0; j < height; j += pxlsToSkip)
                {
                    outputFile.Write(labeledSegmentBitmap[i][j]);
                    outputFile.Write(" ");
                }
                outputFile.Write("\n");
            }
            outputFile.Flush();
            outputFile.Close();
        }
    }

    public class IterateInput
    {
        public const int NumberOfBoardIterations = 5;
        private int consolidationConstant = 3;
        private int boardIterationCounter = 0;

        public int UploadedImageHeight { get; set; }
        public int UploadedImageWidth { get; set; }
        private StreamWriter outputFile = new StreamWriter(@"C:\Users\Amichai\Documents\Prax\output.txt");

        public int[][][] SetOfIteratedBoards { get; set; }

        public IterateInput(int width, int height, int[][] arrayBitmap)
        {
            UploadedImageWidth = width;
            UploadedImageHeight = height;

            PreprocessImage(arrayBitmap); //Right now this does nothing.
            //This can be used to normalize, resize, or darken the input file data if necessary

            //Iterate the board by calling iteration functions internal to the UploadedFile class
            InitializeIteratedBoards(NumberOfBoardIterations); //currently set to 5 but probably should be higher
            IterateBoard(arrayBitmap);
            for (int i = 1; i < IterateInput.NumberOfBoardIterations; i++)
            {   //this counter starts at 1 because the board has already been iterated one time
                IterateBoard(SetOfIteratedBoards[i]);
            }

            //Print the iterated board to a file
            //This is a debugging function

            PrintBoard(SetOfIteratedBoards[0], UploadedImageWidth, UploadedImageHeight);

        }

        public void PreprocessImage(int[][] DocumentBitmap)
        {
            //do any necessary preprocessing
        }

        public void PrintBoard(int[][] board, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    outputFile.Write(board[i][j] + " ");
                } outputFile.Write("\n");
            }
            outputFile.Flush();
        }

        public void InitializeIteratedBoards(int numberOfIteratedBoards)
        {
            //initialize the iterated_boards
            SetOfIteratedBoards = new int[numberOfIteratedBoards][][];
            for (int i = 0; i < numberOfIteratedBoards; i++)
            {
                SetOfIteratedBoards[i] = new int[UploadedImageWidth][];
                for (int j = 0; j < UploadedImageWidth; j++)
                {
                    SetOfIteratedBoards[i][j] = new int[UploadedImageHeight];
                }
            }
        }

        public void IterateBoard(int[][] origionalBoard)
        {
            int averageSurroundingDiscrepPxls;
            int rangeOfSurroundingPxls, rangeOfSurroundingDiscrepPxls;
            int[] surroundingPxls = new int[4];
            int[] surroundingDiscrepancyPxls = new int[4];
            for (int i = 1; i < UploadedImageWidth - 1; i++) //The boarders can be neglected in the iterated boards because border information is preserved
            //in neighboring pixels.
            {                           //PAY CLOSE ATTENTION TO THE UNINITIALIZED BORDERS
                for (int j = 1; j < UploadedImageHeight - 1; j++)
                {
                    surroundingPxls[0] = origionalBoard[i - 1][j];
                    surroundingPxls[1] = origionalBoard[i][j - 1];
                    surroundingPxls[2] = origionalBoard[i + 1][j];
                    surroundingPxls[3] = origionalBoard[i][j + 1];
                    for (int k = 0; k < 4; k++)
                        surroundingDiscrepancyPxls[k] = Math.Abs(origionalBoard[i][j] - surroundingPxls[k]);
                    averageSurroundingDiscrepPxls = (surroundingDiscrepancyPxls[0] + surroundingDiscrepancyPxls[1]
                                            + surroundingDiscrepancyPxls[2] + surroundingDiscrepancyPxls[3]) / 4;
                    rangeOfSurroundingDiscrepPxls = Math.Max(Math.Max(surroundingDiscrepancyPxls[0], surroundingDiscrepancyPxls[1]), Math.Max(surroundingDiscrepancyPxls[2], surroundingDiscrepancyPxls[3]))
                                            - Math.Min(Math.Min(surroundingDiscrepancyPxls[0], surroundingDiscrepancyPxls[1]), Math.Min(surroundingDiscrepancyPxls[2], surroundingDiscrepancyPxls[3]));
                    rangeOfSurroundingPxls = Math.Max(Math.Max(surroundingPxls[0], surroundingPxls[1]), Math.Max(surroundingPxls[2], surroundingPxls[3]))
                                            - Math.Min(Math.Min(surroundingPxls[0], surroundingPxls[1]), Math.Min(surroundingPxls[2], surroundingPxls[3]));

                    SetOfIteratedBoards[boardIterationCounter][i][j] = ((averageSurroundingDiscrepPxls * rangeOfSurroundingPxls) / ((rangeOfSurroundingDiscrepPxls / 4) + 1)) / consolidationConstant;
                }
            }
            boardIterationCounter++;
        }

        public void reset()
        {
            outputFile.Close();
            boardIterationCounter = 0;
        }
    }

    public class HeuristicArray
    {
        const double pixelRadius = 10;
        private const int numberOfIteratedBoards = IterateInput.NumberOfBoardIterations;
        private const double pixelsWithinGivenRadius = (pixelRadius / (double)2) * (8 * pixelRadius + 8); //initialize
        public const int numberOfHeuristics = (int)pixelsWithinGivenRadius * numberOfIteratedBoards;
        public int[] currentHeuristicArray = new int[numberOfHeuristics];

        //public Rectangle sizeOfCurrentLetter { get; set; } //We may want this back some day

        private Point XYIndexOffsetPoint(int indexLocation)
        {
            int conversionBase = (int)pixelRadius * 2 + 1;
            int remainder = indexLocation % conversionBase;
            int quotient = indexLocation / conversionBase;
            Point offsetPoint = new Point(-(int)pixelRadius + remainder, -(int)pixelRadius + quotient);
            return offsetPoint;
        }

        public bool DefineHeuristicArray(int xCoordinate, int yCoordinate, int[][][] iteratedBoards, int imageWidth, int imageHeight) //worry about edges
        {
            int XIndex, YIndex;
            Point offsetPoint;
            int sumOfHeuristicArray = 0;
            for (int i = 0; i < numberOfIteratedBoards; i++)
            {
                for (int j = 0; j < pixelsWithinGivenRadius; j++)
                {
                    offsetPoint = XYIndexOffsetPoint(j);
                    XIndex = xCoordinate + offsetPoint.X;
                    YIndex = yCoordinate + offsetPoint.Y;
                    if (XIndex < 1 || YIndex < 1 || YIndex > imageHeight - 2 || XIndex > imageWidth - 2)
                    {
                        currentHeuristicArray[i * (int)pixelsWithinGivenRadius + j] = -1;
                        sumOfHeuristicArray += -1;
                    }
                    else
                    {
                        currentHeuristicArray[i * (int)pixelsWithinGivenRadius + j] = iteratedBoards[i][XIndex][YIndex];
                        sumOfHeuristicArray += iteratedBoards[i][XIndex][YIndex];
                    }
                }
            }
            if (sumOfHeuristicArray > 30)
                return true;
            else
                return false;
        }

        public void CommitArrayToTrainingData(string inputLabel, Rectangle currentLetterSize)
        {
            //Build the currentTDItem
            TrainingData.TrainingDataElement currentTDItem = new TrainingData.TrainingDataElement();
            currentTDItem.dataLabel = inputLabel;
            currentTDItem.sizeOfLetter = currentLetterSize;
            currentTDItem.heuristicArray = currentHeuristicArray;
            //Add the item to the TDLibrary
            TrainingData.TrainingDataLibrary.Add(currentTDItem);



            //Test if the label is unique
            bool isThisLabelUnique = true;
            int indexOfExistingLabel = 0;
            for (int i = 0; i < TrainingData.TrainingDataIndex.Count(); i++)
            {
                if (inputLabel == TrainingData.TrainingDataIndex[i].dataLabel)
                {
                    isThisLabelUnique = false;
                    indexOfExistingLabel = i;
                    i = TrainingData.TrainingDataIndex.Count();
                }
            }
            if (isThisLabelUnique)
            {
                TrainingData.TrainingDataIndexElement currentTDIndexElement = new TrainingData.TrainingDataIndexElement();
                currentTDIndexElement.dataLabel = inputLabel;
                currentTDIndexElement.indexLocations = new List<int>();
                currentTDIndexElement.indexLocations.Add(TrainingData.TrainingDataLibrary.Count() - 1); //zero indexed (indecies start at 0)
                TrainingData.TrainingDataIndex.Add(currentTDIndexElement);
            }
            else
            { //Add our current index location to the List of index locations assocaited with the current label in the TrainingDataIndex
                TrainingData.TrainingDataIndex[indexOfExistingLabel].indexLocations.Add(TrainingData.TrainingDataLibrary.Count() - 1);
            }
        }
    }

    public class TrainingData
    {
        public struct TrainingDataIndexElement
        {
            public string dataLabel;
            public List<int> indexLocations;
        }

        public struct TrainingDataElement
        {
            public string dataLabel;
            public int[] heuristicArray;
            public Rectangle sizeOfLetter;
        }

        static public List<TrainingDataElement> TrainingDataLibrary = new List<TrainingDataElement>();
        static public List<TrainingDataIndexElement> TrainingDataIndex = new List<TrainingDataIndexElement>();
    }
}