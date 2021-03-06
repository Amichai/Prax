﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Diagnostics;

namespace Prax.Recognition
{
    class Segmentator
    {
        private int[][] uploadedDocument; 
        private int width, height;
        //variables for marking segmentation progress
        public int MaxProgress, CurrentProgress; //TODO: Implement progress variables

        public Segmentator(int[][] uploaded)
        {
            this.uploadedDocument = uploaded;
            width = uploadedDocument.Length;
            height = uploadedDocument[0].Length;
            
            colorComplexityHistograms();
            identifyWordEdges(); 
        }

        #region Color and complexity histograms
        private int[] colorHistogram = new int[256];
        private int[] complexityHistorgram = new int[256 * 8];

        private int determineTotalDifference(int i, int j)
        {
            return Math.Abs(uploadedDocument[i][j] - uploadedDocument[i][j + 1]) +
                    Math.Abs(uploadedDocument[i][j] - uploadedDocument[i + 1][j + 1]) +
                    Math.Abs(uploadedDocument[i][j] - uploadedDocument[i + 1][j]) +
                    Math.Abs(uploadedDocument[i][j] - uploadedDocument[i + 1][j - 1]) +
                    Math.Abs(uploadedDocument[i][j] - uploadedDocument[i][j - 1]) +
                    Math.Abs(uploadedDocument[i][j] - uploadedDocument[i - 1][j - 1]) +
                    Math.Abs(uploadedDocument[i][j] - uploadedDocument[i - 1][j]) +
                    Math.Abs(uploadedDocument[i][j] - uploadedDocument[i - 1][j + 1]);
        }

        private void colorComplexityHistograms()
        {
            int maxValue = 0, minValue = 255, index;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {

                    if (uploadedDocument[i][j] > maxValue)
                        maxValue = uploadedDocument[i][j];
                    if (uploadedDocument[i][j] < minValue)
                        minValue = uploadedDocument[i][j];
                    colorHistogram[uploadedDocument[i][j]]++;

                    if (i != width - 1 && i != 0 && j != height - 1 && j != 0)
                    {
                        index = determineTotalDifference(i, j);
                        complexityHistorgram[index]++;
                    }
                }
            }
        }
        #endregion

        #region Resolve word edges
        private int findLowerBoundForColor(int colorThreshold)
        {
            int lowerBound = 0;
            int i = 0;

            while (i < colorHistogram.Length)
            {
                lowerBound += colorHistogram[i];
                if (lowerBound > colorThreshold)
                {
                    lowerBound = i;
                    i = colorHistogram.Length;
                }
                i++;
            }
            return lowerBound;
        }

        private int findLowerBoundForComplexity(int complexityThreshold)
        {
            int lowerBound = 0;
            int i = 1; //we skip 0 complexity

            while (i < complexityHistorgram.Length)
            {
                lowerBound += complexityHistorgram[i];
                if (lowerBound > complexityThreshold)
                {
                    lowerBound = i;
                    i = complexityHistorgram.Length;
                }
                i++;
            }
            return lowerBound;
        }
        
        public wordBoundary[,] labeledPixels; //Defines pixels as edge (2), foreground (0), background (1)
        private List<Point> wordOutlinePoints = new List<Point>(); //contains all the points resolved from all the word outlines

        public enum wordBoundary { insideWord, wordEdge, outsideWord };

