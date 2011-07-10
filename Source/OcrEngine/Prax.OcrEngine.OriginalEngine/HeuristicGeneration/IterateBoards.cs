using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using Prax.OcrEngine.Engine.AutomatedTraining;
using Prax.OcrEngine.Engine.ImageUtilities;
using Prax.OcrEngine.Engine.Segmentation;
using Prax.OcrEngine.Engine.ReferenceData;

namespace Prax.OcrEngine.Engine.HeuristicGeneration {
	public class IterateBoards {
		public int BoardWidth { get { return Boards.First().Matrix.Length; } }

		public List<MatrixBoard> Boards = new List<MatrixBoard>();
		public const int numberOfIterations = 8;

		public HeuristicSet GetHeuristics(BoundedCharacter ch) {
				var rect = ch.Bounds.ToGdi();

			int midpoint = rect.X + (int)Math.Round(rect.Width / 2d);
			if (midpoint - 6 >= 0 && midpoint + 6 < BoardWidth) {
					Rectangle smallerRect = new Rectangle(midpoint - 6, 0, Segmentator.WidthOfCanvas, rect.Height - 1);

				HeuristicSet heursitics = new HeuristicSet { Bounds = rect, Label = ch.ToString() };
					heursitics.GoThroughBoards(Boards, smallerRect);

				return heursitics;
				} else {
					//TODO: Handle smaller chars
				return null;
			}
		}

		private HeuristicSet ExtractHeursitics(int midpoint) {
			HeuristicSet heuristics = new HeuristicSet();
			int idx = midpoint - 6;
			int width = Boards.First().Matrix[0].Length;
			heuristics.GoThroughBoards(Boards, new Rectangle(idx, 0, Segmentation.Segmentator.WidthOfCanvas, width));
			return heuristics;
		}
	}


	public class MatrixBoard {
		public int[][] Matrix { get; set; }
		public Bitmap MatrixImg;
		private int width, height;
		public MatrixBoard(int[][] matrix) {
			this.Matrix = matrix;
			this.width = matrix.Length;
			this.height = matrix[0].Length;
			MatrixImg = this.Matrix.ConvertDoubleArrayToBitmap(Color.White);
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
			iteratedBoard.MatrixImg = iteratedBoard.Matrix.ConvertDoubleArrayToBitmap(Color.White);
			return iteratedBoard;
		}
		/// <summary>
		/// This returns a comparison value for two boards of equal size
		/// </summary>
		/// <returns></returns>
		public int CompareBoards(IterateBoards compBoards) {

			return int.MaxValue;
		}
	}
}
