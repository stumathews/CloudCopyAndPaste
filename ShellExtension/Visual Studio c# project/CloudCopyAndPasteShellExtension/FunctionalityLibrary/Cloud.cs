using Nini.Config;
using SharefileCCPLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FunctionalityLibrary
{
    /// <summary>
    /// Class that does cloud stuff
    /// </summary>
    public class Cloud
    {              
        
        private UserInterface forms = new UserInterface();
               
 
        /// <summary>
        /// Copy the item to the cloud
        /// </summary>
        /// <param name="SelectedItemPaths">The selected item paths.</param>
        /// <exception cref="System.Exception">
        /// Unable to initialise.
        /// or
        /// </exception>
        public void CloudCopyItem(IEnumerable<string> SelectedItemPaths)
        {
            if (!CommonFunctionality.Instance.Initialise()) 
                throw new Exception("Unable to initialise during cloud copy item operation.");

            CommonFunctionality.Instance.Authenticate(false);

            bool uploadSuccessful = true;
            
            // File location on cloud drive to copy to
            CommonFunctionality.Instance.uploadfolderId = CommonFunctionality.Instance.uploadfolderId ?? 
                                                          CommonFunctionality.Instance.config.Configs["sharefile"].Get("uploadid"); 

            performUpload(ref uploadSuccessful, SelectedItemPaths);

            // TODO: Retry upload if it fails.
            CommonFunctionality.Instance.logger.LogIt(uploadSuccessful ? "Upload successful." : "Upload failed.\nPlease try again later.");            
            
        }

        /// <summary>
        /// Clouds the paste item.
        /// </summary>
        /// <param name="FolderPath">The folder path.</param>
        /// <exception cref="System.Exception">
        /// Initialise failed while trying to cloud paste item.
        /// or
        /// Unable to establish location to download file to.
        /// or
        /// Error while pasting:  + ex.Message
        /// </exception>
        public void CloudPasteItem(String FolderPath, String filename)
        {
                CommonFunctionality.Instance.Authenticate(false);
                CommonFunctionality.Instance.uploadfolderId = CommonFunctionality.Instance.uploadfolderId ??
                                                              CommonFunctionality.Instance.config.Configs["sharefile"].Get("uploadid");

                bool isSuccessful = true;

                if (String.IsNullOrEmpty(filename))
                {
                    CommonFunctionality.Instance.logger.LogIt("No files available to download.");
                    return;
                }

                String download_location = FolderPath;
                String default_folder = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                String thePath = null;

                if (String.IsNullOrEmpty(download_location))
                {
                    CommonFunctionality.Instance.logger.LogIt("Could not establish folder location to download file to, reverting to default location : " + default_folder);
                    thePath = default_folder;
                }
                else
                {
                    thePath = download_location;
                }

                if (String.IsNullOrEmpty(thePath))
                    throw new Exception("Unable to establish location to download file to.");

                
                String full_file_path = Path.Combine(thePath, (string)filename);
                CommonFunctionality.Instance.logger.LogIt(String.Format("Pasting item '{0}' to {1}", filename, full_file_path));

                string download_url = CommonFunctionality.Instance.sharefile.GetDownLoadURL(CommonFunctionality.Instance.sharefile.GetItemId(@"/" + CommonFunctionality.Instance.username + @"/" + CommonFunctionality.UPLOADS_FOLDER, (string)filename, SharefileCCP.ITEM_TYPE.FILE));
                DownloadFileWithProgress(download_url, full_file_path);
                

            
        }

        /// <summary>
        /// Pulls the text out of the clipboard and sends it up to the cloud.
        /// </summary>
        /// <exception cref="System.Exception">Unable to Initialize</exception>
        public void CloudClipboardCopy()
        {                        
            if (!CommonFunctionality.Instance.Initialise())
                throw new Exception("Unable to Initialize");

            CommonFunctionality.Instance.Authenticate(false);
                        
            String ClipboardText = ClipboardHelper.GetClipboardText();



            String filename = ClipboardText.PadRight(CommonFunctionality.CLIPBOARD_PREVIEW_LENGTH, '_') // if the text in the clipboard is too small, make up for it.(add padding):
                                           .Substring(0, CommonFunctionality.CLIPBOARD_PREVIEW_LENGTH)  // we make the filename part of the clipboard entry...makes for easy preview access too!
                                           + CommonFunctionality.CLIPBOARD_OUT_EXT; 
                                       

            foreach (char ch in Path.GetInvalidFileNameChars())
            {
                if (!filename.Contains(ch))
                    continue;

                filename = filename.Replace(ch, '_');
            }

            if (File.Exists(filename))
                return;

            // Ensure that this clipboard event we've found ourselves handling is not because the extension swapped out the clipboard file into the vlipboard,
            // tihs would be the case if there is a .in file with the same name as the one we'r eintending to create, dont do this.

            if (File.Exists(Path.Combine(CommonFunctionality.Instance.common_path,Path.GetFileNameWithoutExtension(filename) + CommonFunctionality.CLIPBOARD_IN_EXT)))
                return;


            using( TextWriter tw = File.CreateText(Path.Combine(CommonFunctionality.Instance.common_path, filename)))
            {  
                if(!String.IsNullOrEmpty(ClipboardText))
                    tw.Write(ClipboardText);           
            }

            // get all pending clipboard files and send them off
            string where = CommonFunctionality.Instance.common_path;
            List<string> load_clipboard_files = new List<string>(Directory.EnumerateFiles(where));
            bool worked = false;
            IEnumerable<string> enumerable = load_clipboard_files.Where(item => item.EndsWith(CommonFunctionality.CLIPBOARD_OUT_EXT)).AsEnumerable();


            // upload these .co files

            foreach (var filepath in enumerable)
            {
                int retry_count = 3;
                while (!uploadFile(filepath, filename) &&  retry_count != 0)
                {
                    retry_count--;
                }
                retry_count = 0; //reset 
                
            }                        
        }

        private bool uploadFile(string filepath, string filename)
        {
            Dictionary<string, object> optionalparams = new Dictionary<string, object>();
            optionalparams.Add("folderid", CommonFunctionality.Instance.uploadfolderId);

            CommonFunctionality.Instance.logger.LogIt(string.Format("uploading clipboard file '{0}' to upload folder({1})...", filepath, CommonFunctionality.Instance.uploadfolderId));

            string fname = Path.GetFileName(filepath);
            if (CommonFunctionality.Instance.sharefile.DoesFileExist(fname, CommonFunctionality.UPLOADS_FOLDER, CommonFunctionality.Instance.username))
                return true; // treat this as a successful upload even though we didnt do it.

            string retval = CommonFunctionality.Instance.sharefile.FileUpload(filepath, optionalparams);

            if (!string.IsNullOrEmpty(retval) && !retval.Contains("OK:"))
            {

                CommonFunctionality.Instance.logger.LogItAsError("failed to upload clipboard file " + filepath);
                return false; // there was an issue move to the next file. dont delete it, we;ll try again next time.
            }
            else
            {
                // uploaded fine, lets delete it locally then
                string delete_file_full_path = Path.Combine(CommonFunctionality.Instance.common_path, filepath);
                File.Delete(delete_file_full_path);
                return true;
            }
        }


        /// <summary>
        /// Finds the clipboard file locally, and if it doesnt exist gets it from the cloud, extracts its contents and adds it
        /// to the clipboard as text.
        /// </summary>
        /// <param name="file">The selected clipboard entry the user clicked on(which secretly is part of the clipboard file name)</param>
        /// <exception cref="System.Exception">
        /// Unable to initialise.
        /// or
        /// Unable to obtain clipboard information for  + local_clip_file
        /// </exception>
        public void CloudClipboardPaste(String file)
        {
            CommonFunctionality.Instance.Authenticate(false);
                        
            /* Read the contents of the selected clipboard file, and swap into clipboard. */

            String local_clip_file = Path.Combine(CommonFunctionality.Instance.common_path,
                                                   Path.GetFileNameWithoutExtension(file) + CommonFunctionality.CLIPBOARD_IN_EXT);
            int retry_count = 3;
            
            while (!File.Exists(local_clip_file) && retry_count != 0)
            {
                try
                {
                    // download tihs clipboard file as we dont have it.
                    string look_up_file = Path.GetFileNameWithoutExtension(file) + CommonFunctionality.CLIPBOARD_OUT_EXT;
                    string filename = @"/" + CommonFunctionality.Instance.username + @"/" + CommonFunctionality.UPLOADS_FOLDER + @"/" + look_up_file;
                    String item_id_str = CommonFunctionality.Instance.sharefile.GetItemId(filename, SharefileCCP.ITEM_TYPE.FILE);
                    if (String.IsNullOrEmpty(item_id_str))
                    {
                        string error = "Unable to get file id for " + filename;
                        CommonFunctionality.Instance.logger.LogItAsError(error);
                        throw new Exception(error);
                    }
                    CommonFunctionality.Instance.sharefile.FileDownload(item_id_str, local_clip_file);

                }
                catch (Exception error)
                {
                    // try again.
                }
                finally
                {
                    retry_count--;
                }
            }

            //FIXME: Currently if clipboard mointor is not running, we need to fetch all the clipboard files on the server
            

            // read the clipboard file and swap its contants into the clipboard.

            String data = File.ReadAllText(local_clip_file);
            ClipboardHelper.SwapClipboardFormattedData(DataFormats.StringFormat, data);
            
        }

     

        /// <summary>
        /// Fetches the awaiting clipboard files.
        /// </summary>
        public void FetchAwaitingClipboardFiles()
        {
            FetchSpecificAwaitingClipboardFiles(GetWaitingFiles());
        }
        /// <summary>
        /// Pull down all the waiting clipoard files and rename them to .ci files. This makes them lcoally available for processing
        /// bu the shell extension which looks for .ci files to show as clipboard entries
        /// </summary>
        public void FetchSpecificAwaitingClipboardFiles(IEnumerable<String> filenames)
        {
            CommonFunctionality.Instance.Authenticate(false);
            IEnumerable<String> files = filenames;

            foreach (string filename in files)
            {
                // Of the files waiting we only want clipboard ones...
                if (!filename.EndsWith(CommonFunctionality.CLIPBOARD_OUT_EXT))
                    continue;

                // Prepare a local filename for it to be saved as...
                string fileId = GetFileId(filename);
                string newfilepath = Path.GetFileNameWithoutExtension(Path.Combine(CommonFunctionality.Instance.common_path, filename));
                // give it the IN extension to denote that we've downloaded it and its incomming for processing by clipboard extension.
                newfilepath += CommonFunctionality.CLIPBOARD_IN_EXT;

                // Download the incomming file
                try
                {
                    string newpath = Path.Combine(CommonFunctionality.Instance.common_path, newfilepath);
                    if (!File.Exists(newpath))
                    {
                        CommonFunctionality.Instance.sharefile.FileDownload(fileId, newpath);                        
                    }
                    else
                    {
                        CommonFunctionality.Instance.logger.LogIt(String.Format("{0} already exists locally not pulling it down.", newpath));
                    }
                }
                catch (Exception e)
                {
                    // move on to the next file to download
                    CommonFunctionality.Instance.logger.LogItAsError(e.Message);
                }

            }
        }

        private static string GetFileId(string filename)
        {
            string fileId = CommonFunctionality.Instance.sharefile.GetItemId(@"/" + CommonFunctionality.Instance.username + @"/" + CommonFunctionality.UPLOADS_FOLDER, (string)filename, SharefileCCP.ITEM_TYPE.FILE);
            return fileId;
        }

        /// <summary>
        /// Gets a list of the waiting file nanes on the cloud that can be downloaded
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetWaitingFiles()
        {
            CommonFunctionality.Instance.Authenticate(false);
            IEnumerable<String> files = CommonFunctionality.Instance.sharefile.FolderList(@"/" + CommonFunctionality.Instance.username + @"/" + CommonFunctionality.UPLOADS_FOLDER);
            return files;
        }

        

       

        
        /// <summary>
        /// Performs the upload to the cloud.
        /// </summary>
        /// <param name="uploadSuccessful">if set to <c>true</c> [upload successful].</param>
        /// <param name="filename">The filename.</param>
        public void performUpload(ref bool uploadSuccessful,  IEnumerable<String> SelectedItemPaths)
        {
            //TODO: This should be done aynchronlusly and in its own thread per upload.
            //TODO: Support multiple file uploads
            Dictionary<string, object> optionalParams = new Dictionary<string, object>();
            optionalParams.Add("folderid", CommonFunctionality.Instance.uploadfolderId);
                                                
            if (SelectedItemPaths.Count<string>() < 1)
                throw new Exception("No items selected");

            foreach (var filePath in SelectedItemPaths)
            {
                CommonFunctionality.Instance.logger.LogIt(String.Format("Uploading file '{0}' ...", filePath));

                String filename = Path.GetFileName(filePath);

                string retVal = CommonFunctionality.Instance.sharefile.FileUpload(filePath, optionalParams);
                if (!string.IsNullOrEmpty(retVal) && !retVal.Contains("OK:"))
                {
                    uploadSuccessful = false;
                    break;
                }
            }
        }

        

        /// <summary>
        /// Gets the files available on cloud which can be pasted.
        /// </summary>
        /// <returns></returns>
        private List<object> GetFilesToDownload()
        {
            List<object> files = new List<object>();

            FunctionalityLibrary.UserInterfaces.UploadedFiles uploadedfilesDlg = new FunctionalityLibrary.UserInterfaces.UploadedFiles(CommonFunctionality.Instance.sharefile.FolderList(@"/" + CommonFunctionality.Instance.username + @"/" + CommonFunctionality.UPLOADS_FOLDER));
            System.Windows.Forms.DialogResult res = uploadedfilesDlg.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                files = uploadedfilesDlg.Files;
            }
            return files;
        }

        private void DownloadFileWithProgress(string URL, string save_filename)
        {
            CSWebDownloader.DownloadForm form = new CSWebDownloader.DownloadForm(URL, save_filename);
            form.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            form.Show();
            form.BeginDownload();

        }

        /// <summary>
        /// Checks the internet connection.
        /// </summary>
        /// <exception cref="System.Exception">No internet connection detected</exception>
        private void CheckInternetConnection()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    ;
                }
            }
            catch
            {
                throw new Exception("No internet connection detected");
            }
        }

    }
}
