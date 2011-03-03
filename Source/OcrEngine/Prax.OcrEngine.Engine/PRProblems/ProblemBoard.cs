using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Engine.HeuristicGeneration {
    
    public class DataMatrix {
        public int[][] Data { get; private set; }
        public int Width  { get; private set; }
        public int Height { get; private set; }
        public int Area { get; private set; }

        public DataMatrix(int[][] data) {
            this.Data = data;
            this.Width = data.Length;
            this.Height = data[0].Length;
            this.Area = this.Width * this.Height;
        }

        public List<int> ToList() {
            List<int> listToReturn = new List<int>(Area);
            listToReturn.AddRange(Data.SelectMany(a => a));
            return listToReturn;
        }
    }
}
