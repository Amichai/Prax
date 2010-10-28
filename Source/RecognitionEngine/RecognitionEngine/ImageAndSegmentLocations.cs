using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;

namespace Prax.Recognition
{
    class ImageAndSegmentLocations
    {
        private Tree segmentData = new Tree();
        public Bitmap TrainingImage = new Bitmap(1, 1);

        public ImageAndSegmentLocations()
        {

            string dataFileName = @"C:\Users\Amichai\Documents\doc.txt";
            string dataFontName = "Times New Roman";
            string dataSize = "20";
            string dataStyle = "".ToLower();

            int width, height;
            width = height = 0;

            FontStyle style = FontStyle.Regular;
            if (dataStyle.Contains("b"))
                style |= FontStyle.Bold;
            else if (dataStyle.Contains("i"))
                style |= FontStyle.Italic;
            using (var font = new Font(dataFontName, float.Parse(dataSize), style, GraphicsUnit.Pixel))
            {

                //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ar-AE");
                string text = string.Empty;
                StringCollection lines = new StringCollection();
                int maxCharacters = 0;
                string textDocument = string.Empty;
                using (StreamReader reader = new StreamReader(new FileStream(dataFileName, FileMode.Open, FileAccess.Read)))
                {
                    while (reader.Peek() != -1)
                    {
                        string line = reader.ReadLine();
                        lines.Add(line);
                        if (line.Length > maxCharacters)
                            maxCharacters = line.Length;
                    }
                }
                lines = GetAlteredLines(lines);
                foreach (string s in lines)
                {
                    text += s + Environment.NewLine;
                }
                double expansionFactor = (double)7 / 5; //Magic number to create a visual equality between the whole doc rendered at once and individual letters

                var size = TextRenderer.MeasureText(text, font);
                using (var image = new Bitmap(size.Width, size.Height))
                using (var objGraphics = Graphics.FromImage(image))
                {
                    objGraphics.Clear(Color.White);

                    DrawText(objGraphics, font, Brushes.Black, text, lines);

                    //objGraphics.DrawString(text, font, brush, new PointF(0, 0)); //This is to see how the output should render

                    TrainingImage = image;
                    image.Save("newimage.bmp", ImageFormat.Bmp);
                }
            }
        }

        private StringCollection GetAlteredLines(StringCollection document)
        {
            StringCollection lines = new StringCollection();

            foreach (string line in document)
            {
                string[] words = line.Split(new char[] { ' ' });

                string newline = string.Empty;
                bool bShortLine = false;
                foreach (string word in words)
                {
                    bShortLine = true;
                    if (newline.Length + word.Length <= 100)
                    {
                        bShortLine = true;
                        newline += " ";
                        newline += word;
                    }
                    else
                    {
                        bShortLine = false;
                        lines.Add(newline);
                        newline = word;
                    }
                }
                if (bShortLine)
                    lines.Add(newline);
            }

            return lines;
        }

        class Tree
        {
            public List<YCoordinate> YCoordinates = new List<YCoordinate>();
            public void AddNode(int xCoordinate, int yCoordinate, int width, string text)
            {
                YCoordinate existingCoordinate = GetExistingNode(yCoordinate);
                if (existingCoordinate == null)
                {
                    XCoordinate xcoord = new XCoordinate(xCoordinate);
                    xcoord.Texts[width] = text;
                    YCoordinate ycoord = new YCoordinate(yCoordinate);
                    ycoord.XCoordinates.Add(xcoord);
                    this.YCoordinates.Add(ycoord);
                }
                else
                {
                    existingCoordinate.AddNode(xCoordinate, width, text);
                }
            }

            private YCoordinate GetExistingNode(int yCoordinate)
            {
                foreach (YCoordinate coord in this.YCoordinates)
                {
                    if (coord.Value == yCoordinate)
                        return coord;
                }
                return null;
            }

