using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace Prax.Recognition {
	class SegmentatorV2 {

		private int[][] uploadedDocument;
		private static int width, height;

		public SegmentatorV2(int[][] uploaded) {
			this.uploadedDocument = uploaded;
			width = uploadedDocument.Length;
			height = uploadedDocument[0].Length;
		}

		private class singleRow {
			List<int> rowContents;
			public int textStart = int.MinValue,
						textEnd;
			public int RowSum = 0;

			public singleRow(int width) {
				rowContents = new List<int>(width);
				rowContents.AddRange(new int[width]);
			}

			public void AddPixel(int color, int index) {
				int offsetAjustedColor = (255 - color);
				rowContents[index] += offsetAjustedColor;
				RowSum += offsetAjustedColor;
				if (offsetAjustedColor != 0) {
					textEnd = index;
					if (textStart == int.MinValue)
						textStart = index;
				}
			}
		}

		private List<singleRow> allRows = new List<singleRow>();
		private List<int> rowDerivative = new List<int>();

		private void calculateRowAndColumnSums() {
			allRows.AddRange(new singleRow[height]);
			for (int i = 0; i < height; i++) {
				allRows[i] = new singleRow(width);
			}
			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					allRows[j].AddPixel(uploadedDocument[i][j], i);
				}
			}
			for (int i = 1; i < height; i++) {
				rowDerivative.Add(Math.Abs(allRows[i].RowSum - allRows[i - 1].RowSum));
			}
		}

		public class lineOfText {
			int topBound, bottomBound, centerLine;
			public lineOfText(int top, int bottom, int center) {
				topBound = top;
				bottomBound = bottom;
				centerLine = center;
			}

			public List<Tuple<int, int>> ScanSum = new List<Tuple<int, int>>();

			public void LinearScan(int[][] uploadedDocument) {
				int lineHeight = bottomBound - topBound;
				int lineWidth = uploadedDocument.Length;
				int yIndex;
				List<Tuple<int, int>> TempScanSum = new List<Tuple<int, int>>();
				for (int i = 0; i < lineWidth; i++) {
					int tempSum = 0;
					for (int j = 0; j < lineHeight; j++) {
						yIndex = topBound + j;
						tempSum += MyColor.Offest(uploadedDocument[i][yIndex]);
					}
					TempScanSum.Add(new Tuple<int, int>(tempSum, i));
				}
				for (int i = 1; i < TempScanSum.Count - 1; i++) {
					if (TempScanSum[i].Item1 > 0 || TempScanSum[i - 1].Item1 > 0 || TempScanSum[i + 1].Item1 > 0) {
						ScanSum.Add(TempScanSum[i]);
					}
				}
			}

			List<int> breakIndicies = new List<int>();

			public void DefineSegmentBreaks(int numOfBreaks) {
				int min = ScanSum.Min(t => t.Item2);

				breakIndicies = ScanSum.OrderBy(t => t.Item1) //Order by linearscan sums
											.Take(numOfBreaks)        //Take only the highest values
											.Select(t => t.Item2)     //Keep the indicies
											.OrderBy(t => t)
											.ToList();
			}

			public IEnumerable<OCRSegment> BuildSegments(int MaxSegWidth, int[][] uploadedDocument) {
				DisplayUtility.NewFormForDisplay(uploadedDocument);
				int lineHeight = bottomBound - topBound;
				int subSegmentWidth = 0;
				OCRSegment segToReturn = new OCRSegment();
				int[][] newSegmentToReturn;
				for (int startIdx = 0; startIdx < breakIndicies.Count - 1; startIdx++) {
					int endIndex = startIdx + 1;
					subSegmentWidth = breakIndicies[endIndex] - breakIndicies[startIdx];
					while (endIndex < breakIndicies.Count && subSegmentWidth <= MaxSegWidth && subSegmentWidth > 1) {
						subSegmentWidth = breakIndicies[endIndex] - breakIndicies[startIdx];
						newSegmentToReturn = new int[subSegmentWidth][];
						for (int i = 0; i < subSegmentWidth; i++)
							newSegmentToReturn[i] = new int[lineHeight];
						for (int i = 0; i < subSegmentWidth; i++) {
							for (int j = 0; j < lineHeight; j++) {
								int xRenderIndex = i + breakIndicies[startIdx];
								int yRenderIndex = j + topBound;
								newSegmentToReturn[i][j] = uploadedDocument[xRenderIndex][yRenderIndex];
							}
						}
						segToReturn.InternalPoints = newSegmentToReturn;
						int segWidth = breakIndicies[endIndex] - breakIndicies[startIdx] - 1;
						segToReturn.SegmentLocation = new Rectangle(breakIndicies[startIdx], topBound, segWidth, lineHeight);
						endIndex++;
						yield return segToReturn;
					}
				}
			}
		}

		List<lineOfText> allTextLines = new List<lineOfText>();

		private void defineLineBreaks() {
			List<Tuple<double, int>> indexAndRatings = new List<Tuple<double, int>>();
			for (int i = 0; i < height; i++) {
				int textRange = (allRows[i].textEnd - allRows[i].textStart);
				if (textRange > 20) {
					double rowRating = (double)allRows[i].RowSum / textRange;
					indexAndRatings.Add(new Tuple<double, int>(rowRating, i));
				}
			}
			var orderedRows = indexAndRatings.OrderByDescending(i => i.Item1)
												.ToList();
			List<int> emptyRowIdxs = new List<int>();
			for (int i = 1; i < height - 1; i++) {
				if ((allRows[i].RowSum == 0 && allRows[i + 1].RowSum != 0)
					|| (allRows[i].RowSum == 0 && allRows[i - 1].RowSum != 0)) {
					emptyRowIdxs.Add(i);
				}
			}
			for (int i = 0; i < emptyRowIdxs.Count - 1; i++) {
				for (int j = 0; j < orderedRows.Count / 10; j++) { //orderedRows.Count / 10 is the amount of lines of text that we are willing to check
					if (orderedRows[j].Item2 > emptyRowIdxs[i] && orderedRows[j].Item2 < emptyRowIdxs[i + 1]) {
						int centerLineIdx = orderedRows[j].Item2;
						allTextLines.Add(new lineOfText(emptyRowIdxs[i], emptyRowIdxs[i + 1], centerLineIdx));
						j = orderedRows.Count;
						i++;
					}
				}
			}
		}

		private OCRSegment addBorder(OCRSegment seg) {
			int[][] doubleArray = seg.InternalPoints;
			int segBorder = 10;
			int origionalWidth = doubleArray.Length,
					origionalHeight = doubleArray[0].Length;
			int segWidth = origionalWidth + segBorder,
				segHeight = origionalHeight + segBorder;
			int[][] doubleArrayToReturn = new int[segWidth][];
			for (int i = 0; i < segWidth; i++) {
				doubleArrayToReturn[i] = new int[segHeight];
			}
			for (int i = 0; i < segWidth; i++) {
				for (int j = 0; j < segHeight; j++) {
					doubleArrayToReturn[i][j] = 255;
				}
			}
			int correctedX, correctedY;
			for (int i = 0; i < origionalWidth; i++) {
				for (int j = 0; j < origionalHeight; j++) {
					correctedX = i + (segBorder / 2);
					correctedY = j + (segBorder / 2);
					doubleArrayToReturn[correctedX][correctedY] = doubleArray[i][j];
				}
			}
			seg.InternalPoints = doubleArrayToReturn;
			return seg;
		}
		int counter = 0;
		public IEnumerable<OCRSegment> DefineSegments() {
			int charWidth = 2; //Assumed, average char width 
			calculateRowAndColumnSums();
			
			defineLineBreaks();
			for (int i = 0; i < allTextLines.Count; i++) {
				allTextLines[i].LinearScan(uploadedDocument);
				int numOfBreaks = allTextLines[i].ScanSum.Count / charWidth;
				allTextLines[i].DefineSegmentBreaks(numOfBreaks);
				int maxSegWidth = 32;
				foreach (OCRSegment seg in allTextLines[i].BuildSegments(maxSegWidth, uploadedDocument)) {
					addBorder(seg);
					yield return seg;
				}
			}
		}

		private int determineTopLine() {
			for (int i = 0; i < allRows.Count(); i++) {
				singleRow row = allRows[i];
				if (row.RowSum > 0) {
					return i;
				}
			}
			throw new Exception("No text found");
		}

		private int determineLeftEdge() {
			int lowestRowStart = int.MaxValue;
			for (int i = 0; i < allRows.Count(); i++) {
				if (allRows[i].textStart < lowestRowStart && allRows[i].textStart != int.MinValue) {
					lowestRowStart = allRows[i].textStart;
				}
			}
			if (lowestRowStart < int.MaxValue)
				return lowestRowStart;
			else
				throw new Exception("No border found");
		}

		private int determineRightEdge() {
			int highestRowEnd = int.MinValue;
			for (int i = 0; i < allRows.Count(); i++) {
				if (allRows[i].textEnd > highestRowEnd) {
					highestRowEnd = allRows[i].textEnd;
				}
			}
			if (highestRowEnd > int.MinValue)
				return highestRowEnd;
			else
				throw new Exception("No border found");
		}

		public IEnumerable<OCRSegment> DefineSegments2() {
			calculateRowAndColumnSums();
			int topLine = determineTopLine();
			int leftEdge = determineLeftEdge();
			int rightEdge = determineRightEdge();
			int subSegWidth = 0;
			OCRSegment segToReturn = new OCRSegment();
			int lineHeight = 9;
			int[][] segmentToReturn;

			for (int leftPointer = leftEdge; leftPointer < rightEdge; leftPointer++) {
				for (int rightPointer = leftPointer + 3; rightPointer < leftPointer + 10 && rightPointer < rightEdge; rightPointer++) {
					subSegWidth = rightPointer - leftPointer; 
					if (subSegWidth < 3) throw new Exception("width too small");
					segmentToReturn = new int[subSegWidth][];
					for (int i = 0; i < subSegWidth; i++)
						segmentToReturn[i] = new int[lineHeight];
					for (int i = 0; i < subSegWidth; i++) {
						for (int j = 0; j < lineHeight; j++) {
							int xRenderIdx = i + leftPointer;
							int yRenderIdx = j + topLine;
							segmentToReturn[i][j] = uploadedDocument[xRenderIdx][yRenderIdx];
						}
					}
					segToReturn.InternalPoints = segmentToReturn;
					segToReturn.SegmentLocation = new Rectangle(leftPointer, topLine, subSegWidth, lineHeight);
					yield return segToReturn;
				}
			}
		}

		#region UI event
		private void sendSegmentToUI(int[][] content, Rectangle location) {
			Bitmap bitmap = DisplayUtility.ConvertDoubleArrayToBitmap(content, Color.White);
			OnDisplaySegment(new DisplaySegEventArgs(bitmap, location));
		}

		public static event EventHandler<DisplaySegEventArgs> DisplaySegment;
		void OnDisplaySegment(DisplaySegEventArgs e) {
			var copy = DisplaySegment;
			if (copy != null) copy(this, e);
		}
		#endregion
	}
	static class MyColor {
		static public int Offest(int color) {
			return 255 - color;
		}
	}
}
