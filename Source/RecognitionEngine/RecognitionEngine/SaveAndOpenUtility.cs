using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Formatters.Soap;

namespace Prax.Recognition {
    static class SaveAndOpenUtility {
        public const string RecognizedSegmentsName = "RecognizedSegments";

        public static void SaveRecognizedSegments(ReadOnlyCollection<RecognizedSegment> outputToSave) {
            FileStream saveOutput = new FileStream(RecognizedSegmentsName, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();

            try {
                formatter.Serialize(saveOutput, outputToSave);
            } catch (SerializationException e) {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            } finally {
                saveOutput.Close();
            }
        }

        public static ReadOnlyCollection<RecognizedSegment> OpenRecognizedSegments() {

            if (File.Exists(RecognizedSegmentsName)) {
                FileStream openRecognizedSegments = new FileStream(RecognizedSegmentsName, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();

                var open = (ReadOnlyCollection<RecognizedSegment>)formatter.Deserialize(openRecognizedSegments);
                return open;
            } else
                return null;
        }
    }
}
