using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Prax.OcrEngine.Engine.ReferenceData {
   public interface IReferenceSet {
       IReferenceSet Subset(Expression<Func<string, bool>> query);

       public IEnumerable<ReferenceItem> Items { get; }
    }
}
