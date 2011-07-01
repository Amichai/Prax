using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Prax.Recognition;

namespace ExtractedOCRFunctionality {
	public static class Segmentation {
		public static Heuristics Segment(this IteratedBoards boards){
			Heuristics segmentationData = new Heuristics();
			int width = boards.Boards.First().Matrix.Length;
			int height = boards.Boards.First().Matrix[0].Length;
			for (int idx = 0; idx < width; idx++) {
				foreach (var board in boards.Boards) {
					segmentationData.AddHeursitics(
						board.Matrix.ExtractRectangularContentArea(new Rectangle(idx, 0, 10, height)));
				
				}
			} 
			return segmentationData;
		}
	}

	public class Heuristics {
		private List<int> heuristics = new List<int>();
		public void AddHeursitics(int[][] board) {
			for (int i = 0; i < board.Length; i++) {
				for (int j = 0; j < board[0].Length; j++) {
					heuristics.Add(board[i][j]);
				}
			}
		}
	}

	public class RecognitionCanvass {
		List<MatrixBoard> boards = new List<MatrixBoard>();
	}
}
