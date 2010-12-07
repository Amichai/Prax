using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Prax.Recognition
{
    class TrainingData
    {
        public List<Tuple<string, List<int>>> trainingLibrary { get; private set; }
        //TODO: remove the frist redundant string. Also possibly build indicies when reading
        public List<List<int>> listOfIndicies { get; private set; }
        public List<string> listOfIndexLabels { get; private set; }
        //TODO: make these DS private and expose the data as functions (as few as possible)

        public TrainingData(TrainingDataOptions openOptions) {
            int piecesOfTrainingData = 0;
            string fileName = string.Empty;
                
            fileName = "TrainingData.dat";

            if (File.Exists(fileName) && openOptions != TrainingDataOptions.reset){
                FileStream openTrainingData = new FileStream(fileName, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();

                trainingLibrary = (List<Tuple<string, List<int>>>)formatter.Deserialize(openTrainingData);
                listOfIndicies = (List<List<int>>)formatter.Deserialize(openTrainingData);
                listOfIndexLabels = (List<string>)formatter.Deserialize(openTrainingData);

                openTrainingData.Close();
                piecesOfTrainingData = trainingLibrary.Count;
            }
            if (openOptions == TrainingDataOptions.reset) {
                File.Delete(fileName);
            }
            if(!File.Exists(fileName)){
                trainingLibrary = new List<Tuple<string, List<int>>>();
                listOfIndicies = new List<List<int>>();
                listOfIndexLabels = new List<string>();
            }
        }
    }
}