        private void identifyWordEdges()
        {
            int colorThreshold = 9 * (width * height) / 10;
            int complexityThreshold = (complexityHistorgram.Sum() - complexityHistorgram[0]) / 4; //9
            int colorLowerBound = findLowerBoundForColor(colorThreshold);
            int complexityLowerBound = findLowerBoundForComplexity(complexityThreshold);
            int totalDifference;
            labeledPixels = new wordBoundary[width, height];
            DisplayUtility.DisplayMask display = new DisplayUtility.DisplayMask(uploadedDocument);
            for (int i = 1; i < width - 1; i++)
            {
                for (int j = 1; j < height - 1; j++)
                {
                    totalDifference = determineTotalDifference(i, j);  //The bigger the number associated with the color the closer to white the color is
                    if (uploadedDocument[i][j] >= colorLowerBound || totalDifference < complexityLowerBound)
                    {
                        display.PixelToDisplay(new Point(i, j), DisplayUtility.DisplayMarker1); //Blue
                        labeledPixels[i, j] = wordBoundary.outsideWord;
                        if ((labeledPixels[i - 1, j] != wordBoundary.outsideWord && labeledPixels[i - 1, j] != wordBoundary.wordEdge)
                            || (labeledPixels[i, j - 1] != wordBoundary.outsideWord && labeledPixels[i, j - 1] != wordBoundary.wordEdge))
                        {
                            wordOutlinePoints.Add(new Point(i, j));
                            display.PixelToDisplay(new Point(i, j), DisplayUtility.DisplayMarker2); //Green
                            labeledPixels[i, j] = wordBoundary.wordEdge;
                        }
                    }
                    else if (labeledPixels[i - 1, j] == wordBoundary.outsideWord || labeledPixels[i, j - 1] == wordBoundary.outsideWord)
                    {
                        wordOutlinePoints.Add(new Point(i, j));
                        display.PixelToDisplay(new Point(i, j), DisplayUtility.DisplayMarker2); //Green
                        labeledPixels[i, j] = wordBoundary.wordEdge;
                    }
                }
            }
            //display.RenderBitmap();
            //DisplayUtility.NewFormForDisplay temp = new DisplayUtility.NewFormForDisplay(display.BitmapToRender);
        }
        #endregion
        
        #region Resolve All Word segments
        private Rectangle discreteLoopRectangle;

        private const int segBorder = 10;
        private OCRSegment defineSegmentObjectToReturn(List<Point> loopOfPoints)
        {
            OCRSegment segment = new OCRSegment();
            int segmentWidthIncrease = 2; // this adds some columns & rows of pixels to get back the information lost at the segment edges

            int segWidth = discreteLoopRectangle.Width + segBorder + segmentWidthIncrease,
                segHeight = discreteLoopRectangle.Height + segBorder + segmentWidthIncrease;

            segment.InternalPoints = new int[segWidth][];
            for (int i = 0; i < segWidth; i++)
                segment.InternalPoints[i] = new int[segHeight];

            for (int i = 0; i < segWidth; i++)
                for (int j = 0; j < segHeight; j++)
                    segment.InternalPoints[i][j] = 255;

            int leftBound = discreteLoopRectangle.X,
                rightBound = discreteLoopRectangle.X + discreteLoopRectangle.Width,
                topBound = discreteLoopRectangle.Y,
                bottomBound = discreteLoopRectangle.Y + discreteLoopRectangle.Height;

            int lowestXVal = int.MaxValue,
                lowestYVal = int.MaxValue,
                highestYVal = int.MinValue,
                highestXVal = int.MinValue;

            int correctedX, correctedY;
            for (int i = leftBound - 1; i <= rightBound; i++)
            {
                for (int j = topBound - 1; j <= bottomBound; j++)
                {
                    bool inBounds = i < labeledPixels.GetLength(0) && j < labeledPixels.GetLength(1) && i > 0 && j > 0;
                    if (inBounds && labeledPixels[i, j] != wordBoundary.outsideWord)
                    {
                        correctedX = i - (leftBound) + (segBorder / 2) + 1; //This ensures a buffer of 5 white pixels on every border
                        correctedY = j - (topBound) + (segBorder / 2) + 1;
                        segment.InternalPoints[correctedX][correctedY] = uploadedDocument[i][j];
                        if (i < lowestXVal) lowestXVal = i;
                        if (i > highestXVal) highestXVal = i;
                        if (j < lowestYVal) lowestYVal = j;
                        if (j > highestYVal) highestYVal = j;
                    }
                }
            }
            //DisplayUtility.NewFormForDisplay temp = new DisplayUtility.NewFormForDisplay(segment.InternalPoints);
            segment.SegmentLocation = new Rectangle(lowestXVal, lowestYVal, highestXVal - lowestXVal, highestYVal - lowestYVal);
            return segment;
        }

        //ELIMINATE:
        private bool testSegmentForPlausibilty(OCRSegment returnedSegment) //Refactor this functionality
        {
            //test the center pixels for complexity/information
            if (returnedSegment.InternalPoints.Length > 14 && returnedSegment.InternalPoints[0].Length > 14 && returnedSegment.InternalPoints[0].Length < 200)
                return true;
            else
                return false;
        }

        class AllBorderPoints {
            private HashSet<Point> setOfBorderPoints = new HashSet<Point>();
            public List<Point> listOfPoints = new List<Point>();
            private wordBoundary[,] labeledPixels;
            private int maxProgress, currentProgress = 0;
            private int[][] uploadedDocument;
            