            public string DetermineSegmentText(Rectangle bounds)
            {
                List<YCoordinate> bestYCoordinate = new List<YCoordinate>();
                int bestYDiff = int.MaxValue;

                foreach (YCoordinate yCoordinate in this.YCoordinates)
                {
                    if (Math.Abs(yCoordinate.Value - bounds.Y) == bestYDiff)
                    {
                        bestYCoordinate.Add(yCoordinate);
                    }
                    if (Math.Abs(yCoordinate.Value - bounds.Y) < 4)
                    {
                        bestYCoordinate.Add(yCoordinate);
                    }
                    if (Math.Abs(yCoordinate.Value - bounds.Y) < bestYDiff && Math.Abs(yCoordinate.Value - bounds.Y) >= 4)
                    {
                        bestYCoordinate = new List<YCoordinate>();
                        bestYCoordinate.Add(yCoordinate);
                        bestYDiff = Math.Abs(yCoordinate.Value - bounds.Y);
                    }
                }
                if (bestYCoordinate == null)
                    return null;
                List<XCoordinate> bestXCoordinate = new List<XCoordinate>();
                int bestXDiff = int.MaxValue;
                foreach (YCoordinate yCoordinate in bestYCoordinate)
                {
                    foreach (XCoordinate xCoordinate in yCoordinate.XCoordinates)
                    {
                        if (Math.Abs(xCoordinate.Value - bounds.X) == bestXDiff)
                        {
                            bestXCoordinate.Add(xCoordinate);
                        }
                        if (Math.Abs(xCoordinate.Value - bounds.X) < 4)
                        {
                            bestXCoordinate.Add(xCoordinate);
                        }
                        if (Math.Abs(xCoordinate.Value - bounds.X) < bestXDiff && Math.Abs(xCoordinate.Value - bounds.X) >= 4)
                        {
                            bestXCoordinate = new List<XCoordinate>();
                            bestXCoordinate.Add(xCoordinate);
                            bestXDiff = Math.Abs(xCoordinate.Value - bounds.X);
                        }
                    }
                }
                if (bestXCoordinate == null)
                    return null;
                string matchedWord = null;
                double bestOverlapRatio = int.MinValue;
                foreach (XCoordinate xCoordinate in bestXCoordinate)
                {
                    foreach (int i in xCoordinate.Texts.Keys)
                    {
                        double overlapRatio = 0;
                        if (i <= bounds.Width)
                            overlapRatio = (double)i / bounds.Width;
                        if (i > bounds.Width)
                            overlapRatio = (double)bounds.Width / i;
                        if (overlapRatio > bestOverlapRatio)
                        {
                            bestOverlapRatio = overlapRatio;
                            matchedWord = xCoordinate.Texts[i];
                        }
                        /*
                        Debug.Print("YDiff: " + bestYDiff.ToString());
                        Debug.Print("XDiff: " + bestXDiff.ToString());
                        Debug.Print("Overlap: " + bestOverlapRatio.ToString()); */
                    }
                }
                if (bestOverlapRatio > .81)
                    return matchedWord;
                else
                    return null;
            }
        }

        class YCoordinate
        {
            public void AddNode(int xCoordinate, int width, string text)
            {
                XCoordinate existingCoordinate = GetExistingNode(xCoordinate);
                if (existingCoordinate == null)
                {
                    XCoordinate xcoord = new XCoordinate(xCoordinate);
                    xcoord.Texts[width] = text;
                    this.XCoordinates.Add(xcoord);
                }
                else
                {
                    existingCoordinate.Texts[width] = text;
                }
            }

            private XCoordinate GetExistingNode(int xCoordinate)
            {
                foreach (XCoordinate coord in this.XCoordinates)
                {
                    if (coord.Value == xCoordinate)
                        return coord;
                }
                return null;
            }

            public YCoordinate(int value)
            {
                Value = value;
            }
            public int Value;
            public List<XCoordinate> XCoordinates = new List<XCoordinate>();
        }

        class XCoordinate
        {
            public XCoordinate(int value)
            {
                Value = value;
            }
            public int Value;
            public Dictionary<int, string> Texts = new Dictionary<int, string>();
        }

