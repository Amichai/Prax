using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prax.OcrEngine.Services;
using System.Collections.ObjectModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace Prax.OcrEngine.Engine {
	public class OutputRenderer {
		public const int ThresholdCertainty = 500;
		public List<RecognizedSegment> resolvedSegmentsList = new List<RecognizedSegment>();

		internal void Add(IEnumerable<RecognizedSegment> returnVal) {
			resolvedSegmentsList.AddRange(returnVal);
		}

		const int newLineYDiscrepancy = 3;

		private List<RecognizedSegment> orderAllResults(ReadOnlyCollection<RecognizedSegment> results) {
			var sortedOutput = results.OrderBy(k => k.Bounds.Y).ToList();
			int indiciesToAdjust = 0;

			for (int i = sortedOutput.Count - 1; i >= 0; i--) {
				if (i != 0 && (sortedOutput[i].Bounds.Y - sortedOutput[i - 1].Bounds.Y) < 4) {
					indiciesToAdjust++;
				} else {
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

			sortedOutput = sortedOutput.OrderByDescending(k => k.Bounds.Width)
										.ThenBy(k => k.Bounds.X)
										.ThenBy(k => k.Bounds.Y).ToList();
			return sortedOutput;
		}

		private class lastSegmentRendered {
			public lastSegmentRendered() {
				startingXValue = int.MinValue;
				endingXValue = int.MinValue;
				YValue = 0;
			}
			public void ResetListOfPreviousRenderings() {
				renderingsAndEndIdx = new List<Tuple<string, int>>();
			}

			public void RemoveOldRenderedString(RecognizedSegment seg) {
				for (int k = 0; k < renderingsAndEndIdx.Count; k++) {
					if (renderingsAndEndIdx[k].Item2 <= seg.Bounds.X)
						renderingsAndEndIdx.RemoveAt(k);
					else
						k = renderingsAndEndIdx.Count;
				}
			}

			public List<Tuple<string, int>> renderingsAndEndIdx = new List<Tuple<string, int>>();

			public int startingXValue, endingXValue, YValue;

			public bool TestForNewLine(RecognizedSegment seg, writerPosition position) {
				return Math.Abs(YValue - seg.Bounds.Y) > newLineYDiscrepancy && position != writerPosition.firstSeg;
			}

			public void AdjustLastSegValues(RecognizedSegment seg) {
				endingXValue = seg.Bounds.Right;
				startingXValue = seg.Bounds.X;
				YValue = seg.Bounds.Y;
			}
		}

		private class outputObject {
			string outputString = string.Empty;
			Paragraph paragraph;
			iTextSharp.text.Document doc = new iTextSharp.text.Document();
			PdfWriter writer;
			public outputObject() {
				writer = PdfWriter.GetInstance(doc, new FileStream("pdfOutput.pdf", FileMode.Create));
				string fontpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "times.ttf");
				BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
				Font arabicFont = new Font(basefont, 10f, Font.NORMAL);
				paragraph = new Paragraph(string.Empty, arabicFont);
				paragraph.Alignment = Element.ALIGN_RIGHT;
				doc.Open();
			}

			public void AddOutput(string outputToAdd) {
				paragraph.Add(new string(outputToAdd.Reverse().ToArray()));
				outputString += new string(outputToAdd.Reverse().ToArray());
			}

			public string PrintStoredOutput() {
				if (!paragraph.IsEmpty()) {
					doc.Add(paragraph);
					doc.Close();
					outputString = new string(outputString.Reverse().ToArray());
					writer.Close();
					File.WriteAllText("TxtToRender.txt", outputString);
					return outputString;
				} else return null;
			}
		}

		private int columnStart = 0; //int.MaxValue; 
		//TODO: This won't be necessary when we enforce precise location

		private enum writerPosition { firstSeg, newLine, sameLine };

		public string Render() {
			outputObject resultsRenderer = new outputObject();
			List<RecognizedSegment> sortedOutput = orderAllResults(resolvedSegmentsList.AsReadOnly());

			const int overlapThreshold = 1;
			const int spaceWidth = 7; //The amount of pixels in a space

			lastSegmentRendered lastSeg = new lastSegmentRendered();
			lastSeg.endingXValue = columnStart;
			lastSeg.startingXValue = columnStart;

			writerPosition position = writerPosition.firstSeg;

			for (int i = 0; i < sortedOutput.Count; i++) {
				//Test for new line
				if (lastSeg.TestForNewLine(sortedOutput[i], position)) {
					//New line determined
					resultsRenderer.AddOutput(Environment.NewLine);
					lastSeg.AdjustLastSegValues(sortedOutput[i]);
					position = writerPosition.newLine;
				} else {
					//Not a new line
					if (position == writerPosition.sameLine &&
						//check that the segment isn't wholly subsumed in the last
						!(sortedOutput[i].Bounds.X >= lastSeg.startingXValue
								&& sortedOutput[i].Bounds.Right <= lastSeg.endingXValue)
						) {
						//Add spaces
						int numberOfSpaces = (sortedOutput[i].Bounds.X - lastSeg.endingXValue) / spaceWidth;
						for (int j = 0; j < numberOfSpaces; j++) {
							resultsRenderer.AddOutput(" ");
						}
						string segToResolve = sortedOutput[i].Text;
						string segToRender = string.Empty;
						//Remove segments from the previous renderings list
						lastSeg.RemoveOldRenderedString(sortedOutput[i]);
						//Check for some degree of overlap
						if (lastSeg.endingXValue - sortedOutput[i].Bounds.X > overlapThreshold) {
							//Check if some characters can be removed from segToResolve
							string storedPrevRenderings = string.Empty;
							foreach (string s in lastSeg.renderingsAndEndIdx.Select(k => k.Item1)) {
								storedPrevRenderings += s;
							}
							for (int j = 0; j < segToResolve.ToCharArray().Count(); j++) {
								if (!storedPrevRenderings.Contains(segToResolve[j])) {
									segToRender += segToResolve[j];
								} else {
									int idx = storedPrevRenderings.IndexOf(segToResolve[j]);
									storedPrevRenderings = storedPrevRenderings.Remove(idx, 1);
								}
							}
							lastSeg.renderingsAndEndIdx.Add(new Tuple<string, int>(segToRender, sortedOutput[i].Bounds.Right));
							resultsRenderer.AddOutput(segToRender);
							lastSeg.AdjustLastSegValues(sortedOutput[i]);
						} else {
							//No overlap. Clear list of previous rendrings
							lastSeg.ResetListOfPreviousRenderings();
							lastSeg.renderingsAndEndIdx.Add(new Tuple<string, int>(sortedOutput[i].Text, sortedOutput[i].Bounds.Right));
							resultsRenderer.AddOutput(sortedOutput[i].Text);
							lastSeg.AdjustLastSegValues(sortedOutput[i]);
						}
					}
					if (position == writerPosition.firstSeg || position == writerPosition.newLine) {
						//New line. Render without checking for spaces
						lastSeg.ResetListOfPreviousRenderings();
						lastSeg.renderingsAndEndIdx.Add(new Tuple<string, int>(sortedOutput[i].Text, sortedOutput[i].Bounds.Right));
						resultsRenderer.AddOutput(sortedOutput[i].Text);
						lastSeg.AdjustLastSegValues(sortedOutput[i]);
						position = writerPosition.sameLine;
					}
				}
			}
			return resultsRenderer.PrintStoredOutput();
		}
	}
}
