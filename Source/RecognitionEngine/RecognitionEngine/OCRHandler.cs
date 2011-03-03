using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Prax.Recognition
{
    class OCRHandler
    {
        public List<Tuple<string, List<int>>> trainingLibrary { get; private set; }
        public List<List<int>> listOfIndicies { get; private set; }
        public List<string> listOfIndexLabels { get; private set; }

        public OCRHandler(TrainingDataOptions openOptions)
        {
            TrainingData TDLibrary = new TrainingData(openOptions);
            trainingLibrary = TDLibrary.trainingLibrary;
            listOfIndicies = TDLibrary.listOfIndicies;
            listOfIndexLabels = TDLibrary.listOfIndexLabels;
        }

        public void SaveTrainingData()
        {
            FileStream saveTrainingData = new FileStream("TrainingData.dat", FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(saveTrainingData, trainingLibrary);
                formatter.Serialize(saveTrainingData, listOfIndicies);
                formatter.Serialize(saveTrainingData, listOfIndexLabels);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                saveTrainingData.Close();
            }
        }

        private void submitLabel(string label)
        {
            Tuple<string, List<int>> tupleToAdd = new Tuple<string, List<int>>(label, heuristicArray);
            trainingLibrary.Add(tupleToAdd);

            bool isThisLabelUnique = true;
            int indexOfExistingLabel = 0;
            for (int i = 0; i < listOfIndexLabels.Count; i++)
            {
                if (label == listOfIndexLabels[i])
                {
                    isThisLabelUnique = false;
                    indexOfExistingLabel = i;
                    i = listOfIndexLabels.Count;
                }
            }
            if (isThisLabelUnique)
            {
                listOfIndexLabels.Add(label);
                listOfIndicies.Add(new List<int> { trainingLibrary.Count - 1 });
            }
            else
            {
                listOfIndicies[indexOfExistingLabel].Add(trainingLibrary.Count - 1);
            }
        }

        public void TrainDoubleArray(int[][] doubleArray, string label)
        {
            trainTestPreprocess(doubleArray);
            submitLabel(label);
        }

        public Tuple<string, double> ReadDoubleArray(int[][] doubleArray)
        {
            Tuple<string, double> resolvedSegment;
            trainTestPreprocess(doubleArray);
            resolvedSegment = readNextSegment();
            return resolvedSegment;
        }

        #region Train Test preprocess (iterated boards and heuristics)

        private const int numberOfIterations = 8;

        private int[][][] setOfIteratedBoards { get; set; }

        private int boardIterationCounter;

        private void initializeIteratedBoards()
        {
            setOfIteratedBoards = new int[numberOfIterations][][];
            for (int i = 0; i < numberOfIterations; i++)
            {
                setOfIteratedBoards[i] = new int[width][];
                for (int j = 0; j < width; j++)
                {
                    setOfIteratedBoards[i][j] = new int[height];
                }
            }
            boardIterationCounter = 0;
        }

        private int[][] iterateMe;

        private void iterate()
        {
            const int consolidationConstant = 3;

            int averageSurroundingDiscrepPxls;
            int rangeOfSurroundingPxls, rangeOfSurroundingDiscrepPxls;
            int[] surroundingPxls = new int[4];
            int[] surroundingDiscrepancyPxls = new int[4];

            if (boardIterationCounter == 0)
                iterateMe = currentSegment;

            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; j++)
                {
                    surroundingPxls[0] = iterateMe[i - 1][j];
                    surroundingPxls[1] = iterateMe[i][j - 1];
                    surroundingPxls[2] = iterateMe[i + 1][j];
                    surroundingPxls[3] = iterateMe[i][j + 1];

                    for (int k = 0; k < 4; k++)
                        surroundingDiscrepancyPxls[k] = Math.Abs(iterateMe[i][j] - surroundingPxls[k]);
                    averageSurroundingDiscrepPxls = (surroundingDiscrepancyPxls[0] + surroundingDiscrepancyPxls[1]
                                            + surroundingDiscrepancyPxls[2] + surroundingDiscrepancyPxls[3]) / 4;
                    rangeOfSurroundingDiscrepPxls = Math.Max(Math.Max(surroundingDiscrepancyPxls[0], surroundingDiscrepancyPxls[1]), Math.Max(surroundingDiscrepancyPxls[2], surroundingDiscrepancyPxls[3]))
                                            - Math.Min(Math.Min(surroundingDiscrepancyPxls[0], surroundingDiscrepancyPxls[1]), Math.Min(surroundingDiscrepancyPxls[2], surroundingDiscrepancyPxls[3]));
                    rangeOfSurroundingPxls = Math.Max(Math.Max(surroundingPxls[0], surroundingPxls[1]), Math.Max(surroundingPxls[2], surroundingPxls[3]))
                                            - Math.Min(Math.Min(surroundingPxls[0], surroundingPxls[1]), Math.Min(surroundingPxls[2], surroundingPxls[3]));

                    setOfIteratedBoards[boardIterationCounter][i][j] = ((averageSurroundingDiscrepPxls * rangeOfSurroundingPxls) / ((rangeOfSurroundingDiscrepPxls / 4) + 1)) / consolidationConstant;
                }
            }
            iterateMe = setOfIteratedBoards[boardIterationCounter];

            boardIterationCounter++;
        }

        private void iterateBitmap()
        {
            initializeIteratedBoards();
            for (int i = 0; i < numberOfIterations; i++)
                iterate();
        }

        private List<int> heuristicArray;
        private int sizeOfHeuristicArray;

        private void generateHeuristics()
        {
            sizeOfHeuristicArray = 1550;
            heuristicArray = new List<int>(sizeOfHeuristicArray);
            for (int i = 0; i < sizeOfHeuristicArray; i++)
                heuristicArray.Add(0);

            int currentIndex = 0;
            //Sum up the value of all the quadrants, add them and divide them into eachother

            for (int boardIdx = 0; boardIdx < numberOfIterations; boardIdx++)
            {
                int[] quadrantSum = new int[4];
                for (int i = 0; i < width / 2; i++)
                {
                    for (int j = 0; j < height / 2; j++)
                    {
                        quadrantSum[0] += setOfIteratedBoards[boardIdx][i][j];
                    }
                }
                for (int i = width / 2; i < width; i++)
                {
                    for (int j = 0; j < height / 2; j++)
                    {
                        quadrantSum[1] += setOfIteratedBoards[boardIdx][i][j];
                    }
                }
                for (int i = 0; i < width; i++)
                {
                    for (int j = height / 2; j < height; j++)
                    {
                        quadrantSum[2] += setOfIteratedBoards[boardIdx][i][j];
                    }
                }
                for (int i = width / 2; i < width; i++)
                {
                    for (int j = height / 2; j < height; j++)
                    {
                        quadrantSum[3] += setOfIteratedBoards[boardIdx][i][j];
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    if (quadrantSum[i] == 0)
                        quadrantSum[i] = 1;
                }
                heuristicArray[currentIndex] = quadrantSum[0];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[1];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[2];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[3];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[0] * 4 / quadrantSum[1];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[0] * 4 / quadrantSum[2];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[0] * 4 / quadrantSum[3];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[1] * 4 / quadrantSum[0];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[1] * 4 / quadrantSum[2];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[1] * 4 / quadrantSum[3];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[2] * 4 / quadrantSum[0];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[2] * 4 / quadrantSum[1];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[2] * 4 / quadrantSum[3];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[3] * 4 / quadrantSum[0];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[3] * 4 / quadrantSum[1];
                currentIndex++;
                heuristicArray[currentIndex] = quadrantSum[3] * 4 / quadrantSum[2];
                currentIndex++;
                heuristicArray[currentIndex] = (quadrantSum[3] + quadrantSum[2] + quadrantSum[1]) / quadrantSum[0];
                currentIndex++;
                heuristicArray[currentIndex] = (quadrantSum[3] + quadrantSum[2] + quadrantSum[0]) / quadrantSum[1];
                currentIndex++;
                heuristicArray[currentIndex] = (quadrantSum[3] + quadrantSum[0] + quadrantSum[1]) / quadrantSum[2];
                currentIndex++;
                heuristicArray[currentIndex] = (quadrantSum[0] + quadrantSum[2] + quadrantSum[1]) / quadrantSum[3];
                currentIndex++;
                heuristicArray[currentIndex] = 4 * (quadrantSum[0] + quadrantSum[2]) / (quadrantSum[1] + quadrantSum[3]);
                currentIndex++;
                heuristicArray[currentIndex] = 4 * (quadrantSum[0] + quadrantSum[1]) / (quadrantSum[2] + quadrantSum[3]);
                currentIndex++;
                heuristicArray[currentIndex] = 4 * (quadrantSum[0] + quadrantSum[3]) / (quadrantSum[1] + quadrantSum[2]);
                currentIndex++;
                heuristicArray[currentIndex] = 4 * (quadrantSum[1] + quadrantSum[2]) / (quadrantSum[0] + quadrantSum[3]);
                currentIndex++;
                heuristicArray[currentIndex] = 4 * (quadrantSum[3] + quadrantSum[1]) / (quadrantSum[2] + quadrantSum[0]);
                currentIndex++;
                heuristicArray[currentIndex] = 4 * (quadrantSum[2] + quadrantSum[3]) / (quadrantSum[1] + quadrantSum[0]);
                currentIndex++;
                //Add many many more here
            }

            //CENTER PIXELS HEURISTICS
            for (int boardIdx = 0; boardIdx < numberOfIterations; boardIdx++)
            {
                int sumOfCenterPixels = 0, productOfCenterPixels = 1;
                for (int j = 0; j < height; j++)
                {
                    sumOfCenterPixels += setOfIteratedBoards[boardIdx][width / 2][j];
                    sumOfCenterPixels += setOfIteratedBoards[boardIdx][width / 2 + 1][j];
                    sumOfCenterPixels += setOfIteratedBoards[boardIdx][width / 2 - 1][j];

                    if (setOfIteratedBoards[boardIdx][width / 2][j] != 0)
                        productOfCenterPixels *= setOfIteratedBoards[boardIdx][width / 2][j];
                }

                heuristicArray[currentIndex] = sumOfCenterPixels;
                currentIndex++;
                heuristicArray[currentIndex] = productOfCenterPixels;
                currentIndex++;
                heuristicArray[currentIndex] = sumOfCenterPixels / height;
                currentIndex++;
                heuristicArray[currentIndex] = productOfCenterPixels / height;
                currentIndex++;
                if (sumOfCenterPixels != 0)
                    heuristicArray[currentIndex] = productOfCenterPixels / sumOfCenterPixels; //fix /0
                else
                    heuristicArray[currentIndex] = 0;
                currentIndex++;
            }

            //VARIATION TEMPLATE MEASURED WITH RESPECT TO SIDE PIXELS AND ANGLE PIXELS

            int pixelTemplateSum;
            int currentPixel;
            int[] eightSurroundingPixels = new int[4];
            int baseThreeOffset = 80; //the sum of 1*2+3*2+9*2+27*2

            for (int boardIdx = 0; boardIdx < numberOfIterations; boardIdx++)
            {
                for (int i = 1; i < width - 1; i++)
                {
                    for (int j = 1; j < height - 1; j++)
                    {
                        eightSurroundingPixels[0] = setOfIteratedBoards[boardIdx][i - 1][j + 1];
                        eightSurroundingPixels[1] = setOfIteratedBoards[boardIdx][i + 1][j + 1];
                        eightSurroundingPixels[2] = setOfIteratedBoards[boardIdx][i - 1][j - 1];
                        eightSurroundingPixels[3] = setOfIteratedBoards[boardIdx][i + 1][j - 1];
                        currentPixel = setOfIteratedBoards[boardIdx][i][j];
                        pixelTemplateSum = 0;
                        for (int k = 0; k < 4; k++)
                        {
                            if (eightSurroundingPixels[k] == currentPixel)
                                pixelTemplateSum += (int)(Math.Pow(k, 3) * 0.0);
                            if (eightSurroundingPixels[k] > currentPixel)
                                pixelTemplateSum += (int)(Math.Pow(k, 3) * 1.0);
                            if (eightSurroundingPixels[k] < currentPixel)
                                pixelTemplateSum += (int)(Math.Pow(k, 3) * 2);
                        }
                        heuristicArray[currentIndex + pixelTemplateSum]++;
                    }
                }
                currentIndex += baseThreeOffset;
                for (int i = 1; i < width - 1; i++)
                {
                    for (int j = 1; j < height - 1; j++)
                    {
                        eightSurroundingPixels[0] = setOfIteratedBoards[boardIdx][i][j + 1];
                        eightSurroundingPixels[1] = setOfIteratedBoards[boardIdx][i + 1][j];
                        eightSurroundingPixels[2] = setOfIteratedBoards[boardIdx][i][j - 1];
                        eightSurroundingPixels[3] = setOfIteratedBoards[boardIdx][i - 1][j];

                        currentPixel = setOfIteratedBoards[boardIdx][i][j];
                        pixelTemplateSum = 0;
                        for (int k = 0; k < 4; k++)
                        {
                            if (eightSurroundingPixels[k] == currentPixel)
                                pixelTemplateSum += (int)(Math.Pow(k, 3) * 0.0);
                            if (eightSurroundingPixels[k] > currentPixel)
                                pixelTemplateSum += (int)(Math.Pow(k, 3) * 1.0);
                            if (eightSurroundingPixels[k] < currentPixel)
                                pixelTemplateSum += (int)(Math.Pow(k, 3) * 2);
                        }
                        heuristicArray[currentIndex + pixelTemplateSum]++;
                    }
                }
                currentIndex += baseThreeOffset;
            }
            //CATEGORIZE EVERY PIXEL ACCORDING TO ITS COMPLEXITY AND COLOR RANKING (HISTOGRAM)


            //Sum up the value of every board across all iterated boards
            int[] sumOfBoards = new int[numberOfIterations + 1];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    sumOfBoards[0] += currentSegment[i][j]; //questionably legal
                    for (int k = 0; k < numberOfIterations; k++)
                    {
                        sumOfBoards[k + 1] += setOfIteratedBoards[k][i][j];
                    }
                }
            }

            for (int k = 0; k < numberOfIterations + 1; k++)
            {
                heuristicArray[currentIndex + k] = sumOfBoards[k];
            }
            currentIndex += numberOfIterations + 1;

            //-GLOBAL- HEURISTIC PROPERTIES
            double widthHeightRatio = (double)width / (double)height;
            for (int decimalIdx = 0; decimalIdx < 10; decimalIdx++)
            {
                heuristicArray[currentIndex] = (int)(widthHeightRatio * Math.Pow(10, decimalIdx));
                currentIndex++;
            }
            heuristicArray[currentIndex] = height + width;
            currentIndex++;
            int sumOfAllHeuristics = 0;
            for (int i = 0; i < sizeOfHeuristicArray - 2; i++)
            {
                sumOfAllHeuristics += heuristicArray[i];
            }
            heuristicArray[currentIndex] = sumOfAllHeuristics;
            currentIndex++;
            heuristicArray[currentIndex] = sumOfAllHeuristics / sizeOfHeuristicArray;


            //global properties for each one of the iterated boards
            //each heuristic represents an orientation of relative pixel colors, the number of each heuristic is the amount of the those templates
            //Sum up all the heurisitics for each board, take the average, all the boards together
        }

        int width, height;
        int[][] currentSegment;

        private void trainTestPreprocess(int[][] segment)
        {
            width = segment.GetLength(0);
            height = segment[0].GetLength(0);
            currentSegment = segment;
            iterateBitmap();
            generateHeuristics();
        }
        #endregion

        #region Test/Read algorithm

        private class variancesForEachHeuristic
        {
            public List<List<rollingVarianceCalc>> var2 = new List<List<rollingVarianceCalc>>();

            public variancesForEachHeuristic(int numberOfHeur)
            {
                for (int i = 0; i < numberOfHeur; i++)
                    var2.Add(new List<rollingVarianceCalc>());
            }

            public class rollingVarianceCalc
            {
                public int n = 0;
                double mean = 0,
                        M2 = 0;

                public double calcOnlineVar(double x)
                {
                    n++;
                    double delta = x - mean;
                    mean = mean + delta / n;
                    M2 = M2 + delta * (x - mean);

                    double variance = M2 / (n - 1);
                    return variance;
                }
            }

            public double newVarianceValue(double value, int heuristicIdx, int labelIdx)
            {
                if (labelIdx >= var2[heuristicIdx].Count)
                {
                    while (labelIdx >= var2[heuristicIdx].Count)
                        var2[heuristicIdx].Add(new rollingVarianceCalc());
                    return var2[heuristicIdx][labelIdx].calcOnlineVar(value);
                }
                else
                {
                    return var2[heuristicIdx][labelIdx].calcOnlineVar(value);
                }
            }
        }

        private static variancesForEachHeuristic variances; //TODO: Make this its own class so it won't have to be static 
        private static HeuristicsControlPanel heuristicsControl = new HeuristicsControlPanel();

        private int numberOfUniqueLabels;
        public Tuple<string, double> readNextSegment()
        {
            numberOfUniqueLabels = listOfIndexLabels.Count;
            //int numberOfLabelsToCount = numberOfUniqueLabels - IndiciesToCheck.Count;
            int numberOfLabelsToCount = numberOfUniqueLabels;
            double[][] probabilityFromEachHeuristic = new double[numberOfLabelsToCount][];
            double[][] lblComparisonResults = new double[numberOfLabelsToCount][];
            double[] labelProbability;
            double[] totalComparison_test = new double[numberOfLabelsToCount];

            if (variances == null)
            {
                variances = new variancesForEachHeuristic(sizeOfHeuristicArray);
            }
            

            for (int i = 0; i < numberOfLabelsToCount; i++)
            {
                probabilityFromEachHeuristic[i] = new double[sizeOfHeuristicArray];
                lblComparisonResults[i] = new double[sizeOfHeuristicArray];
            }

            for (int heurIdx = 0; heurIdx < sizeOfHeuristicArray; heurIdx++)
            {
                //foreach(int inspectionLbl in getInspectionLabelIdicies())
                for (int inspectionLbl = 0; inspectionLbl < numberOfUniqueLabels; inspectionLbl++) //This is the label we are trying to get information about. We want to know how feasible this label is given our current array
                {
                    for (int lblTrialIdx = 0; lblTrialIdx < listOfIndicies[inspectionLbl].Count; lblTrialIdx++)
                    { //Iterate through every heuristic array within our given label
                        if (heuristicArray[heurIdx] == trainingLibrary[listOfIndicies[inspectionLbl][lblTrialIdx]].Item2[heurIdx] && heuristicArray[heurIdx] != -1)//  && heuristicArray[heurIdx] != 0) //RESOLVE THE NECESSITY OF != 0 IN THIS STATEMENT
                        {
                            lblComparisonResults[inspectionLbl][heurIdx]++;
                        }
                    }
                    totalComparison_test[inspectionLbl] += lblComparisonResults[inspectionLbl][heurIdx];
                }
                for (int labelIndex = 0; labelIndex < numberOfUniqueLabels; labelIndex++)
                //foreach (int labelIndex in getInspectionLabelIdicies())
                {
                    lblComparisonResults[labelIndex][heurIdx] = lblComparisonResults[labelIndex][heurIdx] / (double)listOfIndicies[labelIndex].Count;
                }
                //This data structure represents:
            }       //What percentage of the time the current heuristics on the current heuristic array were equal to the heuristics on the training data for every given number

            //We are working to produce two DSs: lblComparisonResults[][], totalComparison_test[]

            double heuristicProbabilisticIndication;
            double multiplicativeOffset;
            labelProbability = new double[numberOfLabelsToCount];
            double maxProb = 0;
            int maxProbIndex = 0;
            double aprioriProb = 1.0 / (double)numberOfLabelsToCount;
            double factorIncrease = (1.0 - aprioriProb) / aprioriProb;

            for (int inspectionLbl = 0; inspectionLbl < numberOfUniqueLabels; inspectionLbl++)
            //foreach (int inspectionLbl in getInspectionLabelIdicies())
            {
                labelProbability[inspectionLbl] = 1.0 / (double)numberOfLabelsToCount;
                for (int heurIdx = 0; heurIdx < sizeOfHeuristicArray; heurIdx++)
                {
                    double comparisonToThisLabel = lblComparisonResults[inspectionLbl][heurIdx];
                    double comparisonToOtherLabels = 0;
                    for (int comparisonLbl = 0; comparisonLbl < numberOfUniqueLabels; comparisonLbl++)
                    //foreach (int comparisonLbl in getInspectionLabelIdicies())
                    {
                        if (inspectionLbl != comparisonLbl)
                            comparisonToOtherLabels += lblComparisonResults[comparisonLbl][heurIdx];
                    }

                    if (comparisonToThisLabel + comparisonToOtherLabels != 0)
                    {
                        heuristicProbabilisticIndication = comparisonToThisLabel / (comparisonToThisLabel + comparisonToOtherLabels);
                        heuristicsControl.buildHeuristicProbabilityHistorgram(heuristicProbabilisticIndication, inspectionLbl, heurIdx);                      
                        multiplicativeOffset = variances.newVarianceValue(heuristicProbabilisticIndication, heurIdx, inspectionLbl);
                        multiplicativeOffset += aprioriProb / (double)variances.var2[heurIdx][inspectionLbl].n;
                        if (multiplicativeOffset < double.MaxValue)
                        {
                            labelProbability[inspectionLbl] *= (factorIncrease * heuristicProbabilisticIndication + multiplicativeOffset) / (1 - heuristicProbabilisticIndication + multiplicativeOffset);
                        }
                        if (double.IsInfinity(labelProbability[inspectionLbl]) || labelProbability[inspectionLbl] == 0)
                            heurIdx = sizeOfHeuristicArray;
                    }
                }
                if (labelProbability[inspectionLbl] > maxProb && listOfIndicies[maxProbIndex].Count > 0)
                {
                    maxProb = labelProbability[inspectionLbl];
                    maxProbIndex = inspectionLbl;
                }
            }
            if (maxProb > 0)
            {
                return new Tuple<string,double>(listOfIndexLabels[maxProbIndex], maxProb);
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
