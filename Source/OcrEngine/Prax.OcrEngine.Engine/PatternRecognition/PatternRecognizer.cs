using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prax.OcrEngine.Engine.ReferenceData;

namespace Prax.OcrEngine.Engine.PatternRecognition {
   public class PatternRecognizer {
       public RecognitionResult Recognize(IReferenceSet set, int[] data);
    }
}
