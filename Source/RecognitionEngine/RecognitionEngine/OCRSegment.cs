﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Prax.Recognition
{
    class OCRSegment
    {
        public Rectangle SegmentLocation;
        public bool ThisSegmentIsAWord;
        public int[][] InternalPoints;
    }
}