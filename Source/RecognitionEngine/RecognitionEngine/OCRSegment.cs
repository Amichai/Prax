using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Prax.Recognition
{
    public class OCRSegment
    {
        public Rectangle SegmentLocation;
        public bool IsAWord;
        public int[][] InternalPoints;
    }
}
