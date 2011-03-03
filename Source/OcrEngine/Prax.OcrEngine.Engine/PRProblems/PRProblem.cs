using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Engine.PRProblems {
    public abstract class PRProblem {
        public ProblemBoard OrigionalBoard;
        public ProblemBoard IteratedBoard;
        public const int consolidationConstant = 3;
        public virtual int[][] IterateBoard();
        //Visualization methods
    }

    class WordRecognition : PRProblem { }

    class LetterRecognition : PRProblem {
        public LetterRecognition(int[][] board) {
            this.OrigionalBoard = new ProblemBoard(board);
            this.IteratedBoard = this.OrigionalBoard;
        }
        public int[][] IterateBoard() {
            int width = IteratedBoard.Width;
            int height = IteratedBoard.Height;
            int averageSurroundingDiscrepPxls;
            int rangeOfSurroundingPxls, rangeOfSurroundingDiscrepPxls;
            int[] surroundingPxls = new int[4];
            int[] surroundingDiscrepancyPxls = new int[4];

            int[][] newBoard = new int[width][];
            for(int i=0;i < width; i++){
                newBoard[i] = new int[height];
            }

            for (int i = 1; i < width - 1; i++) {
                for (int j = 1; j < height - 1; j++) {
                    surroundingPxls[0] = IteratedBoard.Board[i - 1][j];
                    surroundingPxls[1] = IteratedBoard.Board[i][j - 1];
                    surroundingPxls[2] = IteratedBoard.Board[i + 1][j];
                    surroundingPxls[3] = IteratedBoard.Board[i][j + 1];

                    for (int k = 0; k < 4; k++)
                        surroundingDiscrepancyPxls[k] = Math.Abs(IteratedBoard.Board[i][j] - surroundingPxls[k]);
                    averageSurroundingDiscrepPxls = (surroundingDiscrepancyPxls[0] + surroundingDiscrepancyPxls[1]
                                            + surroundingDiscrepancyPxls[2] + surroundingDiscrepancyPxls[3]) / 4;
                    rangeOfSurroundingDiscrepPxls = Math.Max(Math.Max(surroundingDiscrepancyPxls[0], surroundingDiscrepancyPxls[1]), Math.Max(surroundingDiscrepancyPxls[2], surroundingDiscrepancyPxls[3]))
                                            - Math.Min(Math.Min(surroundingDiscrepancyPxls[0], surroundingDiscrepancyPxls[1]), Math.Min(surroundingDiscrepancyPxls[2], surroundingDiscrepancyPxls[3]));
                    rangeOfSurroundingPxls = Math.Max(Math.Max(surroundingPxls[0], surroundingPxls[1]), Math.Max(surroundingPxls[2], surroundingPxls[3]))
                                            - Math.Min(Math.Min(surroundingPxls[0], surroundingPxls[1]), Math.Min(surroundingPxls[2], surroundingPxls[3]));

                    newBoard[i][j] = ((averageSurroundingDiscrepPxls * rangeOfSurroundingPxls) / ((rangeOfSurroundingDiscrepPxls / 4) + 1)) / consolidationConstant;
                }
            }
            IteratedBoard = new ProblemBoard(newBoard);
            return newBoard;
        }
    }

    class WhitespaceRecognition : PRProblem { }
}