            public AllBorderPoints(wordBoundary[,] LabeledPixels, List<Point> outlnePoints, int[][] UploadedDocument) {
                labeledPixels = LabeledPixels;
                listOfPoints = outlnePoints;
                setOfBorderPoints = ReturnInternalPoints(listOfPoints);
                maxProgress = setOfBorderPoints.Count();
                listOfPoints = setOfBorderPoints.ToList();
                uploadedDocument = UploadedDocument;
            }

            private HashSet<Point> ReturnInternalPoints(List<Point> currentPoints) {
                HashSet<Point> internalLoopPoints = new HashSet<Point>();
                Point currentPoint;
                for (int k = 0; k < currentPoints.Count; k++) {
                    currentPoint = new Point(currentPoints[k].X, currentPoints[k].Y);
                    for (int i = currentPoint.X - 1; i <= currentPoint.X + 1; i++) {
                        for (int j = currentPoint.Y - 1; j <= currentPoint.Y + 1; j++) {
                            if (labeledPixels[i, j] == wordBoundary.outsideWord && !internalLoopPoints.Contains(new Point(i, j))) {
                                internalLoopPoints.Add(new Point(i, j));
                            }
                        }
                    }
                }
                return internalLoopPoints;
            }

            private IEnumerable<Point> assessSurroundingPoint(Point point, List<Point> internalLoopPoints,
                HashSet<Point> listOfAllAssociatedPoints, List<Point> associatedUnexaminedPoints){
                Point currentPoint;
                for (int i = point.X - 1; i <= point.X + 1; i++){
                    for (int j = point.Y - 1; j <= point.Y + 1; j++){
                        currentPoint = new Point(i, j);
                        if (internalLoopPoints.Contains(currentPoint) && !listOfAllAssociatedPoints.Contains(currentPoint)){
                            listOfAllAssociatedPoints.Add(currentPoint);
                            associatedUnexaminedPoints.Add(currentPoint);
                            yield return currentPoint;
                        }
                    }
                }
            }
            
            private Rectangle discreteLoopRectangle;

            private void reassessBounds(Point pointToAdd) {
                int rightBound = discreteLoopRectangle.X + discreteLoopRectangle.Width,
                    bottomBound = discreteLoopRectangle.Y + discreteLoopRectangle.Height;

                if (pointToAdd.X < discreteLoopRectangle.X)
                    discreteLoopRectangle.X = pointToAdd.X;
                if (pointToAdd.Y < discreteLoopRectangle.Y)
                    discreteLoopRectangle.Y = pointToAdd.Y;
                if (pointToAdd.X > rightBound) {
                    discreteLoopRectangle.Width = pointToAdd.X - discreteLoopRectangle.X;
                    rightBound = discreteLoopRectangle.X + discreteLoopRectangle.Width;
                }
                if (pointToAdd.Y > bottomBound) {
                    discreteLoopRectangle.Height = pointToAdd.Y - discreteLoopRectangle.Y;
                    bottomBound = discreteLoopRectangle.Y + discreteLoopRectangle.Height;
                }
                if (bottomBound < int.MaxValue)

                    discreteLoopRectangle.Height = bottomBound - discreteLoopRectangle.Y;
                if (rightBound < int.MaxValue)
                    discreteLoopRectangle.Width = rightBound - discreteLoopRectangle.X;
            }

            public IEnumerable<List<Point>> ReturnWordLoops() {
                HashSet<Point> listOfAllAssociatedPoints = new HashSet<Point>();
                List<Point> associatedUnexaminedPoints = new List<Point>();
                List<Point> pointUnexamiendOnce = new List<Point>();
                int xVal, yVal;
                int index;
                for (int k = 0; k < listOfPoints.Count; k++) {
                    if (!listOfAllAssociatedPoints.Contains(listOfPoints[k])) {
                        associatedUnexaminedPoints.Add(listOfPoints[k]);
                        List<Point> resolvedLoop = new List<Point>();
                        discreteLoopRectangle = new Rectangle(int.MaxValue, int.MaxValue, 0, 0);
                        while (associatedUnexaminedPoints.Count > 0) {
                            index = 0;
                            listOfAllAssociatedPoints.Add(associatedUnexaminedPoints[index]);
                            xVal = associatedUnexaminedPoints[index].X;
                            yVal = associatedUnexaminedPoints[index].Y;
                            foreach (Point pointToAdd in assessSurroundingPoint(new Point(xVal, yVal), listOfPoints,
                                                                        listOfAllAssociatedPoints, associatedUnexaminedPoints)) {
                                resolvedLoop.Add(pointToAdd);
                                reassessBounds(pointToAdd);
                            }
                            associatedUnexaminedPoints.RemoveAt(index);
                        }
                        DisplayUtility.NewFormForDisplay(listOfPoints, uploadedDocument);
                        DisplayUtility.NewFormForDisplay(resolvedLoop, uploadedDocument);
                        currentProgress += resolvedLoop.Count + 1;
                        float currentPercentProgress = (((float)currentProgress / maxProgress) * 100);
                        Debug.Print(currentPercentProgress.ToString());
                        yield return resolvedLoop;
                    }
                }
            }
        }

