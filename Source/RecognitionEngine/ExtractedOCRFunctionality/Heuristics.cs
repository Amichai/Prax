using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prax.Recognition;
using System.Drawing;

namespace ExtractedOCRFunctionality {
	public class Heuristics {
		private List<int> heuristics = new List<int>();
		public void AddHeursitics(int[][] board) {
			for (int i = 0; i < board.Length; i++) {
				for (int j = 0; j < board[0].Length; j++) {
					heuristics.Add(board[i][j]);
				}
			}
		}
		public void GoThroughBoards(List<MatrixBoard> boards, Rectangle rect) {
			foreach (var board in boards) {
				AddHeursitics(
					board.Matrix.ExtractRectangularContentArea(rect));
			}

		}
	}
}
