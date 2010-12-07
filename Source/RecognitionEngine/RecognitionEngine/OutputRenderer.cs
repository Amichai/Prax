using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Prax.OcrEngine.Services;
using System.Diagnostics;
namespace Prax.Recognition
{
    class OutputRenderer
    {
        private int columnStart = 442; //int.MaxValue; 
        //TODO: This won't be necessary when we enforce precise location

        private List<RecognizedSegment> orderAllResults(ReadOnlyCollection<RecognizedSegment> results) {
            var sortedOutput = results.OrderBy(k => k.Bounds.Y).ToList();
            int indiciesToAdjust = 0;

            for (int i = sortedOutput.Count - 1; i >= 0; i--) {
                if (i != 0 && (sortedOutput[i].Bounds.Y - sortedOutput[i - 1].Bounds.Y) < 4) {
                    indiciesToAdjust++;
                } 
                else {
                    for (int j = 1; j <= indiciesToAdjust; j++) {
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
            return sortedOutput;
        }

        private enum writerPosition { firstSeg, newLine, sameLine };

        public Stream Convert(Stream input, ReadOnlyCollection<RecognizedSegment> results)
        {
            var doc = new iTextSharp.text.Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream("pdfOutput.pdf", FileMode.Create));
            string fontpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arabtype.ttf");
            BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Font arabicFont = new Font(basefont, 10f, Font.NORMAL);

            Paragraph paragraph = new Paragraph(string.Empty, arabicFont);
            paragraph.Alignment = Element.ALIGN_RIGHT;
            
            doc.Open();

            List<RecognizedSegment> sortedOutput = orderAllResults(results);
            
            const int overlapThreshold = 3;
            const int spaceWidth = 2; //The amount of pixels in a space
            const int newLineYDiscrepancy = 3;
            string outputString = string.Empty;
            int xIndex = columnStart;
               //xIndex is the right bound of the last piece rendered
            int yIndex = 0;
            string lastSegRendered = string.Empty;
            writerPosition position = writerPosition.firstSeg;

            for (int i = 0; i < sortedOutput.Count; i++) {
                //Test for new line
                if (Math.Abs(yIndex - sortedOutput[i].Bounds.Y) > newLineYDiscrepancy && position != writerPosition.firstSeg) {
                    paragraph.Add("\n");  //ylocation descrepancy greater than newLineYDiscrepancy
                    outputString += Environment.NewLine;
                    position = writerPosition.newLine;
                    xIndex = columnStart;
                    yIndex = sortedOutput[i].Bounds.Y;
                } 
                else {
                    if (position == writerPosition.sameLine) {
                        //Add spaces
                        int numberOfSpaces = (sortedOutput[i].Bounds.X - xIndex) / spaceWidth; 
                        for (int j = 0; j < numberOfSpaces; j++) {
                            paragraph.Add(" ");
                            outputString += " ";
                        }
                        //check segment colision
                        string segToResolve = sortedOutput[i].Text;
                        //Adjust the string to print in case of conflict
                        string segToRender = string.Empty;
                        if (xIndex - sortedOutput[i].Bounds.X > overlapThreshold) {
                            for (int j = 0; j < segToResolve.ToCharArray().Count(); j++) {
                                char removeMe = segToResolve[j];
                                if (!lastSegRendered.Contains(removeMe)) {
                                    segToRender += segToResolve[j];
                                }
                            }
                        } else
                            segToRender = segToResolve; 
                        //Print
                        paragraph.Add(new string(segToRender.Reverse().ToArray()));
                        lastSegRendered = segToResolve;
                        xIndex = sortedOutput[i].Bounds.Right;
                    } else { //render without checking for spaces
                        lastSegRendered = sortedOutput[i].Text;
                        paragraph.Add(new string(sortedOutput[i].Text.Reverse().ToArray()));
                        xIndex = sortedOutput[i].Bounds.Right;
                        yIndex = sortedOutput[i].Bounds.Y;
                        position = writerPosition.sameLine;
                    }
                }
            }
            if (!paragraph.IsEmpty()) {
                doc.Add(paragraph);
                doc.Close();
                FileStream returnedFileAsStream = new FileStream("pdfOutput.pdf", FileMode.Open);
                return returnedFileAsStream;
            }
            return null;
        }
        public ResultFormat OutputFormat { get { return ResultFormat.Pdf; } }
    }
}
