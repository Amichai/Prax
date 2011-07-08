using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Prax.OcrEngine.Engine.ImageUtilities;

namespace Prax.OcrEngine.Engine.HeuristicGeneration {
	public class HeuristicSet {
		public int Count { get { return heuristics.Count; } }
		public string Label { get; set; }
		public Rectangle Bounds { get; set; }	//TODO: Set this property

		private List<int> heuristics = new List<int>();
		public void AddHeursitics(int[][] board) {
			for (int i = 0; i < board.Length; i++) {
				for (int j = 0; j < board[0].Length; j++) {
					heuristics.Add(board[i][j]);
				}
			}
		}
		public int GetAtIndex(int index) {
			return heuristics[index];
		}
		public void GoThroughBoards(List<MatrixBoard> boards, Rectangle rect) {
			Bounds = rect;
			foreach (var board in boards) {
				AddHeursitics(
					board.Matrix.ExtractRectangularContentArea(rect));
			}
		}
	}
}
