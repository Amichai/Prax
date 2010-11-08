using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Prax.Recognition
{
    public class ReturnedSegment
    {
        public ReturnedSegment(string text, int xDiscrep, int yDiscrep, double overlapRatio)
        {
            Text = text;
            XDiscrep = xDiscrep;
            YDiscrep = yDiscrep;
            OverlapRatio = overlapRatio;
        }
        ///<summary>Gets the recognized text.</summary>
        public string Text { get; private set; }
        /// <summary>
        /// Gets the discrepancy between the rectangular segment and 
        /// tree structure segmentData containing the rendered text
        /// </summary>
        public int XDiscrep { get; private set; }
        /// <summary>
        /// Gets the discrepancy between the rectangular segment and 
        /// tree structure segmentData containing the rendered text
        /// </summary>
        public int YDiscrep { get; private set; }
        /// <summary>
        /// Gets the ratio of the segment width in the rectangular segment compared to
        /// the tree structure segment Data containing the rendered text
        /// </summary>
        public double OverlapRatio { get; private set; }
    }
}
