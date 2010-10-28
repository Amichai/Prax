using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Prax.Recognition
{
    public struct RecognizedSegment
    {
        public RecognizedSegment(Rectangle bounds, string text, double certainty)
            : this()
        {
            Bounds = bounds;
            Text = text;
            Certainty = certainty;
        }

        ///<summary>Gets the area in the image that contains the string.</summary>
        public Rectangle Bounds { get; private set; }
        ///<summary>Gets the recognized text.</summary>
        public string Text { get; private set; }
        ///<summary>Gets the certainty of the recognition, between 0 and 1.</summary>
        public double Certainty { get; private set; }
    }
}
