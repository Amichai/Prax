namespace Prax.Recognition
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.upload = new System.Windows.Forms.Button();
            this.clearData = new System.Windows.Forms.Button();
            this.train = new System.Windows.Forms.Button();
            this.read = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.algorithmOutput = new System.Windows.Forms.Label();
            this.TrainingDataInfo = new System.Windows.Forms.Label();
            this.PercentComplete = new System.Windows.Forms.Label();
            this.segView = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // upload
            // 
            this.upload.Location = new System.Drawing.Point(13, 13);
            this.upload.Name = "upload";
            this.upload.Size = new System.Drawing.Size(75, 23);
            this.upload.TabIndex = 0;
            this.upload.Text = "Upload";
            this.upload.UseVisualStyleBackColor = true;
            this.upload.Click += new System.EventHandler(this.uploadFile);
            // 
            // clearData
            // 
            this.clearData.Location = new System.Drawing.Point(94, 13);
            this.clearData.Name = "clearData";
            this.clearData.Size = new System.Drawing.Size(75, 23);
            this.clearData.TabIndex = 1;
            this.clearData.Text = "Clear Data";
            this.clearData.UseVisualStyleBackColor = true;
            this.clearData.Click += new System.EventHandler(this.clearTrainingData);
            // 
            // train
            // 
            this.train.Location = new System.Drawing.Point(175, 13);
            this.train.Name = "train";
            this.train.Size = new System.Drawing.Size(75, 23);
            this.train.TabIndex = 2;
            this.train.Text = "Train";
            this.train.UseVisualStyleBackColor = true;
            this.train.Click += new System.EventHandler(this.trainAlgorithm);
            // 
            // read
            // 
            this.read.Location = new System.Drawing.Point(256, 13);
            this.read.Name = "read";
            this.read.Size = new System.Drawing.Size(75, 23);
            this.read.TabIndex = 3;
            this.read.Text = "Test";
            this.read.UseVisualStyleBackColor = true;
            this.read.Click += new System.EventHandler(this.readDocument);
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(13, 42);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(318, 317);
            this.pictureBox.TabIndex = 4;
            this.pictureBox.TabStop = false;
            // 
            // algorithmOutput
            // 
            this.algorithmOutput.AutoSize = true;
            this.algorithmOutput.Location = new System.Drawing.Point(547, 13);
            this.algorithmOutput.Name = "algorithmOutput";
            this.algorithmOutput.Size = new System.Drawing.Size(0, 13);
            this.algorithmOutput.TabIndex = 5;
            // 
            // TrainingDataInfo
            // 
            this.TrainingDataInfo.AutoSize = true;
            this.TrainingDataInfo.Location = new System.Drawing.Point(547, 42);
            this.TrainingDataInfo.Name = "TrainingDataInfo";
            this.TrainingDataInfo.Size = new System.Drawing.Size(0, 13);
            this.TrainingDataInfo.TabIndex = 6;
            // 
            // PercentComplete
            // 
            this.PercentComplete.AutoSize = true;
            this.PercentComplete.Location = new System.Drawing.Point(353, 13);
            this.PercentComplete.Name = "PercentComplete";
            this.PercentComplete.Size = new System.Drawing.Size(0, 13);
            this.PercentComplete.TabIndex = 7;
            // 
            // imageView
            // 
            this.segView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.segView.AutoScroll = true;
            this.segView.Location = new System.Drawing.Point(342, 13);
            this.segView.Name = "imageView";
            this.segView.Size = new System.Drawing.Size(294, 346);
            this.segView.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 371);
            this.Controls.Add(this.segView);
            this.Controls.Add(this.PercentComplete);
            this.Controls.Add(this.TrainingDataInfo);
            this.Controls.Add(this.algorithmOutput);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.read);
            this.Controls.Add(this.train);
            this.Controls.Add(this.clearData);
            this.Controls.Add(this.upload);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button upload;
        private System.Windows.Forms.Button clearData;
        private System.Windows.Forms.Button train;
        private System.Windows.Forms.Button read;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label algorithmOutput;
        private System.Windows.Forms.Label TrainingDataInfo;
        private System.Windows.Forms.Label PercentComplete;
        private System.Windows.Forms.Panel segView;
    }
}