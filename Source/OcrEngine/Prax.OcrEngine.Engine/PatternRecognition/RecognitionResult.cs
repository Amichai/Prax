using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Engine.PatternRecognition {
    public struct RecognitionResult {
        public string Label { get; private set; }
        public double Certainty { get; private set; }
    }
}
