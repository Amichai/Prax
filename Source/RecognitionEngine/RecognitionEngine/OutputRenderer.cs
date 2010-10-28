using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Prax.Recognition
{
    class OutputRenderer
    {
        string OutputFormat { get; set;  } //TODO: not implemented

        private int columnStart = 0; //int.MaxValue; 
        //TODO: This won't be necessary when we inforce precise location

        public FileStream RenderFile(ReadOnlyCollection<RecognizedSegment> ocrResults)
        {
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream("pdfOutput.pdf", FileMode.Create));
            doc.Open();
            Paragraph paragraph = new Paragraph();

            var sortedOutput = ocrResults.OrderBy(k => k.Bounds.Y).ToList();
            int indiciesToAdjust = 0;

            for (int i = sortedOutput.Count - 1; i >= 0; i--)
            {
                if (i != 0 && (sortedOutput[i].Bounds.Y - sortedOutput[i - 1].Bounds.Y) < 4)
                    indiciesToAdjust++;
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

            for (int i = 0; i < sortedOutput.Count; i++)
            {
                if (lastOutputUnit.Bounds.X != 0 && Math.Abs(lastOutputUnit.Bounds.Y - sortedOutput[i].Bounds.Y) > newLineYDiscrepancy)
                {               //test for a ylocation descrepancy greater than newLineYDiscrepancy
                    paragraph.Add("\n");
                    sameLine = false;
                }
                else
                    sameLine = true;
                if (lastOutputUnit.Bounds.X != 0)
                {
                    if (sameLine)
                    {
                        numberOfSpaces = (sortedOutput[i].Bounds.X - (lastOutputUnit.Bounds.X + lastOutputUnit.Bounds.Width)) / spaceWidth;
                        for (int j = 0; j < numberOfSpaces; j++)
                        {
                            paragraph.Add(" ");
                        }
                        if (!(lastOutputUnit.Bounds.X + lastOutputUnit.Bounds.Width > sortedOutput[i].Bounds.X + sortedOutput[i].Bounds.Width) && sortedOutput[i].Certainty > certaintyThreshold)
                        {   //check that the current output isn't subsumed in the last output                               and that the certainty surpasses a threshold
                            output = sortedOutput[i].Text;
                            paragraph.Add(output);
                        }
                    }
                    else
                    {
                        numberOfSpaces = (sortedOutput[i].Bounds.X - columnStart) / spaceWidth;
                        for (int j = 0; j < numberOfSpaces; j++)
                        {
                            paragraph.Add(" ");
                        }
                        if (sortedOutput[i].Certainty > certaintyThreshold)
                        {
                            output = sortedOutput[i].Text;
                            paragraph.Add(output);
                        }
                    }
                }
                else
                {
                    numberOfSpaces = (sortedOutput[i].Bounds.X - columnStart) / spaceWidth;
                    for (int j = 0; j < numberOfSpaces; j++)
                    {
                        paragraph.Add(" ");
                    }
                    if (sortedOutput[i].Certainty > certaintyThreshold)
                    {
                        output = sortedOutput[i].Text;
                        paragraph.Add(output);
                    }
                }
                lastOutputUnit = sortedOutput[i];
            }
            doc.Add(paragraph);
            doc.Close();

            FileStream returnedFileAsStream = new FileStream("pdfOutput.pdf", FileMode.Open);
            return returnedFileAsStream;
        }
    }
}
