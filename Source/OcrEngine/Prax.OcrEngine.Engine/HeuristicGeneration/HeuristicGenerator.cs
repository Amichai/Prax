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

		public ReadOnlyCollection<int> BuildData() {
			List<int> heuristics = new List<int>(OriginalBoard.Area * iterationCount);

			for (int i = 0; i < iterationCount; i++) {
				IteratedBoard = new DataMatrix(this.IterateBoard());

				//A new set of heuristics is added after each iteration
				heuristics.AddRange(IteratedBoard.ToList());
			}
			return new ReadOnlyCollection<int>(heuristics);
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
