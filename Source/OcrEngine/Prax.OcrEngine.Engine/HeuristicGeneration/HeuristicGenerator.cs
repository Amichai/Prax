using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Engine.HeuristicGeneration {
	///<summary>Converts data arrays into PR heuristics.</summary>
	public abstract class HeuristicGenerator {
		protected HeuristicGenerator(int[][] data) { OriginalBoard = new DataMatrix(data); }

		///<summary>Gets the original data that will be converted.</summary>
		public DataMatrix OriginalBoard { get; private set; }
		///<summary>Gets the converted data as of the current iteration.</summary>
		///<remarks>This is set by the BuildData method.</remarks>
		public DataMatrix IteratedBoard { get; protected set; }

		public virtual int ConsolidationConstant { get { return 3; } }

		private const int iterationCount = 50;

		private List<int> iterationResults;
		public ReadOnlyCollection<int> BuildData() {
			iterationResults = new List<int>(OriginalBoard.Area * iterationCount);

			for (int i = 0; i < iterationCount; i++) {
				IteratedBoard = new DataMatrix(this.IterateBoard());

				//A new set of heuristics is added after each iteration
				iterationResults.AddRange(IteratedBoard.ToList());
			}

			return new ReadOnlyCollection<int>(iterationResults);
		}
		#region Base Logic
		///<summary>Processes the data.</summary>
		///<returns>The new data after the current iteration.</returns>
		protected virtual int[][] IterateBoard() {
			int width = IteratedBoard.Width;
			int height = IteratedBoard.Height;
			int averageSurroundingDiscrepPxls;
			int rangeOfSurroundingPxls, rangeOfSurroundingDiscrepPxls;
			int[] surroundingPxls = new int[4];
			int[] surroundingDiscrepancyPxls = new int[4];

			int[][] newBoard = new int[width][];
			for (int i = 0; i < width; i++) {
				newBoard[i] = new int[height];
			}

			for (int i = 1; i < width - 1; i++) {
				for (int j = 1; j < height - 1; j++) {
					surroundingPxls[0] = IteratedBoard.Data[i - 1][j];
					surroundingPxls[1] = IteratedBoard.Data[i][j - 1];
					surroundingPxls[2] = IteratedBoard.Data[i + 1][j];
					surroundingPxls[3] = IteratedBoard.Data[i][j + 1];

					for (int k = 0; k < 4; k++)
						surroundingDiscrepancyPxls[k] = Math.Abs(IteratedBoard.Data[i][j] - surroundingPxls[k]);

					averageSurroundingDiscrepPxls = (int)surroundingDiscrepancyPxls.Average();	//TODO: Are you sure you want to truncate?
					rangeOfSurroundingDiscrepPxls = surroundingDiscrepancyPxls.Max() - surroundingDiscrepancyPxls.Min();
					rangeOfSurroundingPxls = surroundingPxls.Max() - surroundingPxls.Min();

					newBoard[i][j] = ((averageSurroundingDiscrepPxls * rangeOfSurroundingPxls) / ((rangeOfSurroundingDiscrepPxls / 4) + 1)) / ConsolidationConstant;
				}
			}
			return newBoard;
		}

		private int GetIteratedCell(int iterationIndex, int x, int y) {
			return iterationResults[IteratedBoard.Area * iterationIndex
								  + IteratedBoard.Height * x
								  + y];
		}
		private IEnumerable<int> SumQuadrants() {
			//Sum up the value of all the quadrants, add them and divide them into each-other
			for (int boardIdx = 0; boardIdx < iterationCount; boardIdx++) {
				int[] quadrantSum = new int[4];
				for (int x = 0; x < IteratedBoard.Width / 2; x++)
					for (int y = 0; y < IteratedBoard.Height / 2; y++)
						quadrantSum[0] += GetIteratedCell(boardIdx, x, y);

				for (int x = IteratedBoard.Width / 2; x < IteratedBoard.Width; x++)
					for (int y = 0; y < IteratedBoard.Height / 2; y++)
						quadrantSum[1] += GetIteratedCell(boardIdx, x, y);

				for (int x = 0; x < IteratedBoard.Width; x++)
					for (int y = IteratedBoard.Height / 2; y < IteratedBoard.Height; y++)
						quadrantSum[2] += GetIteratedCell(boardIdx, x, y);

				for (int x = IteratedBoard.Width / 2; x < IteratedBoard.Width; x++)
					for (int y = IteratedBoard.Height / 2; y < IteratedBoard.Height; y++)
						quadrantSum[3] += GetIteratedCell(boardIdx, x, y);

				for (int i = 0; i < 4; i++) {
					if (quadrantSum[i] == 0)
						quadrantSum[i] = 1;

					yield return quadrantSum[i];
				}

				for (int i = 0; i < 4; i++) {
					for (int j = 0; j < 4; j++) {
						if (i != j)
							yield return quadrantSum[i] * 4 / quadrantSum[j];
					}
				}

				yield return (quadrantSum[3] + quadrantSum[2] + quadrantSum[1]) / quadrantSum[0];
				yield return (quadrantSum[3] + quadrantSum[2] + quadrantSum[0]) / quadrantSum[1];
				yield return (quadrantSum[3] + quadrantSum[0] + quadrantSum[1]) / quadrantSum[2];
				yield return (quadrantSum[0] + quadrantSum[2] + quadrantSum[1]) / quadrantSum[3];

				yield return 4 * (quadrantSum[0] + quadrantSum[2]) / (quadrantSum[1] + quadrantSum[3]);
				yield return 4 * (quadrantSum[0] + quadrantSum[1]) / (quadrantSum[2] + quadrantSum[3]);
				yield return 4 * (quadrantSum[0] + quadrantSum[3]) / (quadrantSum[1] + quadrantSum[2]);
				yield return 4 * (quadrantSum[1] + quadrantSum[2]) / (quadrantSum[0] + quadrantSum[3]);
				yield return 4 * (quadrantSum[3] + quadrantSum[1]) / (quadrantSum[2] + quadrantSum[0]);
				yield return 4 * (quadrantSum[2] + quadrantSum[3]) / (quadrantSum[1] + quadrantSum[0]);

				//Add many many more here
			}
		}
		private IEnumerable<int> CalcCenterPixels() {
			//CENTER PIXELS HEURISTICS
			for (int boardIdx = 0; boardIdx < iterationCount; boardIdx++) {
				int sumOfCenterPixels = 0, productOfCenterPixels = 1;
				for (int y = 0; y < IteratedBoard.Height; y++) {
					sumOfCenterPixels += GetIteratedCell(boardIdx, IteratedBoard.Width / 2 - 1, y);
					sumOfCenterPixels += GetIteratedCell(boardIdx, IteratedBoard.Width / 2 + 0, y);
					sumOfCenterPixels += GetIteratedCell(boardIdx, IteratedBoard.Width / 2 + 1, y);

					var center = GetIteratedCell(boardIdx, IteratedBoard.Width / 2, y);
					if (center != 0)
						productOfCenterPixels *= center;
				}

				yield return sumOfCenterPixels;
				yield return productOfCenterPixels;
				yield return sumOfCenterPixels / IteratedBoard.Height;
				yield return productOfCenterPixels / IteratedBoard.Height;

				if (sumOfCenterPixels != 0)
					yield return productOfCenterPixels / sumOfCenterPixels;
				else
					yield return 0;
			}
		}

		private IEnumerable<int> PixelSumVariations() {
			//VARIATION TEMPLATE MEASURED WITH RESPECT TO SIDE PIXELS AND ANGLE PIXELS

			int[] eightSurroundingPixels = new int[4];

			const int baseThreeOffset = 1 * 2 + 3 * 2 + 9 * 2 + 27 * 2;

			for (int boardIdx = 0; boardIdx < iterationCount; boardIdx++) {
				var pixelSums = new int[baseThreeOffset];

				for (int x = 1; x < IteratedBoard.Width - 1; x++) {
					for (int y = 1; y < IteratedBoard.Height - 1; y++) {
						eightSurroundingPixels[0] = GetIteratedCell(boardIdx, x - 1, y + 1);
						eightSurroundingPixels[1] = GetIteratedCell(boardIdx, x + 1, y + 1);
						eightSurroundingPixels[2] = GetIteratedCell(boardIdx, x - 1, y - 1);
						eightSurroundingPixels[3] = GetIteratedCell(boardIdx, x + 1, y - 1);

						int currentPixel = GetIteratedCell(boardIdx, x, y);
						int pixelTemplateSum = 0;

						for (int k = 0; k < 4; k++) {
							if (eightSurroundingPixels[k] == currentPixel)
								pixelTemplateSum += (int)(Math.Pow(k, 3) * 0.0);
							if (eightSurroundingPixels[k] > currentPixel)
								pixelTemplateSum += (int)(Math.Pow(k, 3) * 1.0);
							if (eightSurroundingPixels[k] < currentPixel)
								pixelTemplateSum += (int)(Math.Pow(k, 3) * 2);
						}
						pixelSums[pixelTemplateSum]++;
					}
				}

				for (int i = 0; i < baseThreeOffset; i++)
					yield return pixelSums[i];

				pixelSums = new int[baseThreeOffset];
				for (int x = 1; x < IteratedBoard.Width - 1; x++) {
					for (int y = 1; y < IteratedBoard.Height - 1; y++) {
						eightSurroundingPixels[0] = GetIteratedCell(boardIdx, x, y + 1);
						eightSurroundingPixels[1] = GetIteratedCell(boardIdx, x + 1, y);
						eightSurroundingPixels[2] = GetIteratedCell(boardIdx, x, y - 1);
						eightSurroundingPixels[3] = GetIteratedCell(boardIdx, x - 1, y);

						int currentPixel = GetIteratedCell(boardIdx, x, y);
						int pixelTemplateSum = 0;
						for (int k = 0; k < 4; k++) {
							if (eightSurroundingPixels[k] == currentPixel)
								pixelTemplateSum += (int)(Math.Pow(k, 3) * 0.0);
							if (eightSurroundingPixels[k] > currentPixel)
								pixelTemplateSum += (int)(Math.Pow(k, 3) * 1.0);
							if (eightSurroundingPixels[k] < currentPixel)
								pixelTemplateSum += (int)(Math.Pow(k, 3) * 2);
						}
						pixelSums[pixelTemplateSum]++;
					}
				}

				for (int i = 0; i < baseThreeOffset; i++)
					yield return pixelSums[i];
			}
		}

		private IEnumerable<int> BoardSums() {
			yield return OriginalBoard.Data.Sum(a => a.Sum());
			for (int x = 0; x < IteratedBoard.Width; x++) {
				for (int y = 0; y < IteratedBoard.Height; y++) {
					yield return OriginalBoard.Data[x][y];
				}
			}

			//Sum up the value of every board across all iterated boards
			for (int i = 0; i < iterationCount; i++) {
				var sum = 0;

				for (int x = 0; x < IteratedBoard.Width; x++)
					for (int y = 0; y < IteratedBoard.Height; y++)
						sum += GetIteratedCell(i, x, y);

				yield return sum;
			}
		}
		private IEnumerable<int> AspectRatio() {
			double widthHeightRatio = (double)IteratedBoard.Width / (double)IteratedBoard.Height;

			int power = 1;
			for (int decimalIdx = 0; decimalIdx < 10; decimalIdx++) {
				yield return (int)(decimalIdx * power);
				power *= 10;
			}
		}

		private void generateHeuristics() {
			int sizeOfHeuristicArray = 1550;
			var heuristics = new List<int>(sizeOfHeuristicArray);

			heuristics.AddRange(SumQuadrants());

			heuristics.AddRange(CalcCenterPixels());

			//CATEGORIZE EVERY PIXEL ACCORDING TO ITS COMPLEXITY AND COLOR RANKING (HISTOGRAM)

			heuristics.AddRange(PixelSumVariations());

			heuristics.AddRange(BoardSums());

			heuristics.AddRange(AspectRatio());

			//-GLOBAL- HEURISTIC PROPERTIES
			heuristics.Add(IteratedBoard.Height + IteratedBoard.Width);

			int sumOfAllHeuristics = heuristics.Sum();	//TODO: .Take(sizeOfHeuristicArray-2)?

			heuristics.Add(sumOfAllHeuristics);
			heuristics.Add(sumOfAllHeuristics / sizeOfHeuristicArray);	//TODO: Use .Count - 2?


			//global properties for each one of the iterated boards
			//each heuristic represents an orientation of relative pixel colors, the number of each heuristic is the amount of the those templates
			//Sum up all the heurisitics for each board, take the average, all the boards together
		}
		#endregion

		//TODO: Visualization methods
	}


	//class LetterRecognition : HeuristicGenerator {
	//    public LetterRecognition(int[][] data)
	//        : base(data) {
	//        this.IteratedBoard = this.OriginalBoard;
	//    }
	//}
}
