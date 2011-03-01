using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prax.OcrEngine.Engine.ReferenceData {
    public interface  IReferenceLibrary :IReferenceSet{
        void AppendData(IEnumerable<ReferenceItem> items);
        void RemoveData(IEnumerable<ReferenceItem> items);
        void Clear();
    }
}
