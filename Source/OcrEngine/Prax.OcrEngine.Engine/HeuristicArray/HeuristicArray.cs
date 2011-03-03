using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prax.OcrEngine.Engine.ImageRecognition;
using Prax.OcrEngine.Engine.PRProblems;

namespace Prax.OcrEngine.Engine.HeuristicArray {
    public enum ProblemType { WordRecognition, LetterRecognition, WhitespaceRecognition };

    class HeuristicArray {
        public List<int> heuristics;
        public int SizeOfHeuristicArray;
        private int numberOfIterations = 50;
        /// <summary>The constructor takes an input and builds a corresponding 
        /// heuristic array.</summary>
        /// <remarks>Overload the constructor to handle input types other than int[][]
        /// </remarks>
        public HeuristicArray(PRProblem problem) {
            for (int i = 0; i < numberOfIterations; i++) {
                problem.IterateBoard();
                heuristics.AddRange(problem.IteratedBoard.BoardToList());
            }
            SizeOfHeuristicArray = problem.OrigionalBoard.Area * numberOfIterations;
        }
        //Needs visualization methods, optimization and heuristic skip methods.
        //IterationMethods
    }
}
