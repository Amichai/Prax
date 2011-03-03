using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Engine.PRProblems {
    class ProblemBoard {
        public int[][] Board;
        public int Width, Height;
        public int Area;
        public ProblemBoard(int[][] board) {
            this.Board = board;
            this.Width = board.Length;
            this.Height = board[0].Length;
            this.Area = this.Width * this.Height;
        }
        public List<int> BoardToList() {
            List<int> listToReturn = new List<int>(Area);
            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height; j++) {
                    listToReturn.Add(Board[i][j]);
                }
            }
            return listToReturn;
        }
    }
}
