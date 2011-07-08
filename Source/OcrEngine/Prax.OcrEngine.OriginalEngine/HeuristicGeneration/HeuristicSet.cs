using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Prax.OcrEngine.Engine.ImageUtilities;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Engine.HeuristicGeneration {
	public class HeuristicSet {
		public HeuristicSet() { Heuristics = new ReadOnlyCollection<int>(writableHeuristics); }

		public ReadOnlyCollection<int> Heuristics { get; private set; }

		public string Label { get; set; }
		public Rectangle Bounds { get; set; }	//TODO: Set this property

		private List<int> writableHeuristics = new List<int>();
		public void AddHeursitics(int[][] board) {
			for (int i = 0; i < board.Length; i++) {
				for (int j = 0; j < board[0].Length; j++) {
					writableHeuristics.Add(board[i][j]);
				}
			}
		}
		public int GetAtIndex(int index) {
			return writableHeuristics[index];
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
