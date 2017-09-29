using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FunctionalityLibrary.UserInterfaces
{
    /// <summary>
    /// Form that shows user friendly error messages and technical details
    /// </summary>
    public partial class ErrorForm : Form
    {
        private bool isExpand;

        public ErrorForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// User friendly error message
        /// </summary>
        public string Message
        {
            set
            {
                txtMessage.Text = value;
            }
        }

        /// <summary>
        /// Technical details
        /// </summary>
        public string Technical
        {
            get
            {
                return txtTechnicalDetails.Text;
            }
            set
            {
                if (value != null)
                    value = value.Trim();
                txtTechnicalDetails.Text = value;
            }
        }

        /// <summary>
        /// Shows ErrorForm with specified error messages
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="technical">Technical details</param>
        public static void Show(string message, string technical = "")
        {
            ErrorForm form = new ErrorForm();
            form.Message = message;
            form.Technical = technical;
            form.ShowDialog();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ErrorForm_Load(object sender, EventArgs e)
        {
            bool isTechDetails = txtTechnicalDetails.Text.Length > 0;

            lblTechnicalDetails.Visible = isTechDetails;
            txtTechnicalDetails.Visible = isTechDetails;
            lnkCopy.Visible = isTechDetails;
            picExpand.Visible = isTechDetails;

            picExpand.Image = Images.options_expand;
            this.Size = MinimumSize;
            isExpand = true;
        }

        private void lnkCopy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetDataObject(txtTechnicalDetails.Text, true);
        }

        private void UpdateDisplay()
        {
        }

        private void picExpand_Click(object sender, EventArgs e)
        {
            SuspendLayout();

            if (isExpand)
            {
                picExpand.Image = Images.options_collapse;
                this.Size = MaximumSize;
                isExpand = false;
            }
            else
            {
                picExpand.Image = Images.options_expand;
                this.Size = MinimumSize;
                isExpand=true;
            }

            ResumeLayout();
        }

    }
}
