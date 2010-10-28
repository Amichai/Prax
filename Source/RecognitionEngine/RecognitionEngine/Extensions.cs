using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Prax.Recognition
{
    static class Extensions
    {
        ///<summary>Measures the exact size of a string.</summary>
        ///<param name="graphics">A Graphics object to measure the string on.</param>
        ///<param name="text">The string to measure.</param>
        ///<param name="font">The font used to draw the string.</param>
        ///<returns>The exact width of the string in pixels.</returns>
        public static SizeF MeasureStringSize(this Graphics graphics, string text, Font font)
        {
            if (graphics == null) throw new ArgumentNullException("graphics");
            if (text == null) throw new ArgumentNullException("text");
            if (font == null) throw new ArgumentNullException("font");

            using (var format = new StringFormat())
            {
                format.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, text.Length) });

                using (var region = graphics.MeasureCharacterRanges(text, font, new Rectangle(0, 0, int.MaxValue / 2, int.MaxValue / 2), format)[0])
                {
                    return region.GetBounds(graphics).Size;
                }
            }
        }
    }
}
