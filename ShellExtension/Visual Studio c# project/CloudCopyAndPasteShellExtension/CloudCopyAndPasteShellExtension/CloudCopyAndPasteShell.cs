#define DEV_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using Nini.Config;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Serialization;
using FunctionalityLibrary;
using System.Drawing;


namespace CloudCopyAndPasteShellExtension
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Directory)]
    [COMServerAssociation(AssociationType.Class, "Directory\\Background")]
    [COMServerAssociation(AssociationType.AllFiles)]
    public class TheShellExtension : SharpContextMenu
    {
        const string VERSION = "1.0alpha";
        private static Cloud cloud = new FunctionalityLibrary.Cloud();
        private static UserInterface forms = new FunctionalityLibrary.UserInterface();
        private bool isUsingClipboardMonitor = false;


        /// <summary>
        /// Determines whether this instance [can show menu] - the first method that the newly initialised shell extension calls
        /// </summary>
        /// <returns></returns>
        protected override bool CanShowMenu()
        {
            try
            {
                CommonFunctionality.Instance.Initialise();
                CommonFunctionality.Instance.logger.LogIt(String.Format("Version {0} shell extension initialised.", GetVersion()));

                return true; // we always determine to show the menu as part of the shell extension
            }
            catch (Exception error) // This is the highest program catch point for CanShowMenu() as this function is called externally as an entry point. Cant throw any higher.
            {
                string msg = error.Message;
                forms.ShowUserMessage(msg, CommonFunctionality.SEVERITY.High);
                return false;
            }
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <returns></returns>
        private string GetVersion()
        {
            return VERSION;
        }

        /// <summary>
        /// Creates the menu and shows it to the user
        /// </summary>
        /// <returns></returns>
        protected override System.Windows.Forms.ContextMenuStrip CreateMenu()
        {
            //  Create the menu strip.
            var menu = new ContextMenuStrip();
            try
            {
                ConfigureShellExtension(menu); // Populates the context menu that is shown in Explorer                
            }
            catch (System.Net.WebException webex)
            {
                // Problem could not get online, this isn't critical so dont show the user the problem, just silently ignore it
                CommonFunctionality.Instance.logger.LogItAsError("Problem with the web? - " + webex.Message);
            }
            catch (Exception e)
            {
                PrintLogCritialException(e);
            }

            return menu;
        }

        /// <summary>
        /// Prints the log critial exception.
        /// </summary>
        /// <param name="e">The decimal.</param>
        private void PrintLogCritialException(Exception e)
        {
            String technical_msg = DateTime.Now.ToShortDateString() + " "
                    + DateTime.Now.ToShortTimeString() + " "
                    + " Catastrophic error while creating shell extension: "
                    + e.Message
                    + e.StackTrace;

            CommonFunctionality.Instance.logger.LogItAsError(technical_msg);
            FunctionalityLibrary.UserInterfaces.ErrorForm form = new FunctionalityLibrary.UserInterfaces.ErrorForm();
            form.Message = e.Message;
            form.Technical = technical_msg;
            form.ShowDialog();

            LogError(e.Message, e); // Built into Sharpshell
        }

        /// <summary>
        /// Configures the shell extension menu and sets on click events
        /// </summary>
        /// <param name="menu">The menu.</param>
        /// <exception cref="System.Exception">Unable to setup cntent menu :  + creationError.Message</exception>
        private void ConfigureShellExtension(ContextMenuStrip menu) // throws Exception[
        {
            /* Create content menu */
            try
            {
                try
                {
                    var cloudcopyandpaste = new ToolStripMenuItem { Text = "Cloud copy/paste" };
                    cloudcopyandpaste.Image = Images.cloud16;
                    menu.Items.Add(cloudcopyandpaste);

                    var cloudclipboard = new ToolStripMenuItem { Text = "Clipboard" };
                    cloudclipboard.Image = Images.clipboard_empty;

                    if (CommonFunctionality.processIsRunning("ClipboardMonitor"))
                    {
                        CommonFunctionality.Instance.logger.LogIt("Found ClipboardMonitor, using it...");
                        isUsingClipboardMonitor = true;

                    }
                    else
                    {

                        // first give the user the ability to manually copy the clkipboard contents...
                        var cloudClipboardCopyMenuItem = new ToolStripMenuItem { Text = "Clipboard copy" };
                        cloudClipboardCopyMenuItem.Image = Images.clipboard_copy;
                        cloudclipboard.DropDownItems.Add(cloudClipboardCopyMenuItem);
                        cloudClipboardCopyMenuItem.Click += (sender, args) => CloudClipboardCopy();

                        isUsingClipboardMonitor = false;

                    }

                    var cloudClipboardPasteMenuItem = new ToolStripMenuItem { Text = "Clipboard paste" };
                    cloudClipboardPasteMenuItem.Image = Images.clipboard_paste;

                    var cloudfilestorage = new ToolStripMenuItem { Text = "File" };
                    cloudfilestorage.Image = Images.file;

                    var cloudCopyMenuItem = new ToolStripMenuItem { Text = "File copy" };
                    cloudCopyMenuItem.Image = Images.clipboard_copy;

                    var cloudPasteMenuItem = new ToolStripMenuItem { Text = "File paste" };
                    cloudPasteMenuItem.Image = Images.clipboard_paste;


                    PopulateDynamicMenuItems(cloudPasteMenuItem, cloudClipboardPasteMenuItem);

                    /* Setup event handlers for the various options in the content menu. */


                    cloudCopyMenuItem.Click += (sender, args) => CloudCopyItem();


                    bool click_on_empty_space = SelectedItemPaths.Count<string>() > 0;
                    // If we clicked on background of folder?


                    //  Finally add the menu items to the context menu.                
                    if (click_on_empty_space) // can only copy selected items
                        cloudfilestorage.DropDownItems.Add(cloudCopyMenuItem);


                    if (cloudClipboardPasteMenuItem.DropDownItems.Count > 0)
                        cloudclipboard.DropDownItems.Add(cloudClipboardPasteMenuItem);

                    if (cloudPasteMenuItem.DropDownItems.Count > 0)
                        cloudfilestorage.DropDownItems.Add(cloudPasteMenuItem);

                    cloudcopyandpaste.DropDownItems.Add(cloudclipboard);
                    cloudcopyandpaste.DropDownItems.Add(cloudfilestorage);
                }
                catch (System.Net.WebException webex)
                {
                    throw new System.Net.WebException("Problem configuring the shell extension:" + webex.Message);
                }
                catch (Exception)
                {
                    // last ditch effort is to reauthenticate
                    CommonFunctionality.Instance.Authenticate(true);
                    ConfigureShellExtension(menu);
                }
            }
            catch (Exception creationError)
            {
                // throw it up higher
                throw new Exception(
                    "Unable to setup cloud copy and paste shell extension menu : " + creationError.Message,
                    creationError);
            }
        }





        /// <summary>
        /// Get all the files to download(clipboard entries) and add them as paste cli[pboard sub items (shell) menu.
        /// </summary>
        /// <param name="cloudPasteMenuItem">The cloud paste menu item.</param>
        /// <param name="cloudClipboardPasteMenuItem">The cloud clipboard paste menu item.</param>
        private void PopulateDynamicMenuItems(ToolStripMenuItem cloudPasteMenuItem, ToolStripMenuItem cloudClipboardPasteMenuItem)
        {

            CommonFunctionality.Instance.Authenticate(false);


            // Get a list of all the files in the upload folder

            List<string> all_filenames = cloud.GetWaitingFiles().ToList(); // we always get a list of files from the server. bit of a hit each time, i guess

            // We need to get any files that have been sent up to the cloud clipboard.            
            //cloud.FetchSpecificAwaitingClipboardFiles(all_filenames); // update local clipboard files

            // Get all files(not clipboard files) in the upload cloud folder on the server/cloud...

            AddCloudFileNames(cloudPasteMenuItem, all_filenames);

            // Get references to all clipboard files that are waiting to be downloaded...
            List<string> clips_waiting_filenames = all_filenames.Where(item => item.EndsWith(CommonFunctionality.CLIPBOARD_OUT_EXT)).ToList(); // eg. "People inclanaction.co" as stored on the cloud

            MergeLocalAndCloudClipboardFiles(cloudClipboardPasteMenuItem, clips_waiting_filenames);
        }

        /// <summary>
        /// This ensures that files on the cloud and in the local cache are both 'looked at' and considered when adding item to the menu.
        /// Duplicates should not exist.
        /// </summary>
        /// <param name="cloudClipboardPasteMenuItem"></param>
        /// <param name="clips_waiting_filenames"></param>
        private void MergeLocalAndCloudClipboardFiles(ToolStripMenuItem cloudClipboardPasteMenuItem, List<string> clips_waiting_filenames)
        {

            AddOnlineCloudClipboardFileNames(cloudClipboardPasteMenuItem, clips_waiting_filenames);
            AddLocalCacheClipboadFileNames(cloudClipboardPasteMenuItem, clips_waiting_filenames);
        }

        /// <summary>
        /// Adds the cloud file names, on the cloud as items in the drop down menu. These are purely references to files on the server, they may or may not
        /// actually exist locally yet.
        /// </summary>
        /// <param name="cloudPasteMenuItem">The cloud paste menu item.</param>
        /// <param name="all_filenames">The all_filenames.</param>
        private void AddCloudFileNames(ToolStripMenuItem cloudPasteMenuItem, List<string> all_filenames)
        {
            List<string> normal_filenames = all_filenames.Where(item => !item.EndsWith(CommonFunctionality.CLIPBOARD_IN_EXT)
                                                                     && !item.EndsWith(CommonFunctionality.CLIPBOARD_OUT_EXT)).ToList();

            // Populate the pasteable files sub menu under cloud paste

            int filecount = normal_filenames.Count;
            ToolStripMenuItem[] items = new ToolStripMenuItem[filecount]; // You would obviously calculate this value at runtime
            for (int i = 0; i < items.Length; i++)
            {


                items[i] = new ToolStripMenuItem();

                items[i].Image = GetImageByName(GetImageNameFromFilename(normal_filenames[i]));

                if (items[i].Image == null)
                    items[i].Image = Images.txt;

                items[i].Name = normal_filenames[i];
                items[i].Tag = normal_filenames[i];
                items[i].Text = normal_filenames[i];
                items[i].Click += download_click_file; // Register what will happen when the user clicks on a pasteable file.
            }

            cloudPasteMenuItem.DropDownItems.AddRange(items); //add 'em 
        }

        private Bitmap GetImageByName(string name)
        {
            return (Bitmap)Images.ResourceManager.GetObject(name);
        }

        private string GetImageNameFromFilename(string filename)
        {
            string file_ext = Path.GetExtension(filename);
            switch (file_ext)
            {
                case ".xlsx": file_ext = ".xls"; break;
                case ".docx": file_ext = ".doc"; break;
                case ".pptx": file_ext = ".ppt"; break;
            }
            file_ext = file_ext.TrimStart('.');



            return file_ext;
        }

        /// <summary>
        /// Adds the cloud clipboard file names to the drop down. these may or may not exist locally yet.
        /// </summary>
        /// <param name="cloudClipboardPasteMenuItem">The cloud clipboard paste menu item.</param>
        /// <param name="clips_waiting_filenames">The clips_waiting_filenames.</param>
        private void AddOnlineCloudClipboardFileNames(ToolStripMenuItem cloudClipboardPasteMenuItem, List<string> clips_waiting_filenames)
        {
            // scan through results and turn them into dropdown items...
            ToolStripMenuItem[] clips = new ToolStripMenuItem[clips_waiting_filenames.Count];
            for (int i = 0; i < clips.Length; i++)
            {
                String preview = clips_waiting_filenames[i].Substring(0, CommonFunctionality.CLIPBOARD_PREVIEW_LENGTH);
                clips[i] = new ToolStripMenuItem();
                clips[i].Name = preview; // this becomes the key for the subitem dropdown
                clips[i].Tag = clips_waiting_filenames[i];
                clips[i].Image = Images.txt;
                // We use the first 0 - CommonFunctionality.CLIPBOARD_PREVIEW_LENGTH chars of the file name as it also serves as preview text
                clips[i].Text = preview;
                clips[i].Click += SwapClipBoardContents;

                // add what we've found to the dropdown list...
                if (!cloudClipboardPasteMenuItem.DropDown.Items.ContainsKey(preview))
                    cloudClipboardPasteMenuItem.DropDown.Items.Add(clips[i]);
            }


        }

        /// <summary>
        /// look locally for the files that aren't waiting(already downloaded) but are available
        /// </summary>
        /// <param name="cloudClipboardPasteMenuItem">The cloud clipboard paste menu item.</param>
        /// <param name="clips_waiting_filenames">The clips_waiting_filenames.</param>
        private void AddLocalCacheClipboadFileNames(ToolStripMenuItem cloudClipboardPasteMenuItem, List<string> clips_waiting_filenames)
        {

            List<string> localclips_filenames = Directory.EnumerateFiles(CommonFunctionality.Instance.common_path).ToList().Where(item => item.EndsWith(CommonFunctionality.CLIPBOARD_IN_EXT)).ToList();
            int localclipscount = localclips_filenames.Count;
            ToolStripMenuItem[] localclips = new ToolStripMenuItem[localclipscount];
            for (int i = 0; i < localclips.Length; i++)
            {
                string preview = Path.GetFileName(localclips_filenames[i]).Substring(0, CommonFunctionality.CLIPBOARD_PREVIEW_LENGTH);

                localclips[i] = new ToolStripMenuItem();
                localclips[i].Name = preview; // this becomes the key for the subitem dropdown
                localclips[i].Tag = localclips_filenames[i];
                localclips[i].Image = Images.txt;
                localclips[i].Text = preview;
                localclips[i].Click += SwapClipBoardContents;
                // Top up the list if we've not already go it from the 
                if (!cloudClipboardPasteMenuItem.DropDownItems.ContainsKey(preview)) // ensure we're not adding something we added already from the online cloud upload folder contents that we also have as a local file cache.
                    cloudClipboardPasteMenuItem.DropDownItems.Add(localclips[i]);
            }

        }

        /// <summary>
        /// Swaps the clip board contents.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void SwapClipBoardContents(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string filename = (string)item.Tag;
            try
            {
                cloud.CloudClipboardPaste(filename);
            }
            catch (Exception)
            {
                // retry by reauthenticating - last hope
                CommonFunctionality.Instance.Authenticate(true);
                cloud.CloudClipboardPaste(filename);
            }
        }

        /// <summary>
        /// Handles the file event of the download_click control.
        /// Download/paste this file...
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void download_click_file(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string filename = item.Text;
            try
            {
                // Do the actual paste - fetch file from cloud and download/paste it at the current location
                cloud.CloudPasteItem(FolderPath, filename);
            }
            catch (Exception)
            {
                CommonFunctionality.Instance.Authenticate(true); // try again by reauthenticating
                cloud.CloudPasteItem(FolderPath, filename);
            }
        }

        /// <summary>
        /// Thread 
        /// </summary>
        private delegate void CloudClipboardCopyDelegate();
        private void beginClipboardCopy()
        {
            CloudClipboardCopyDelegate thread = cloud.CloudClipboardCopy;
            thread.BeginInvoke(null, null); // go and dont wait up, upload!
        }



        /// <summary>
        /// Cloud clipboard copy event handler
        /// </summary>
        private void CloudClipboardCopy()
        {
            try
            {
                try
                {
                    if (!CommonFunctionality.processIsRunning("ClipboardMonitor"))
                        beginClipboardCopy();
                }
                catch (Exception e)
                {
                    CommonFunctionality.Instance.Authenticate(true); // reauthenticate - session timeout?
                    if (!CommonFunctionality.processIsRunning("ClipboardMonitor"))
                        beginClipboardCopy();
                }
            }
            catch (Exception e)
            {
                PrintLogCritialException(e);
            }
        }

        /// <summary>
        /// Thread to process copy items work
        /// </summary>
        /// <param name="SelectedItemPaths"></param>
        public delegate void CloudCopytItemDelegate(IEnumerable<string> SelectedItemPaths);

        /// <summary>
        /// Clouds the copy item.
        /// </summary>
        private void CloudCopyItem()
        {
            try
            {
                try
                {
                    // Copies the selected files under the cursor to the cloud

                    // Starts off a thread to copy the item without blocking the main thread.
                    CloudCopytItemDelegate thread = cloud.CloudCopyItem;
                    thread.BeginInvoke(SelectedItemPaths, null, null);
                }
                catch (Exception e)
                {
                    CommonFunctionality.Instance.Authenticate(true); // reauthenticate and try again
                    cloud.CloudCopyItem(SelectedItemPaths);
                }
            }
            catch (Exception e)
            {
                PrintLogCritialException(e);
            }
        }


        /// <summary>
        /// Serializes the base64.
        /// </summary>
        /// <param name="o">The automatic.</param>
        /// <returns></returns>
        public static string SerializeBase64(object o)
        {
            // Serialize to a base 64 string
            byte[] bytes;
            long length = 0;
            MemoryStream ws = new MemoryStream();
            BinaryFormatter sf = new BinaryFormatter();
            sf.Serialize(ws, o);
            length = ws.Length;
            bytes = ws.GetBuffer();
            string encodedData = bytes.Length + ":" + Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None);
            return encodedData;

        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="fileName"></param>
        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                //Log exception here
            }
        }

        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                string attributeXml = string.Empty;

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception ex)
            {
                //Log exception here
            }

            return objectOut;
        }


    }
}
