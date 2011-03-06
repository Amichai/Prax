using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Engine.Tests.HeuristicGeneration {
	class LegacyHeuristics {
		int width, height;
		int[][] currentSegment;

		public List<int> trainTestPreprocess(int[][] segment) {
			width = segment.GetLength(0);
			height = segment[0].GetLength(0);
			currentSegment = segment;
			iterateBitmap();
			generateHeuristics();

			return heuristicArray;
		}

		private const int numberOfIterations = 8;

		private int[][][] setOfIteratedBoards { get; set; }

		private int boardIterationCounter;

		private void initializeIteratedBoards() {
			setOfIteratedBoards = new int[numberOfIterations][][];
			for (int i = 0; i < numberOfIterations; i++) {
				setOfIteratedBoards[i] = new int[width][];
				for (int j = 0; j < width; j++) {
					setOfIteratedBoards[i][j] = new int[height];
				}
			}
			boardIterationCounter = 0;
		}

		private int[][] iterateMe;

		private void iterate() {
			const int consolidationConstant = 3;

			int averageSurroundingDiscrepPxls;
			int rangeOfSurroundingPxls, rangeOfSurroundingDiscrepPxls;
			int[] surroundingPxls = new int[4];
			int[] surroundingDiscrepancyPxls = new int[4];

			if (boardIterationCounter == 0)
				iterateMe = currentSegment;

			for (int i = 1; i < width - 1; i++) {
				for (int j = 1; j < height - 1; j++) {
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

		private void iterateBitmap() {
			initializeIteratedBoards();
			for (int i = 0; i < numberOfIterations; i++)
				iterate();
		}

		private List<int> heuristicArray;
		private int sizeOfHeuristicArray;
		private void generateHeuristics() {
			sizeOfHeuristicArray = 1550;
			heuristicArray = new List<int>(sizeOfHeuristicArray);
			for (int i = 0; i < sizeOfHeuristicArray; i++)
				heuristicArray.Add(0);

			int currentIndex = 0;
			//Sum up the value of all the quadrants, add them and divide them into eachother

			for (int boardIdx = 0; boardIdx < numberOfIterations; boardIdx++) {
				int[] quadrantSum = new int[4];
				for (int i = 0; i < width / 2; i++) {
					for (int j = 0; j < height / 2; j++) {
						quadrantSum[0] += setOfIteratedBoards[boardIdx][i][j];
					}
				}
				for (int i = width / 2; i < width; i++) {
					for (int j = 0; j < height / 2; j++) {
						quadrantSum[1] += setOfIteratedBoards[boardIdx][i][j];
					}
				}
				for (int i = 0; i < width; i++) {
					for (int j = height / 2; j < height; j++) {
						quadrantSum[2] += setOfIteratedBoards[boardIdx][i][j];
					}
				}
				for (int i = width / 2; i < width; i++) {
					for (int j = height / 2; j < height; j++) {
						quadrantSum[3] += setOfIteratedBoards[boardIdx][i][j];
					}
				}
				for (int i = 0; i < 4; i++) {
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

			//currentIndex = 208

			//CENTER PIXELS HEURISTICS
			for (int boardIdx = 0; boardIdx < numberOfIterations; boardIdx++) {
				int sumOfCenterPixels = 0, productOfCenterPixels = 1;
				for (int j = 0; j < height; j++) {
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

			//currentIndex = 248

			int pixelTemplateSum;
			int currentPixel;
			int[] eightSurroundingPixels = new int[4];
			int baseThreeOffset = 80; //the sum of 1*2+3*2+9*2+27*2

			for (int boardIdx = 0; boardIdx < numberOfIterations; boardIdx++) {
				for (int i = 1; i < width - 1; i++) {
					for (int j = 1; j < height - 1; j++) {
						eightSurroundingPixels[0] = setOfIteratedBoards[boardIdx][i - 1][j + 1];
						eightSurroundingPixels[1] = setOfIteratedBoards[boardIdx][i + 1][j + 1];
						eightSurroundingPixels[2] = setOfIteratedBoards[boardIdx][i - 1][j - 1];
						eightSurroundingPixels[3] = setOfIteratedBoards[boardIdx][i + 1][j - 1];
						currentPixel = setOfIteratedBoards[boardIdx][i][j];
						pixelTemplateSum = 0;
						for (int k = 0; k < 4; k++) {
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
				for (int i = 1; i < width - 1; i++) {
					for (int j = 1; j < height - 1; j++) {
						eightSurroundingPixels[0] = setOfIteratedBoards[boardIdx][i][j + 1];
						eightSurroundingPixels[1] = setOfIteratedBoards[boardIdx][i + 1][j];
						eightSurroundingPixels[2] = setOfIteratedBoards[boardIdx][i][j - 1];
						eightSurroundingPixels[3] = setOfIteratedBoards[boardIdx][i - 1][j];

						currentPixel = setOfIteratedBoards[boardIdx][i][j];
						pixelTemplateSum = 0;
						for (int k = 0; k < 4; k++) {
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

			//currentIndex = 1528

			//Sum up the value of every board across all iterated boards
			int[] sumOfBoards = new int[numberOfIterations + 1];
			for (int i = 0; i < width; i++) {
				for (int j = 0; j < height; j++) {
					sumOfBoards[0] += currentSegment[i][j]; //questionably legal
					for (int k = 0; k < numberOfIterations; k++) {
						sumOfBoards[k + 1] += setOfIteratedBoards[k][i][j];
					}
				}
			}

			for (int k = 0; k < numberOfIterations + 1; k++) {
				heuristicArray[currentIndex + k] = sumOfBoards[k];
			}
			currentIndex += numberOfIterations + 1;

			//-GLOBAL- HEURISTIC PROPERTIES
			double widthHeightRatio = (double)width / (double)height;
			for (int decimalIdx = 0; decimalIdx < 10; decimalIdx++) {
				heuristicArray[currentIndex] = (int)(widthHeightRatio * Math.Pow(10, decimalIdx));
				currentIndex++;
			}
			heuristicArray[currentIndex] = height + width;
			currentIndex++;
			int sumOfAllHeuristics = 0;
			for (int i = 0; i < sizeOfHeuristicArray - 2; i++) {
				sumOfAllHeuristics += heuristicArray[i];
			}
			heuristicArray[currentIndex] = sumOfAllHeuristics;
			currentIndex++;
			heuristicArray[currentIndex] = sumOfAllHeuristics / sizeOfHeuristicArray;


			//global properties for each one of the iterated boards
			//each heuristic represents an orientation of relative pixel colors, the number of each heuristic is the amount of the those templates
			//Sum up all the heurisitics for each board, take the average, all the boards together
		}

	}
}
