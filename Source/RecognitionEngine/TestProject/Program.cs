using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExtractedOCRFunctionality;
using TextRenderer;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;

namespace TestProject {
	class Program {
		[STAThreadAttribute]
		static void Main(string[] args) {
			string fileName = @"C:\Users\Amichai\Pictures\Handwritten\poe.bmp";
			Document uploadDocument = new Document(fileName);
			IteratedBoards boards = uploadDocument.DefineIteratedBoards();
			uploadDocument.Segment();
			Renderer.RenderImage("test").CreateStream();
		}
	}
}
