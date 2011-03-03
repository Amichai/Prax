using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Engine.PRProblems {
    
    public class ProblemBoard {
        public int[][] Board { get; private set; }
        public int Width  { get; private set; }
        public int Height { get; private set; }
        public int Area { get; private set; }

        public ProblemBoard(int[][] board) {
            this.Board = board;
            this.Width = board.Length;
            this.Height = board[0].Length;
            this.Area = this.Width * this.Height;
        }

        public List<int> BoardToList() {
            List<int> listToReturn = new List<int>(Area);
            listToReturn.AddRange(Board.SelectMany(a => a));
            return listToReturn;
        }
    }
}
