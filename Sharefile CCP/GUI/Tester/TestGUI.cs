using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CloudCopyAndPasteShellExtension;
using SharefileCCPLib;
using CloudCopyAndPasteShellExtension;

namespace Tester
{
    public partial class TestGUI : Form
    {
        const string UPLOADS_FOLDER = "Uploads";
        const string username = "harry.rydzkowski@citrix.com";
        const string DOWNLOADS_FOLDER = @"C:\Users\henryr\Documents\Downloads\";
        private delegate List<object> GetItemsDelegate();
        

        public TestGUI()
        {
            InitializeComponent();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(delegate()
            {
                SharefileCCP sharefile = new SharefileCCP();
                SharefileCCP.HRESULT loginStatus = sharefile.Authenticate("citrix", "sharefile.com", "thing", "Apps3cur3");

                if (loginStatus == SharefileCCP.HRESULT.S_OK)
                {
                    string folderId = string.Empty;

                    //if (!sharefile.DoesFolderExist(UPLOADS_FOLDER))
                    if(!sharefile.DoesFolderExist(@"/" + username + @"/",UPLOADS_FOLDER))
                    {
                        folderId = sharefile.CreateFolder(username, UPLOADS_FOLDER);
                    }
                    else
                    {
                        //folderId = sharefile.GetItemId(UPLOADS_FOLDER, SharefileCCP.ITEM_TYPE.FOLDER);
                        folderId = sharefile.GetItemId(@"/" + username + @"/", UPLOADS_FOLDER, SharefileCCP.ITEM_TYPE.FOLDER);
                    }
                    Dictionary<string,object> optionalParams = new Dictionary<string,object>();
                    optionalParams.Add("folderid", folderId);

                    bool uploadSuccessful = true;

                    List<object> items = GetFilesToUpload();

                    foreach (object listItem in items)
                    {
                        string retVal = sharefile.FileUpload((string)listItem, optionalParams);
                        if (!string.IsNullOrEmpty(retVal) && !retVal.Contains("OK:"))
                        {
                            uploadSuccessful = false;
                            break;
                        }
                    }

                    MessageBox.Show(uploadSuccessful ? "Upload successful." : "Upload failed.\nPlease try again later.");
                }
            }));
            thread.Start();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "All Files(*.*)|*";
            openFileDialog1.Multiselect = true;
            openFileDialog1.ShowReadOnly = false;
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                List<ShareFileItem> items = new List<ShareFileItem>();

                foreach(string filename in openFileDialog1.FileNames)
                {
                    items.Add(new ShareFileItem(filename, "", "", ""));
                }

                SetList(items);
            }
        }

        private void lstFilesToUpload_DragDrop(object sender, DragEventArgs e)
        {
        }

        private void SetList(List<ShareFileItem> items)
        {
            foreach (ShareFileItem item in items)
            {
                lstFilesToUpload.Items.Add(item.Filename);
            }
        }

        /// <summary>
        /// Downloads to \My Documents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownload_Click(object sender, EventArgs e)
        {
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(delegate()
            {
                bool isSuccessful=true;

                SharefileCCP sharefile = new SharefileCCP();
                SharefileCCP.HRESULT loginStatus = sharefile.Authenticate("citrix", "sharefile.com", username, "Apps3cur3");

                if (loginStatus == SharefileCCP.HRESULT.S_OK)
                {
                    List<object> items = GetUploadedFiles();

                    foreach (object filename in items)
                    {
                        //if (!sharefile.FileDownload(sharefile.GetItemId((string)filename, SharefileCCP.ITEM_TYPE.FILE), DOWNLOADS_FOLDER + (string)filename))
                        sharefile.FileDownload(sharefile.GetItemId(@"/" + username + @"/" + UPLOADS_FOLDER, (string)filename, SharefileCCP.ITEM_TYPE.FILE), DOWNLOADS_FOLDER + (string)filename);                        
                    }

                    MessageBox.Show(isSuccessful ? "Download successful." : "Download failed.\nPlease try again later.");
                }
                else if (loginStatus == SharefileCCP.HRESULT.E_FAIL)
                {

                    FunctionalityLibrary.UserInterfaces.ErrorForm frm = new FunctionalityLibrary.UserInterfaces.ErrorForm()
                    {
                        Message = sharefile.FriendlyError,
                        Technical = sharefile.TechnicalError,
                    };
                    frm.ShowDialog(this);

                }
            }));
            thread.Start();

        }

        private List<object> GetUploadedFiles()
        {
            if (lstUploadedFiles.InvokeRequired)
            {
                var del = new GetItemsDelegate(GetUploadedFiles);
                return lstUploadedFiles.Invoke(del) as List<object>;
            }
            return (from object item in lstUploadedFiles.SelectedItems select item).ToList();
        }

        private List<object> GetFilesToUpload()
        {
            if (lstFilesToUpload.InvokeRequired)
            {
                var del = new GetItemsDelegate(GetFilesToUpload);
                return lstFilesToUpload.Invoke(del) as List<object>;
            }
            return (from object item in lstFilesToUpload.Items select item).ToList();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PopulateUploadedFilesList();
        }

        private void PopulateUploadedFilesList()
        {
            lstUploadedFiles.Items.Clear();

            SharefileCCP sharefile = new SharefileCCP();
            SharefileCCP.HRESULT loginStatus = sharefile.Authenticate("citrix1", "sharefile.com", username, "Apps3cur3");

            if (loginStatus == SharefileCCP.HRESULT.S_OK)
            {
                List<string> files = sharefile.FolderList(@"/" + username + @"/" + UPLOADS_FOLDER);

                foreach (string file in files)
                {
                    lstUploadedFiles.Items.Add(file);
                }
            }
            else
            {
                FunctionalityLibrary.UserInterfaces.ErrorForm.Show(sharefile.FriendlyError, sharefile.TechnicalError);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            PopulateUploadedFilesList();
        }

        private void copyAndPasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyAndPaste form = new CopyAndPaste();
            form.ShowDialog();
        }
    }
}
