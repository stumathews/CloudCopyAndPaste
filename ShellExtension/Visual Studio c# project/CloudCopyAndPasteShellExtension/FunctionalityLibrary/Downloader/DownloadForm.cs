/****************************** Module Header ******************************\
 * Module Name:  MainForm.cs
 * Project:      CSWebDownloader
 * Copyright (c) Microsoft Corporation.
 * 
 * This is the main form of this application. It is used to initialize the UI and 
 * handle the events.
 * 
 * This source is subject to the Microsoft Public License.
 * See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
 * All other rights reserved.
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CSWebDownloader
{
    public partial class DownloadForm : Form
    {
        IDownloader downloader = null;

        DateTime lastNotificationTime;

        /// <summary>
        /// The download URL
        /// </summary>
        public string FileToDownload
        {
            get
            {
                return tbURL.Text;
            }
            set
            {
                tbURL.Text = value;
            }
        }
        /// <summary>
        /// The local path where you want to save the file.
        /// </summary>
        public string DownloadPath
        {
            get
            {
                return tbPath.Text;
            }
            set
            {
                tbPath.Text = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadForm"/> class.
        /// </summary>
        /// <param name="URL">The URL.</param>
        /// <param name="save_filename">The save_filename.</param>
        public DownloadForm(string URL, string save_filename)
        {
            InitializeComponent();

            this.FileToDownload = URL;
            this.DownloadPath = save_filename;
        }

        /// <summary>
        /// Starts the download.
        /// </summary>
        private void DownLoadFile()
        {
            // Initialize an instance of HttpDownloadClient.
            downloader = new HttpDownloadClient(FileToDownload);

            // Register the events of HttpDownloadClient.
            downloader.DownloadCompleted += DownloadCompleted;
            downloader.DownloadProgressChanged += DownloadProgressChanged;
            downloader.StatusChanged += StatusChanged;

          
            // Check whether the file exists.
            if (File.Exists(DownloadPath))
            {
                string message = "There is already a file with the same name, "
                        + "do you want to delete it? "
                        + "If not, please change the local path. ";
                var result = MessageBox.Show(
                    message,
                    "File name conflict: " + DownloadPath,
                    MessageBoxButtons.OKCancel);

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    File.Delete(DownloadPath);
                }
                else
                {
                    return;
                }
            }

            // Construct the temporary file path.
            string tempPath = this.DownloadPath + ".tmp";

            // Delete the temporary file if it already exists.
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            // Initialize an instance of HttpDownloadClient.
            // Store the file to a temporary file first.
            downloader.DownloadPath = tempPath;

            // Start to download file.
            downloader.BeginDownload();
        }
        

        /// <summary>
        /// Handle StatusChanged event.
        /// </summary>
        void StatusChanged(object sender, EventArgs e)
        {
            this.Invoke(new EventHandler(StatusChangedHanlder), sender, e);
        }


        /// <summary>
        /// Statuses the changed hanlder.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void StatusChangedHanlder(object sender, EventArgs e)
        {
            // Refresh the status.
            lbStatus.Text = downloader.Status.ToString();

            // Refresh the buttons.
            switch (downloader.Status)
            {
                case DownloadStatus.Waiting:                    
                    btnCancel.Enabled = true;                    
                    btnPause.Enabled = false;
                    tbPath.Enabled = false;
                    tbURL.Enabled = false;
                    break;
                case DownloadStatus.Canceled:
                case DownloadStatus.Completed:                    
                    btnCancel.Enabled = true;                    
                    btnPause.Enabled = false;
                    tbPath.Enabled = false;
                    tbURL.Enabled = true;
                    break;
                case DownloadStatus.Downloading:                   
                    btnCancel.Enabled = true;                    
                    btnPause.Enabled = true & downloader.IsRangeSupported;
                    tbPath.Enabled = false;
                    tbURL.Enabled = false;
                    break;
                case DownloadStatus.Paused:                    
                    btnCancel.Enabled = true;
                    // The "Resume" button.
                    btnPause.Enabled = true & downloader.IsRangeSupported;
                    tbPath.Enabled = false;
                    tbURL.Enabled = false;
                    break;
            }

            if (downloader.Status == DownloadStatus.Paused)
            {
                lbSummary.Text =
                   String.Format("Received: {0}KB, Total: {1}KB, Time: {2}:{3}:{4}",
                   downloader.DownloadedSize / 1024, downloader.TotalSize / 1024,
                   downloader.TotalUsedTime.Hours, downloader.TotalUsedTime.Minutes,
                   downloader.TotalUsedTime.Seconds);

                btnPause.Text = "Resume";
            }
            else
            {
                btnPause.Text = "Pause";
            }
        }



        /// <summary>
        /// Handle DownloadProgressChanged event.
        /// </summary>
        void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.Invoke(
                new EventHandler<DownloadProgressChangedEventArgs>(DownloadProgressChangedHanlder),
                sender, e);
        }

        void DownloadProgressChangedHanlder(object sender, DownloadProgressChangedEventArgs e)
        {
            // Refresh the summary every second.
            if (DateTime.Now > lastNotificationTime.AddSeconds(1))
            {
                lbSummary.Text = String.Format("Received: {0}KB, Total: {1}KB, Speed: {2}KB/s",
                    e.ReceivedSize / 1024, e.TotalSize / 1024, e.DownloadSpeed / 1024);
                prgDownload.Value = (int)(e.ReceivedSize * 100 / e.TotalSize);
                lastNotificationTime = DateTime.Now;
            }
        }


        /// <summary>
        /// Handle DownloadCompleted event.
        /// </summary>
        void DownloadCompleted(object sender, DownloadCompletedEventArgs e)
        {
            this.Invoke(
                new EventHandler<DownloadCompletedEventArgs>(DownloadCompletedHanlder),
                sender, e);
        }

        void DownloadCompletedHanlder(object sender, DownloadCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                lbSummary.Text =
                    String.Format("Received: {0}KB, Total: {1}KB, Time: {2}:{3}:{4}",
                    e.DownloadedSize / 1024, e.TotalSize / 1024, e.TotalTime.Hours,
                    e.TotalTime.Minutes, e.TotalTime.Seconds);

                if (File.Exists(tbPath.Text.Trim()))
                {
                    File.Delete(tbPath.Text.Trim());
                }

                File.Move(tbPath.Text.Trim() + ".tmp", tbPath.Text.Trim());
                prgDownload.Value = 100;
                Close();
            }
            else
            {
                lbSummary.Text = e.Error.Message;
                if (File.Exists(tbPath.Text.Trim() + ".tmp"))
                {
                    File.Delete(tbPath.Text.Trim() + ".tmp");
                }

                if (File.Exists(tbPath.Text.Trim()))
                {
                    File.Delete(tbPath.Text.Trim());
                }

                prgDownload.Value = 0;
            }
        }

        /// <summary>
        /// Handle btnCancel Click event.
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (downloader != null)
            {
                downloader.Cancel();
            }

        }

        /// <summary>
        /// Handle btnPause Click event.
        /// </summary>
        private void btnPause_Click(object sender, EventArgs e)
        {
            if (downloader.Status == DownloadStatus.Paused)
            {
                downloader.BeginResume();
            }
            else if (downloader.Status == DownloadStatus.Downloading)
            {
                downloader.Pause();
            }
        }

        /// <summary>
        /// Begins the download.
        /// </summary>
        internal void BeginDownload()
        {
            DownLoadFile();
        }
    }
}
