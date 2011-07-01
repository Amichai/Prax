using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Prax.Recognition;

namespace ExtractedOCRFunctionality {
	public static class Segmentation {
		public static const int WidthOfCanvas = 11;
		public static HeuristicReturnValues Segment(this IteratedBoards boards) {
			HeuristicReturnValues heuristics = new HeuristicReturnValues();
			int width = boards.Boards.First().Matrix.Length;
			int height = boards.Boards.First().Matrix[0].Length;
			for (int idx = 0; idx < width - WidthOfCanvas; idx++) {
				heuristics.GoThroughBoards(boards.Boards, new Rectangle(idx, 0, Segmentation.WidthOfCanvas, height));
			}
			return heuristics;
		}
	}
}