        private void DrawText(Graphics objGraphics, Font font, Brush brush, string text, StringCollection lines)
        {
            int width, height;

            width = (int)objGraphics.MeasureStringSize(text, font).Width;
            height = (int)objGraphics.MeasureStringSize(text, font).Height;
            Rectangle textBound = new Rectangle(0, 0, width, height);

            int charHorizontalGap, charVerticalGap;
            CalculateGap(objGraphics, font, out charHorizontalGap, out charVerticalGap);

            int yIndex = height / 4;
            int xOffset =  width / 4;
            int tempWidth = width;
            foreach (string line in lines)
            {
                //int xIndex = xOffset;
                
                int xIndex = tempWidth - xOffset;
                foreach (string word in line.Split(new char[] { ' ' }))
                {
                    int wordWidth = 0;
                    int wordXidx = xIndex;
                    for (int i = 0; i < word.Count(); i++)
                    {
                        char c = convertUnicodeChar(word, ref i);
                        //char c = word[i];
                        width = (int)objGraphics.MeasureStringSize(c.ToString(), font).Width;
                        width = width * 2 / 3;
                        height = (int)objGraphics.MeasureStringSize(c.ToString(), font).Height;
                        TextRenderer.DrawText(objGraphics, c.ToString(), font, new Point(xIndex, yIndex), Color.Black);
                        objGraphics.DrawString(c.ToString(), font, brush, new PointF(xIndex, yIndex));
                        //xIndex += width;
                        xIndex -= width;
                        wordWidth += width;
                        segmentData.AddNode(xIndex, yIndex, width, c.ToString());
                    }
                    objGraphics.DrawString(" ", font, brush, xIndex, yIndex);
                    width = (int)objGraphics.MeasureStringSize(" ", font).Width;
                    //xIndex += width;
                    xIndex -= width;
                    //segmentData.AddNode(wordXidx, yIndex, wordWidth, word);
                    segmentData.AddNode(xIndex, yIndex, wordWidth, word);
                }
                height = (int)objGraphics.MeasureStringSize(line, font).Height;
                yIndex += height;
            }
        }

        private void CalculateGap(Graphics objGraphics, Font font, out int charHorizontalGap, out int charVerticalGap)
        {
            int combinedWidth = (int)objGraphics.MeasureStringSize("ab", font).Width;
            int aWidth = (int)objGraphics.MeasureStringSize("a", font).Width;
            int bWidth = (int)objGraphics.MeasureStringSize("b", font).Width;

            charHorizontalGap = combinedWidth - aWidth - bWidth;
            int combinedHeight = (int)objGraphics.MeasureStringSize("a" + Environment.NewLine + "b", font).Height;
            int aHeight = (int)objGraphics.MeasureStringSize("a", font).Height;
            int bHeight = (int)objGraphics.MeasureStringSize("b", font).Height;
            charVerticalGap = combinedHeight - aHeight - bHeight;
        }

        public string LabelAtThisSegmentLocation(Rectangle segmentLocation)
        {
            //TODO: test to see if the segment under inspection matches a defined label
            return segmentData.DetermineSegmentText(segmentLocation);
        }

        #region letter conversion
        private enum letterPosition
        {
            start, middle, end, isolated
        };

        private char testForLigatures(int currentChar, int nextChar)
        {
            if (currentChar == 1604 && nextChar == 1570)
            {
                return (char)65269;
            }
            if (currentChar == 1604 && nextChar == 1571)
            {
                return (char)65271;
            }
            if (currentChar == 1604 && nextChar == 1573)
            {
                return (char)65273;
            }
            if (currentChar == 1604 && nextChar == 1575)
            {
                return (char)65275;
            }
            return char.MinValue;
        }
        static private int[] RF = new int[] { 1570, 1571, 1573, 1575, 1577, 1583, 1584, 1585, 1586, 1608, 1609 };
        private HashSet<int> restrictedForms = new HashSet<int>(RF);

        private enum arabicLetterForms
        {
            restricted, unrestricted
        };

        private arabicLetterForms previousForm = arabicLetterForms.restricted;

