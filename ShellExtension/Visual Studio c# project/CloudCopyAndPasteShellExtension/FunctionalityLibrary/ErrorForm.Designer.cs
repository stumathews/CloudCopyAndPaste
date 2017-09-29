namespace FunctionalityLibrary.UserInterfaces
{
    partial class ErrorForm
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
            this.picExpand = new System.Windows.Forms.PictureBox();
            this.lblTechnicalDetails = new System.Windows.Forms.Label();
            this.lnkCopy = new System.Windows.Forms.LinkLabel();
            this.txtTechnicalDetails = new System.Windows.Forms.TextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picExpand)).BeginInit();
            this.SuspendLayout();
            // 
            // picExpand
            // 
            this.picExpand.Location = new System.Drawing.Point(10, 94);
            this.picExpand.Name = "picExpand";
            this.picExpand.Size = new System.Drawing.Size(25, 25);
            this.picExpand.TabIndex = 12;
            this.picExpand.TabStop = false;
            this.picExpand.Click += new System.EventHandler(this.picExpand_Click);
            // 
            // lblTechnicalDetails
            // 
            this.lblTechnicalDetails.AutoSize = true;
            this.lblTechnicalDetails.Location = new System.Drawing.Point(40, 97);
            this.lblTechnicalDetails.Name = "lblTechnicalDetails";
            this.lblTechnicalDetails.Size = new System.Drawing.Size(39, 13);
            this.lblTechnicalDetails.TabIndex = 11;
            this.lblTechnicalDetails.Text = "Details";
            // 
            // lnkCopy
            // 
            this.lnkCopy.AutoSize = true;
            this.lnkCopy.Location = new System.Drawing.Point(92, 97);
            this.lnkCopy.Name = "lnkCopy";
            this.lnkCopy.Size = new System.Drawing.Size(31, 13);
            this.lnkCopy.TabIndex = 10;
            this.lnkCopy.TabStop = true;
            this.lnkCopy.Text = "Copy";
            this.lnkCopy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCopy_LinkClicked);
            // 
            // txtTechnicalDetails
            // 
            this.txtTechnicalDetails.BackColor = System.Drawing.SystemColors.Control;
            this.txtTechnicalDetails.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTechnicalDetails.Location = new System.Drawing.Point(12, 142);
            this.txtTechnicalDetails.Multiline = true;
            this.txtTechnicalDetails.Name = "txtTechnicalDetails";
            this.txtTechnicalDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTechnicalDetails.Size = new System.Drawing.Size(435, 112);
            this.txtTechnicalDetails.TabIndex = 9;
            // 
            // txtMessage
            // 
            this.txtMessage.BackColor = System.Drawing.SystemColors.Control;
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Location = new System.Drawing.Point(19, 23);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(412, 51);
            this.txtMessage.TabIndex = 8;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(370, 94);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // ErrorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(448, 154);
            this.ControlBox = false;
            this.Controls.Add(this.picExpand);
            this.Controls.Add(this.lblTechnicalDetails);
            this.Controls.Add(this.lnkCopy);
            this.Controls.Add(this.txtTechnicalDetails);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximumSize = new System.Drawing.Size(464, 310);
            this.MinimumSize = new System.Drawing.Size(464, 160);
            this.Name = "ErrorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "An error occurred";
            this.Load += new System.EventHandler(this.ErrorForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picExpand)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picExpand;
        private System.Windows.Forms.Label lblTechnicalDetails;
        private System.Windows.Forms.LinkLabel lnkCopy;
        private System.Windows.Forms.TextBox txtTechnicalDetails;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnOK;
    }
}