using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tester
{
    public partial class CopyAndPaste : Form
    {
        FunctionalityLibrary.Cloud cloud = new FunctionalityLibrary.Cloud();
        public CopyAndPaste()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            
            cloud.CloudClipboardCopy();
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            //cloud.CloudClipboardPaste();
        }
    }
}
