using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Prax.Recognition;

namespace ExtractedOCRFunctionality {
	public static class Segmentation {
		public static const int widthOfSubBoard = 11;
		public static Heuristics Segment(this IteratedBoards boards) {
			Heuristics heuristics = new Heuristics();
			int width = boards.Boards.First().Matrix.Length;
			int height = boards.Boards.First().Matrix[0].Length;
			for (int idx = 0; idx < width - widthOfSubBoard; idx++) {
				heuristics.GoThroughBoards(boards.Boards, new Rectangle(idx, 0, Segmentation.widthOfSubBoard, height));
			}
			return heuristics;
		}

		public class RecognitionCanvass {
			List<MatrixBoard> boards = new List<MatrixBoard>();
		}
	}
}
