using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Prax.OcrEngine.Engine.ReferenceData {
    public sealed class ReferenceItem {
        public string Label { get; private set; }
        public ReadOnlyCollection<int> Data { get; private set; }
    }
}
