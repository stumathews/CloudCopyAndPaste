using Nini.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharefileCCPLib;

namespace FunctionalityLibrary
{

    /// <summary>
    /// Common functionality that is passed around and re-used
    /// C\n be tested outside the shell extension
    /// </summary>
    public sealed class CommonFunctionality
    {
        private static volatile CommonFunctionality instance;
        private static object syncRoot = new Object();

        public string username = null;
        public string password = null;
        public string subdomain = null;
        public string domain = null;

        public const string UPLOADS_FOLDER = "Uploads";
        public const string CONFIG_FILE_NAME = "cloudconfig.ini";
        public const string LOG_FILE_NAME = "cloudcopylog.log";
        public const string CLIPBOARD_OUT_EXT = ".co";
        public const string CLIPBOARD_IN_EXT = ".ci";
        public const int CLIPBOARD_PREVIEW_LENGTH = 16;
                
        public enum SEVERITY { Low, Medium, High };

        public string common_path = null;
        public bool _isLoggedIn = false;
        public bool _isUploadFolderSetupAlready = false;
        public string uploadfolderId = null;
        private bool _initialised = false;
        private bool _fetchSettings = false;

        public IConfigSource config = null;                 // config.ini API wrapper
        public Logger logger = new Logger();
        public SharefileCCP sharefile = new SharefileCCP();
        
        

        /// <summary>
        /// Initialises, prepares and loads all members with persistant data accessibe for the duration of the process running it
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// You are missing the configuration file with cloud provider details. Expected file: + config_file_path
        /// or
        /// </exception>
        public bool Initialise()
        {
            
            if (_initialised)
                return true;
                        
            try
            {

                // Find and load the Configuration file...

                String config_file_exepected_location = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                CommonFunctionality.Instance.common_path = config_file_exepected_location;

                String config_file_path = Path.Combine(config_file_exepected_location, CommonFunctionality.CONFIG_FILE_NAME);

                if (!File.Exists(config_file_path))
                    throw new Exception("You are missing the configuration file with cloud provider details. Expected file:" + config_file_path);

                // check if the config file is loaded
                if (CommonFunctionality.Instance.config == null)
                {
                    CommonFunctionality.Instance.config = new IniConfigSource(Path.Combine(config_file_exepected_location, CommonFunctionality.CONFIG_FILE_NAME)); //load configuration file as object
                    logger.LogIt("Loaded configuration file: " + config_file_path);
                }
                
                // Check if common folder is setup and stored/saved
                if (CommonFunctionality.Instance.common_path == null)
                {
                    CommonFunctionality.Instance.common_path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                    logger.LogIt("Common folder prepared.");
                }                                            
                
            }
            catch (Exception initError)
            {
                String error = "Problem trying to initialise shell extension: " + initError.Message;
                throw new Exception(error, initError);
            }            
            _initialised = true;
            return true; // Yep initialise went ok, shell extension should have everything it needs to function moving forward...
        }

        /// <summary>
        /// Create upload folder if its not there in share file account
        /// </summary>
        private string GetCreateUploadFolder() // throws Exception
        {
            try
            {
                if (!sharefile.DoesFolderExist(@"/" + CommonFunctionality.Instance.username + @"/", CommonFunctionality.UPLOADS_FOLDER))
                    uploadfolderId = sharefile.CreateFolder(CommonFunctionality.Instance.username, CommonFunctionality.UPLOADS_FOLDER);
                else
                    uploadfolderId = sharefile.GetItemId(@"/" + CommonFunctionality.Instance.username + @"/", CommonFunctionality.UPLOADS_FOLDER, SharefileCCP.ITEM_TYPE.FOLDER);


                if (uploadfolderId == String.Empty)
                    throw new Exception("Could not obtain upload folder id. Empty folder ID.");

                // We persist this so other functions can use it.            
                CommonFunctionality.Instance.config.Configs["sharefile"].Set("uploadid", uploadfolderId);
                CommonFunctionality.Instance.config.Save();

                _isUploadFolderSetupAlready = true;
                logger.LogIt("Creating/Setting-up upload folder complete.");
            }
            catch (Exception error)
            {
                _isUploadFolderSetupAlready = false;
                throw new Exception("Unable to Create Upload Folder.", error);
            }

            return uploadfolderId;
        }

