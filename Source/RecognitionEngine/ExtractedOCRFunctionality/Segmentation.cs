using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtractedOCRFunctionality {
	public static class Segmentation {
		public static DocumentSegmentationData Segment(this Document doc){
			//Scan across the document
			//Look up for whitespace/borders in training data
			return new DocumentSegmentationData();
		}
	}

	public class DocumentSegmentationData {
	}
}