        private char convertUnicodeChar(string word, ref int idx)
        {
            int currentChar = word[idx];
            arabicLetterForms currentForm;
            if (restrictedForms.Contains(currentChar))
                currentForm = arabicLetterForms.restricted;
            else
                currentForm = arabicLetterForms.unrestricted;

            letterPosition currentPosition = letterPosition.middle;

            if (idx == word.Count() - 1)
                currentPosition = letterPosition.end;
            if (idx == 0)
            {
                currentPosition = letterPosition.start;
                previousForm = arabicLetterForms.restricted;
            }
            if (word.Count() == 1)
                currentPosition = letterPosition.isolated;

            int newCharVal = getContextualForm(currentChar);
            if (newCharVal == char.MinValue)
                return char.MinValue;

            int nextChar = 0, prevChar = 0;

            if (idx > 0)
                prevChar = word[idx - 1];
            if (idx < word.Count() - 1)
                nextChar = word[idx + 1];

            int ligature = testForLigatures(currentChar, nextChar);
            if (ligature != char.MinValue)
            {
                newCharVal = ligature;
                currentForm = arabicLetterForms.restricted;
                idx++;
            }

            if (currentPosition == letterPosition.isolated) //stand alone letter
            {
                newCharVal = newCharVal + 0; //isolated glyph
            }

            if (currentPosition == letterPosition.start || previousForm == arabicLetterForms.restricted)
            {   //No right bind
                if (currentForm == arabicLetterForms.restricted)
                    newCharVal = newCharVal + 0; //isolated glyph
                if (currentForm == arabicLetterForms.unrestricted && currentPosition != letterPosition.end)
                    newCharVal = newCharVal + 2; //starting glyph
            }
            if ((currentPosition == letterPosition.end || currentForm == arabicLetterForms.restricted) && previousForm == arabicLetterForms.unrestricted)
            {  //Right bind only
                newCharVal = newCharVal + 1;
            }
            if (currentPosition != letterPosition.end && currentForm == arabicLetterForms.unrestricted && previousForm == arabicLetterForms.unrestricted)
            { //Right and left bind
                newCharVal = newCharVal + 3;
            }

            previousForm = currentForm;
            return (char)newCharVal;
        }

        int getContextualForm(int currentChar)
        {
            int newCharVal = char.MinValue;
            switch (currentChar)
            {
                case 1575: //0627
                    newCharVal = 65165;
                    break;
                case 1576: //0628
                    newCharVal = 65167;
                    break;
                case 1578: //062A
                    newCharVal = 65173;
                    break;
                case 1579:
                    newCharVal = 65177;
                    break;
                case 1580:
                    newCharVal = 65181;
                    break;
                case 1581:
                    newCharVal = 65185;
                    break;
                case 1582:
                    newCharVal = 65189;
                    break;
                case 1583:
                    newCharVal = 65193;
                    break;
                case 1584:
                    newCharVal = 65195;
                    break;
                case 1585:
                    newCharVal = 65197;
                    break;
                case 1586:
                    newCharVal = 65199;
                    break;
                case 1587:
                    newCharVal = 65201;
                    break;
                case 1588:
                    newCharVal = 65205;
                    break;
                case 1589:
                    newCharVal = 65209;
                    break;
                case 1590:
                    newCharVal = 65213;
                    break;
                case 1591:
                    newCharVal = 65217;
                    break;
                case 1592:
                    newCharVal = 65221;
                    break;
                case 1593:
                    newCharVal = 65225;
                    break;
                case 1594:
                    newCharVal = 65229;
                    break;
                case 1601:
                    newCharVal = 65233;
                    break;
                case 1602:
                    newCharVal = 65237;
                    break;
                case 1603:
                    newCharVal = 65241;
                    break;
                case 1604:
                    newCharVal = 65245;
                    break;
                case 1605:
                    newCharVal = 65249;
                    break;
                case 1606:
                    newCharVal = 65253;
                    break;
                case 1607:
                    newCharVal = 65257;
                    break;
                case 1608:
                    newCharVal = 65261;
                    break;
                case 1610:
                    newCharVal = 65265;
                    break;
                case 1570:
                    newCharVal = 65153;
                    break;
                case 1571:
                    newCharVal = 65155;
                    break;
                case 1577:
                    newCharVal = 65171;
                    break;
                case 1609:
                    newCharVal = 65263;
                    break;
            }
            return newCharVal;
        }
        #endregion
    }
}
