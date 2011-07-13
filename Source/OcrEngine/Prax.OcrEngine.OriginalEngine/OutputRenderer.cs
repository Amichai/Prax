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
	public class OutputRenderer : IResultsConverter {
		public static readonly OutputRenderer PlainText = new OutputRenderer(ResultFormat.PlainText, () => new PlainTextRenderer());
		public static readonly OutputRenderer Pdf = new OutputRenderer(ResultFormat.Pdf, () => new PdfRenderer());

		readonly Func<IRenderer> rendererCreator;
		private OutputRenderer(ResultFormat format, Func<IRenderer> rendererCreator) {
			OutputFormat = format;
			this.rendererCreator = rendererCreator;
		}

		public const int ThresholdCertainty = 500;
		const int newLineYDiscrepancy = 3;

		static List<RecognizedSegment> CombineSegments(IEnumerable<RecognizedSegment> segments) {
			var sortedOutput = segments.OrderBy(k => k.Bounds.Y).ToList();
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

			return sortedOutput.OrderByDescending(k => k.Bounds.Width)
							   .ThenBy(k => k.Bounds.X)
							   .ThenBy(k => k.Bounds.Y)
							   .ToList();
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
		
		//TODO: This won't be necessary when we enforce precise location
		private enum writerPosition { firstSeg, newLine, sameLine };

		public ResultFormat OutputFormat { get; private set; }

		public Stream Convert(Stream input, ReadOnlyCollection<RecognizedSegment> results) {
			List<RecognizedSegment> sortedOutput = CombineSegments(results);

			var renderer = rendererCreator();

			const int overlapThreshold = 1;
			const int spaceWidth = 4; //The amount of pixels in a space

			lastSegmentRendered lastSeg = new lastSegmentRendered();
			const int columnStart = 0; //int.MaxValue; 
			lastSeg.endingXValue = columnStart;
			lastSeg.startingXValue = columnStart;

			writerPosition position = writerPosition.firstSeg;

			for (int i = 0; i < sortedOutput.Count; i++) {
				//Test for new line
				if (lastSeg.TestForNewLine(sortedOutput[i], position)) {
					//New line determined
					renderer.AddText(Environment.NewLine);
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
							renderer.AddText(" ");
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
							renderer.AddText(segToRender);
							lastSeg.AdjustLastSegValues(sortedOutput[i]);
						} else {
							//No overlap. Clear list of previous rendrings
							lastSeg.ResetListOfPreviousRenderings();
							lastSeg.renderingsAndEndIdx.Add(new Tuple<string, int>(sortedOutput[i].Text, sortedOutput[i].Bounds.Right));
							renderer.AddText(sortedOutput[i].Text);
							lastSeg.AdjustLastSegValues(sortedOutput[i]);
						}
					}
					if (position == writerPosition.firstSeg || position == writerPosition.newLine) {
						//New line. Render without checking for spaces
						lastSeg.ResetListOfPreviousRenderings();
						lastSeg.renderingsAndEndIdx.Add(new Tuple<string, int>(sortedOutput[i].Text, sortedOutput[i].Bounds.Right));
						renderer.AddText(sortedOutput[i].Text);
						lastSeg.AdjustLastSegValues(sortedOutput[i]);
						position = writerPosition.sameLine;
					}
				}
			}
			return renderer.GetStream();
		}

		interface IRenderer {
			void AddText(string text);
			Stream GetStream();
		}
		class PlainTextRenderer : IRenderer {
			StringBuilder builder = new StringBuilder();

			public void AddText(string text) {
				builder.Insert(0, new string(text.Reverse().ToArray()));
			}

			public Stream GetStream() {
				return new MemoryStream(Encoding.UTF8.GetBytes(builder.ToString()));
			}
		}
		class PdfRenderer : IRenderer {
			static readonly string fontpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "times.ttf");
			readonly MemoryStream stream = new MemoryStream();

			Paragraph paragraph;
			iTextSharp.text.Document doc = new iTextSharp.text.Document();
			PdfWriter writer;

			public PdfRenderer() {
				writer = PdfWriter.GetInstance(doc, stream);
				writer.CloseStream = false;
				BaseFont basefont = BaseFont.CreateFont(fontpath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
				Font arabicFont = new Font(basefont, 10f, Font.NORMAL);
				paragraph = new Paragraph(string.Empty, arabicFont);
				paragraph.Alignment = Element.ALIGN_RIGHT;
				doc.Open();
			}

			public void AddText(string text) {
				paragraph.Add(new string(text.Reverse().ToArray()));
			}

			public Stream GetStream() {
				if (!paragraph.IsEmpty())
					doc.Add(paragraph);

				doc.Close();
				writer.Close();
				stream.Position = 0;
				return stream;
			}
		}
	}
}
