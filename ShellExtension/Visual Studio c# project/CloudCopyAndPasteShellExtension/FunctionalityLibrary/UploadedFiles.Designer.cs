namespace FunctionalityLibrary.UserInterfaces
{
    partial class UploadedFiles
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
            this.label2 = new System.Windows.Forms.Label();
            this.lstUploadedFiles = new System.Windows.Forms.ListBox();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(42, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Uploaded files";
            // 
            // lstUploadedFiles
            // 
            this.lstUploadedFiles.AllowDrop = true;
            this.lstUploadedFiles.FormattingEnabled = true;
            this.lstUploadedFiles.Location = new System.Drawing.Point(44, 26);
            this.lstUploadedFiles.Name = "lstUploadedFiles";
            this.lstUploadedFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstUploadedFiles.Size = new System.Drawing.Size(230, 238);
            this.lstUploadedFiles.TabIndex = 8;
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(44, 270);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(75, 23);
            this.btnDownload.TabIndex = 7;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(199, 270);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // UploadedFiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 302);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstUploadedFiles);
            this.Controls.Add(this.btnDownload);
            this.Name = "UploadedFiles";
            this.Text = "UploadedFiles";
            this.Load += new System.EventHandler(this.UploadedFiles_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lstUploadedFiles;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnCancel;
    }
}