using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prax.Recognition;
using System.Drawing;

namespace ExtractedOCRFunctionality {
	public class IteratedBoards{
		public List<MatrixBoard> Boards = new List<MatrixBoard>();
		public const int numberOfIterations = 8;

		public TrainingData Train(CharacterBounds charBounds) {
			int midpoint;
			int boardWidth = Boards.First().Matrix.Length;
			foreach(var character in charBounds.items){
				midpoint = character.Item1.X + (int)Math.Round(character.Item1.Width / 2d);
				Rectangle rect = new Rectangle(midpoint - 6, 0, boardWidth, character.Item1.Height);
				HeuristicReturnValues heursitics = new HeuristicReturnValues();
				heursitics.GoThroughBoards(Boards, rect);
				TrainingData trainingData = new TrainingData();
			}
			//Find the midpoint of the charbounds
			//Use that midpoint to extract the heursitcis
			throw new NotImplementedException();
		}

		private HeuristicReturnValues ExtractHeursitics(int midpoint){
			HeuristicReturnValues heuristics = new HeuristicReturnValues();
			int idx = midpoint - 6; 
			int width = Boards.First().Matrix[0].Length;
			heuristics.GoThroughBoards(Boards, new Rectangle(idx, 0, Segmentation.WidthOfCanvas, width));
			return heuristics;
		}
	}


	public class MatrixBoard {
		public int[][] Matrix { get; set; }
		public Bitmap MatrixImg;
		private int width, height;
		public MatrixBoard(int [][] matrix) {
			this.Matrix = matrix;
			this.width = matrix.Length;
			this.height = matrix[0].Length;
			MatrixImg = GraphicsHelper.ConvertDoubleArrayToBitmap(this.Matrix, Color.White);
		}
		public MatrixBoard(int width, int height) {
			this.width = width;
			this.height = height;
			this.Matrix = new int[width][];
			for (int i = 0; i < width; i++)
				this.Matrix[i] = new int[height];
		}
		public MatrixBoard IterateBoard() {
			const int consolidationConstant = 3;
			MatrixBoard iteratedBoard = new MatrixBoard(width, height);
			int averageSurroundingDiscrepPxls;
			int rangeOfSurroundingPxls, rangeOfSurroundingDiscrepPxls;
			int[] surroundingPxls = new int[4];
			int[] surroundingDiscrepancyPxls = new int[4];

			for (int i = 1; i < width - 1; i++) {
				for (int j = 1; j < height - 1; j++) {
					surroundingPxls[0] = Matrix[i - 1][j];
					surroundingPxls[1] = Matrix[i][j - 1];
					surroundingPxls[2] = Matrix[i + 1][j];
					surroundingPxls[3] = Matrix[i][j + 1];

					for (int k = 0; k < 4; k++)
						surroundingDiscrepancyPxls[k] = Math.Abs(Matrix[i][j] - surroundingPxls[k]);
					averageSurroundingDiscrepPxls = (surroundingDiscrepancyPxls[0] + surroundingDiscrepancyPxls[1]
											+ surroundingDiscrepancyPxls[2] + surroundingDiscrepancyPxls[3]) / 4;
					rangeOfSurroundingDiscrepPxls = Math.Max(Math.Max(surroundingDiscrepancyPxls[0], surroundingDiscrepancyPxls[1]), Math.Max(surroundingDiscrepancyPxls[2], surroundingDiscrepancyPxls[3]))
											- Math.Min(Math.Min(surroundingDiscrepancyPxls[0], surroundingDiscrepancyPxls[1]), Math.Min(surroundingDiscrepancyPxls[2], surroundingDiscrepancyPxls[3]));
					rangeOfSurroundingPxls = Math.Max(Math.Max(surroundingPxls[0], surroundingPxls[1]), Math.Max(surroundingPxls[2], surroundingPxls[3]))
											- Math.Min(Math.Min(surroundingPxls[0], surroundingPxls[1]), Math.Min(surroundingPxls[2], surroundingPxls[3]));

					iteratedBoard.Matrix[i][j] = ((averageSurroundingDiscrepPxls * rangeOfSurroundingPxls) / ((rangeOfSurroundingDiscrepPxls / 4) + 1)) / consolidationConstant;
				}
			}
			iteratedBoard.MatrixImg = GraphicsHelper.ConvertDoubleArrayToBitmap(iteratedBoard.Matrix, Color.White);
			return iteratedBoard;
		}
		/// <summary>
		/// This returns a comparison value for two boards of equal size
		/// </summary>
		/// <returns></returns>
		public int CompareBoards(IteratedBoards compBoards){

			return int.MaxValue;
		}
	}
}