        /// <summary>
        /// Looks up details stored in the configuration file, used by cloud copy.
        /// </summary>
        private void FetchSettings() // throws Exception
        {
            // Some sanity checks here
            if (_fetchSettings)
                return;
            
            /* Get all details that we'll need to operate moving forward. */

            CommonFunctionality.Instance.username = CommonFunctionality.Instance.config.Configs["sharefile"].Get("username");
            CommonFunctionality.Instance.password = CommonFunctionality.Instance.config.Configs["sharefile"].Get("password");
            CommonFunctionality.Instance.subdomain = CommonFunctionality.Instance.config.Configs["sharefile"].Get("subdomain");
            CommonFunctionality.Instance.domain = CommonFunctionality.Instance.config.Configs["sharefile"].Get("domain");
            CommonFunctionality.Instance.uploadfolderId = CommonFunctionality.Instance.config.Configs["sharefile"].Get("uploadid");

            /* Santity checks: */

            if (String.IsNullOrEmpty(CommonFunctionality.Instance.username) || 
                String.IsNullOrEmpty(CommonFunctionality.Instance.password) || 
                String.IsNullOrEmpty(CommonFunctionality.Instance.subdomain) || 
                String.IsNullOrEmpty(CommonFunctionality.Instance.domain))
            {
                string message = "It appearsthat the username is blank, or unable to read cloudconfig.ini file. Latter file expected located in " + CommonFunctionality.Instance.common_path;
                throw new Exception(message);
            }

            _fetchSettings = true;
        }

        /// <summary>
        /// Authenticates the sharefile library and fetches persistant data in config file.
        /// Doesn't reauthenticate if we've already authenticated.
        /// </summary>
        public bool Authenticate(bool forceReAuthenticate)
        {           

            if (!_isLoggedIn || forceReAuthenticate)
            {
                // Ensure we have the persisted settings loaded from config file - this ignores them if we have them already.
                FetchSettings();

                if (sharefile.Authenticate(CommonFunctionality.Instance.subdomain, 
                                            CommonFunctionality.Instance.domain, 
                                            CommonFunctionality.Instance.username, 
                                            CommonFunctionality.Instance.password) != SharefileCCP.HRESULT.S_OK)
                {
                    _isLoggedIn = false;
                    throw new Exception(sharefile.FriendlyError + " - " + sharefile.TechnicalError);
                }

                _isLoggedIn = true;                
            }
            
            /* Prepare the upload folder, unless its already prepared. */

            if (!_isUploadFolderSetupAlready)
            {
                if (String.IsNullOrEmpty(GetCreateUploadFolder()))
                {
                    _isUploadFolderSetupAlready = false;
                    throw new Exception("Upload file id is null or empty.");
                }
            }            

            return true;
        }

        /// <summary>
        /// Checks to see if the named process is running
        /// </summary>
        /// <returns></returns>
        public static bool processIsRunning(String processname)
        {
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(processname);
            //foreach (System.Diagnostics.Process proc in processes)
            //{
            //    Console.WriteLine("Current physical memory : " + proc.WorkingSet64.ToString());
            //    Console.WriteLine("Total processor time : " + proc.TotalProcessorTime.ToString());
            //    Console.WriteLine("Virtual memory size : " + proc.VirtualMemorySize64.ToString());
            //}
            return (processes.Length != 0);
        }


        /// <summary>
        /// Gets the Singleton instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static CommonFunctionality Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new CommonFunctionality();
                    }
                }

                return instance;
            }
        }
    }
    
}
