using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Prax.OcrEngine.Services;
namespace Prax.Recognition
{
    class OutputRenderer:IResultsConverter
    {
        private int columnStart = 0; //int.MaxValue; 
        //TODO: This won't be necessary when we enforce precise location

        public Stream Convert(Stream input, ReadOnlyCollection<RecognizedSegment> results)
        {
            var doc = new iTextSharp.text.Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream("pdfOutput.pdf", FileMode.Create));
            
            doc.Open();
            //Paragraph paragraph = new Paragraph();
            string fontpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arabtype.ttf");
            BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font arabicFont = new Font(basefont, 10f, Font.NORMAL);

            Paragraph paragraph = new Paragraph(string.Empty, arabicFont);

//            var sortedOutput = results.OrderBy(k => k.Bounds.X).ToList();
            var sortedOutput = results.OrderBy(k => k.Bounds.Y).ToList();
            int indiciesToAdjust = 0;

            for (int i = sortedOutput.Count - 1; i >= 0; i--)
            {
                if (i != 0 && (sortedOutput[i].Bounds.Y - sortedOutput[i - 1].Bounds.Y) < 4)
                {
                    indiciesToAdjust++;
                }
                else
                {
                    for (int j = 1; j <= indiciesToAdjust; j++)
                    {
                        System.Drawing.Rectangle newBounds = new System.Drawing.Rectangle(sortedOutput[i + j].Bounds.X,
                                                                                            sortedOutput[i].Bounds.Y,
                                                                                            sortedOutput[i + j].Bounds.Width,
                                                                                            sortedOutput[i + j].Bounds.Height);
                        sortedOutput[i + j] = new RecognizedSegment(newBounds, sortedOutput[i + j].Text, sortedOutput[i + j].Certainty);
                    }
                    indiciesToAdjust = 0;
                }
            }




            sortedOutput = sortedOutput.OrderBy(k => k.Bounds.Width).OrderBy(k => k.Bounds.X).OrderBy(k => k.Bounds.Y).ToList();
            int numberOfSpaces = 0;  //this should not be initialized to zero because of the case of first line indent
            const int spaceWidth = 2; //The amount of pixels in a space
            const int newLineYDiscrepancy = 3;
            const double certaintyThreshold = .5;
            string output = " ";
            RecognizedSegment lastOutputUnit = new RecognizedSegment();
            bool sameLine = true;


            StreamWriter streamWriter = new StreamWriter("output.txt", false, Encoding.Unicode);
            string tempString = "جمعيّة ال";
            

            for (int i = 0; i < sortedOutput.Count(); i++)
            {
                paragraph.Add(sortedOutput[i].Text);
                streamWriter.Write(sortedOutput[i].Text);
            }
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    paragraph.Add(j.ToString() + " ");
                    paragraph.Add(tempString);
                }
                paragraph.Add("\n" + i.ToString() + " ");

            }


            //for (int i = 0; i < sortedOutput.Count; i++)
            //{
            //    if (lastOutputUnit.Bounds.X != 0 && Math.Abs(lastOutputUnit.Bounds.Y - sortedOutput[i].Bounds.Y) > newLineYDiscrepancy)
            //    {               //test for a ylocation descrepancy greater than newLineYDiscrepancy
            //        paragraph.Add("\n");
            //        sameLine = false;
            //    }
            //    else
            //        sameLine = true;
            //    if (lastOutputUnit.Bounds.X != 0)
            //    {
            //        if (sameLine)
            //        {
            //            numberOfSpaces = (sortedOutput[i].Bounds.X - (lastOutputUnit.Bounds.X + lastOutputUnit.Bounds.Width)) / spaceWidth;
            //            for (int j = 0; j < numberOfSpaces; j++)
            //            {
            //                paragraph.Add(" ");
            //            }
            //            if (!(lastOutputUnit.Bounds.X + lastOutputUnit.Bounds.Width > sortedOutput[i].Bounds.X + sortedOutput[i].Bounds.Width) && sortedOutput[i].Certainty > certaintyThreshold)
            //            {   //check that the current output isn't subsumed in the last output                               and that the certainty surpasses a threshold
            //                output = sortedOutput[i].Text;
            //                paragraph.Add(output);
            //            }
            //        }
            //        else
            //        {
            //            numberOfSpaces = (sortedOutput[i].Bounds.X - columnStart) / spaceWidth;
            //            for (int j = 0; j < numberOfSpaces; j++)
            //            {
            //                paragraph.Add(" ");
            //            }
            //            if (sortedOutput[i].Certainty > certaintyThreshold)
            //            {
            //                output = sortedOutput[i].Text;
            //                paragraph.Add(output);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        numberOfSpaces = (sortedOutput[i].Bounds.X - columnStart) / spaceWidth;
            //        for (int j = 0; j < numberOfSpaces; j++)
            //        {
            //            paragraph.Add(" ");
            //        }
            //        if (sortedOutput[i].Certainty > certaintyThreshold)
            //        {
            //            output = sortedOutput[i].Text;
            //            paragraph.Add(output);
            //        }
            //    }
            //    lastOutputUnit = sortedOutput[i];
            //}

            doc.Add(paragraph);
            doc.Close();

            FileStream returnedFileAsStream = new FileStream("pdfOutput.pdf", FileMode.Open);
            return returnedFileAsStream;
        }
        public ResultFormat OutputFormat { get { return ResultFormat.Pdf; } }
    }
}
