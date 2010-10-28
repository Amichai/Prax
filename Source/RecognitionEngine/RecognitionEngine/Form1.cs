using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Prax.Recognition
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private UploadDocument uploadDocument = null;

        private void uploadFile(object sender, EventArgs e)
        {
            upload.Enabled = false;
            DisplayUtility.DefaultPictureBox = pictureBox;
            uploadDocument = new UploadDocument(pictureBox);
            upload.Enabled = true;
        }

        private void trainAlgorithm(object sender, EventArgs e)
        {
            train.Enabled = false;
            DisplayUtility.DefaultPictureBox = pictureBox;
            AlgorithmTrainer trainHandler = new AlgorithmTrainer();
            train.Enabled = true;
        }

        private void readDocument(object sender, EventArgs e)
        {
            read.Enabled = false;
            DocumentReader readHandler = new DocumentReader(uploadDocument.uploadedDocument);
            read.Enabled = true;
        }

        private void clearTrainingData(object sender, EventArgs e)
        {
            File.Delete("TrainingData.dat");
        }       
    }
}