        private void SendSegmentToUI(int[][] content, Rectangle location) {
            Bitmap bitmap = DisplayUtility.ConvertDoubleArrayToBitmap(content, Color.White);
            OnDisplaySegment(new DisplaySegEventArgs(bitmap,location));
        }

        private IEnumerable<OCRSegment> handleSegmentsSizes(AllBorderPoints borderPoints) {
            //DisplayUtility.NewFormForDisplay(borderPoints.listOfPoints, uploadedDocument);
            OCRSegment wordSegment = new OCRSegment();
            foreach (List<Point> wordPointLoop in borderPoints.ReturnWordLoops()) {
                //DisplayUtility.NewFormForDisplay(wordPointLoop, uploadedDocument);
                if (wordPointLoop.Count > 5 && wordPointLoop.Count < 400) { //Test the amount of points in the loop
                    wordSegment = defineSegmentObjectToReturn(wordPointLoop);
                    if (testSegmentForPlausibilty(wordSegment)) {  //Test the dimensions of the points
                        wordSegment.IsAWord = true;
                        foreach (OCRSegment subSegment in defineSubSegments(wordSegment)) {
                            subSegment.IsAWord = false;
                            SendSegmentToUI(subSegment.InternalPoints, new Rectangle(0, 0, 0, 0)); //TODO: Derive the actual location of the subsegment
                            yield return subSegment;
                        }
                        SendSegmentToUI(wordSegment.InternalPoints, wordSegment.SegmentLocation);
                        yield return wordSegment;
                    } else {
                        AllBorderPoints next = new AllBorderPoints(labeledPixels, wordPointLoop, uploadedDocument);
                        handleSegmentsSizes(next);
                    }
                } 
            }
        }

        public IEnumerable<OCRSegment> DefineSegments(){
            AllBorderPoints borderPoints = new AllBorderPoints(labeledPixels, wordOutlinePoints, uploadedDocument);
            foreach(OCRSegment segmentToReturn in handleSegmentsSizes(borderPoints))
                yield return segmentToReturn;
        }

		public static event EventHandler<DisplaySegEventArgs> DisplaySegment;
		void OnDisplaySegment(DisplaySegEventArgs e) {
			var copy = DisplaySegment;
			if (copy != null) copy(this, e);
		}
        #endregion

        #region Resolve All Letter SubSegments

        private int[][] addBorder(int[][] doubleArray)
        {
            int origionalWidth = doubleArray.Length,
                origionalHeight = doubleArray[0].Length;
            int segWidth = origionalWidth + segBorder,
                segHeight = origionalHeight;
            int[][] doubleArrayToReturn = new int[segWidth][];
            for (int i = 0; i < segWidth; i++)
            {
                doubleArrayToReturn[i] = new int[segHeight];
            }
            for (int i = 0; i < segWidth; i++)
            {
                for (int j = 0; j < segHeight; j++)
                {
                    doubleArrayToReturn[i][j] = 255;
                }
            }
            int correctedX, correctedY;
            for (int i = 0; i < origionalWidth; i++)
            {
                for (int j = 0; j < origionalHeight; j++)
                {
                    correctedX = i + (segBorder / 2);
                    correctedY = j;
                    doubleArrayToReturn[correctedX][correctedY] = doubleArray[i][j];
                }
            }
            return doubleArrayToReturn;
        }

        public int MaximumSubSegmentWidth = 15;
        public int MinimumSubSegmentWidth = 2;
        

