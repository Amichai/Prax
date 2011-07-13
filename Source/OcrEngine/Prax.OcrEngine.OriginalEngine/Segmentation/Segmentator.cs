using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Prax.OcrEngine.Engine.HeuristicGeneration;

namespace Prax.OcrEngine.Engine.Segmentation {
	public static class Segmentator {
		public const int WidthOfCanvas = 15;
		public const int HeightOfCanvas = 15;
		public static int PointerOffset = (int)Math.Floor(WidthOfCanvas / 2d);

		public static IEnumerable<HeuristicSet> Segment(this IterateBoards boards) {
			int width = boards.Boards.First().Matrix.Length;
			int height = boards.Boards.First().Matrix[0].Length;
			for (int idx = 0; idx < width - WidthOfCanvas; idx++) {
				var heuristics = new HeuristicSet { Bounds = new Rectangle(idx, 0, Segmentator.WidthOfCanvas, Segmentator.HeightOfCanvas) };
				heuristics.GoThroughBoards(boards.Boards, heuristics.Bounds);
				yield return heuristics;
			}
		}
	}
}
