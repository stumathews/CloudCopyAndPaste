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
    public partial class UploadedFiles : Form
    {
        public List<object> Files
        {
            get;
            set;
        }

        public UploadedFiles(List<string> files)
        {
            InitializeComponent();

            foreach (string file in files)
                lstUploadedFiles.Items.Add(file);
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            Files = (from object item in lstUploadedFiles.SelectedItems select item).ToList();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void UploadedFiles_Load(object sender, EventArgs e)
        {

        }
    }
}