        private IEnumerable<OCRSegment> takeSegmentBreaksAndReturnSubSegments(OCRSegment wordSegment, List<int> breakPoints, int height, int borderOffset)
        {
            int subSegmentWidth = 0;
            OCRSegment subSegmentToReturn = new OCRSegment();
            int[][] newSegmentToReturn;
            for (int startIdx = 0; startIdx < breakPoints.Count - 1; startIdx++)
            {
                int endIndex = startIdx + 1;
                subSegmentWidth = breakPoints[endIndex] - breakPoints[startIdx];
                while (endIndex < breakPoints.Count && subSegmentWidth <= MaximumSubSegmentWidth && subSegmentWidth > 2)
                {
                    subSegmentWidth = breakPoints[endIndex] - breakPoints[startIdx];
                    newSegmentToReturn = new int[subSegmentWidth][];
                    for (int i = 0; i < subSegmentWidth; i++)
                        newSegmentToReturn[i] = new int[height];

                    for (int i = 0; i < subSegmentWidth; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            int xRenderIndex = i + breakPoints[startIdx];
                            newSegmentToReturn[i][j] = wordSegment.InternalPoints[xRenderIndex][j];
                        }
                    }
                    newSegmentToReturn = addBorder(newSegmentToReturn);
                    subSegmentToReturn.InternalPoints = newSegmentToReturn;
                    
                    subSegmentToReturn.SegmentLocation = new Rectangle(wordSegment.SegmentLocation.X - borderOffset + breakPoints[startIdx],
                                                                        wordSegment.SegmentLocation.Y,
                                                                        breakPoints[endIndex] - breakPoints[startIdx] - 1,
                                                                        wordSegment.SegmentLocation.Height);
                    endIndex++;
                    //DisplayUtility.NewFormForDisplay test = new DisplayUtility.NewFormForDisplay(subSegmentToReturn.InternalPoints);
                    yield return subSegmentToReturn;
                }
            }
        }

        private IEnumerable<OCRSegment> defineSubSegments(OCRSegment wordSegment)
        { 
            int width = wordSegment.InternalPoints.Length,
                height = wordSegment.InternalPoints[0].Length;
            int border = segBorder / 2;

            List<int> listOfVerticalSums = new List<int>();
            for (int i = border; i < width - border; i++)
            {
                int verticalSum = 0;
                for (int j = 0; j < height; j++)
                {
                    verticalSum += wordSegment.InternalPoints[i][j];
                }
                listOfVerticalSums.Add(verticalSum);
            }
            List<int> diffVerticalSums = new List<int>();
            for (int i = 1; i < listOfVerticalSums.Count; i++)
            {
                diffVerticalSums.Add(listOfVerticalSums[i - 1] - listOfVerticalSums[i]);
            }
            int numberOfBreaks = (width - segBorder);

            var sortedVerticalSums = listOfVerticalSums.Select((x, i) => new KeyValuePair<int, int>(x, i))
                                            .OrderBy(x => x.Key).ToList();

            List<int> ignoreIndicies = new List<int>();
            int thresholdRating = sortedVerticalSums[listOfVerticalSums.Count - numberOfBreaks].Key;
            for (int i = 0; i < listOfVerticalSums.Count - 1; i++)
            {
                if (listOfVerticalSums[i] >= thresholdRating && listOfVerticalSums[i + 1] >= thresholdRating)
                {
                    if (sortedVerticalSums[i].Key >= sortedVerticalSums[i + 1].Key)
                    {
                        ignoreIndicies.Add(i + 1);
                    }
                    else
                        ignoreIndicies.Add(i);
                }
            }
            List<int> segmentBreakPoints = new List<int>();

            for (int i = listOfVerticalSums.Count - 1; i >= 0; i--)
            {
                if (i >= listOfVerticalSums.Count - numberOfBreaks)
                {
                    if (!ignoreIndicies.Contains(sortedVerticalSums[i].Value)
                        && sortedVerticalSums[i].Value > 1
                        && sortedVerticalSums[i].Value < listOfVerticalSums.Count - 2)
                    {
                        segmentBreakPoints.Add(sortedVerticalSums[i].Value + border);
                    }
                }
            }
            segmentBreakPoints.Add(width - border);
            segmentBreakPoints.Add(border);
            segmentBreakPoints.Sort();

            foreach (OCRSegment subSegToReturn in takeSegmentBreaksAndReturnSubSegments(wordSegment, segmentBreakPoints, height, border))
                yield return subSegToReturn;
        }
        #endregion
    }
}
