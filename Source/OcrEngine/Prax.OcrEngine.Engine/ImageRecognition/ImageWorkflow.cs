using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Prax.OcrEngine.Engine.ImageRecognition {
	///<summary>A single step in a workflow that processes an image.</summary>
	///<remarks>These instances can be used on multiple threads simultaneously; they must not store any class-level state.</remarks>
	[ImmutableObject(true)]
	interface IWorkflowStep {
		///<summary>Processes an image.  The image should be mutated to store results.</summary>
		void Process(ImageData image);
	}
	///<summary>Executes a series of operations on an image.</summary>
	[ImmutableObject(true)]
	class ImageWorkflow {
		public static readonly ImageWorkflow StandardWorkflow = new ImageWorkflow(
			//TODO: Create workflow steps
		);

		///<summary>Creates a workflow that executes a series of steps.</summary>
		public ImageWorkflow(params IWorkflowStep[] steps) { Steps = new ReadOnlyCollection<IWorkflowStep>(steps); }
		///<summary>Creates a workflow that executes a series of steps.</summary>
		public ImageWorkflow(IList<IWorkflowStep> steps) { Steps = new ReadOnlyCollection<IWorkflowStep>(steps); }

		///<summary>Gets the steps performed by this workflow.</summary>
		public ReadOnlyCollection<IWorkflowStep> Steps { get; private set; }

		///<summary>Processes an image.  The image will be modified to store results.</summary>
		public virtual void ProcessImage(ImageData image) {
		}
	}
}
