using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Prax.OcrEngine.Engine.HeuristicGeneration;

namespace Prax.OcrEngine.Engine.Segmentation {
	public static class Segmentator {
		public const int WidthOfCanvas = 11;
		public static HeuristicReturnValues Segment(this IterateBoards boards) {
			var heuristics = new HeuristicReturnValues();
			int width = boards.Boards.First().Matrix.Length;
			int height = boards.Boards.First().Matrix[0].Length;
			for (int idx = 0; idx < width - WidthOfCanvas; idx++) {
				heuristics.GoThroughBoards(boards.Boards, new Rectangle(idx, 0, Segmentator.WidthOfCanvas, height));
			}
			return heuristics;
		}
	}
}
