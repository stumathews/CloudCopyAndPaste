using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Windows.Forms;


/// Copyright (c) 2013 Citrix Systems, Inc.
///
/// Permission is hereby granted, free of charge, to any person obtaining a 
/// copy of this software and associated documentation files (the "Software"),
/// to deal in the Software without restriction, including without limitation 
/// the rights to use, copy, modify, merge, publish, distribute, sublicense, 
/// and/or sell copies of the Software, and to permit persons to whom the 
/// Software is furnished to do so, subject to the following conditions:
///
/// The above copyright notice and this permission notice shall be included in 
/// all copies or substantial portions of the Software.
///
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
/// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
/// IN THE SOFTWARE.


namespace SharefileCCPLib
{
    /// <summary>
    /// Methods in this class make use of ShareFile API. Please see api.sharefile.com for more information.
    /// Requirements:
    ///
    /// Json.NET library. see http://json.codeplex.com/
    ///
    /// Optional parameters can be passed to functions as a Dictionary as follows:
    ///
    /// ex:
    /// 
    /// Dictionary<string, object> optionalParameters = new Dictionary<string, object>();
    /// optionalParameters.Add("company", "ACompany");
    /// optionalParameters.Add("password", "Apassword");
    /// optionalParameters.Add("addshared", true);
    /// sample.usersCreate("firstname", "lastname", "an@email.com", false, optionalParameters);
    /// 
    /// See api.sharefile.com for optional parameter names for each operation.
    /// </summary>
    public class SharefileCCP  
    {
        public enum ITEM_TYPE
        {
            FOLDER=0,
            FILE=1
        }

        public enum HRESULT
        {
            E_FAIL=-1,
            S_OK=0,
            S_FALSE=1
        }

        string subdomain;
        string tld;
        string authId;

        //error information
        public string FriendlyError { get; set; }
        public string TechnicalError { get; set; }

        public string CLIPBOARD_BIN_FILE
        {
            get { return "Clipboard.bin"; }
        }

        public string CLIPBOARD_TEXT_FILE
        {
            get { return "Clipboard.txt"; }
        }

        /// <summary>
        /// Calls getAuthID to retrieve an authid that will be used for subsequent calls to API.
        /// 
        /// If you normally login to ShareFile at an address like https://mycompany.sharefile.com, 
        /// then your subdomain is mycompany and your tld is sharefile.com
        /// </summary>
        /// <param name="subdomain">your subdomain</param>
        /// <param name="tld">your top level domain</param>
        /// <param name="username">your username</param>
        /// <param name="password">your password</param>
        /// <returns>true if login was successful, false otherwise.</returns>
        public HRESULT Authenticate(string subdomain, string tld, string username, string password)
        {
            HRESULT hr = HRESULT.S_OK;
            FriendlyError = string.Empty; TechnicalError = string.Empty;

            try
            {
                this.subdomain = subdomain;
                this.tld = tld;

                string requestUrl = string.Format("https://{0}.{1}/rest/getAuthID.aspx?fmt=json&username={2}&password={3}",
                    subdomain, tld, HttpUtility.UrlEncode(username), HttpUtility.UrlEncode(password));
                Console.WriteLine(requestUrl);

                JObject jsonObj = InvokeShareFileOperation(requestUrl);
                if (!(bool)jsonObj["error"])
                {
                    string authId = (string)jsonObj["value"];
                    this.authId = authId;
                    hr = HRESULT.S_OK;
                }
                else
                {
                    Console.WriteLine(jsonObj["errorCode"] + " : " + jsonObj["errorMessage"]);
                    FriendlyError = "Login failed.\r\nPlease check your configuration settings.";
                    hr = HRESULT.S_FALSE;
                }
            }
            catch (WebException webError)
            {
                throw new WebException("Problem with internet connection?", webError);
            }
            catch (Exception ex)
            {
                FriendlyError = "Login failed.";
                CCPException ccpEx = new CCPException("Authentication failed", ex);
                TechnicalError = ccpEx.GetLastError();

                hr = HRESULT.E_FAIL;
            }
            finally
            {
            }
            return hr;
        }

        /// <summary>
        /// Creates a folder of the given path and name (overwriting an existing folder)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string CreateFolder(string path, string name)
        {
            Dictionary<string, object> requiredParameters = new Dictionary<string, object>();
            requiredParameters.Add("path", path);
            requiredParameters.Add("name", name);

            String url = BuildUrl("folder", "create", requiredParameters);

            JObject jsonObj = InvokeShareFileOperation(url);
            if (!(bool)jsonObj["error"])
            {
                string folderId = (string)jsonObj["value"];
                return folderId;
            }
            return string.Empty;
        }

        /// <summary>
        /// Determines if a folder with the given name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool DoesFolderExist(string name)
        {
            string id = GetItemId(name, ITEM_TYPE.FOLDER);
            return !string.IsNullOrEmpty(id);
        }


        /// <summary>
        /// Doeses the file exist.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool DoesFileExist(string filename, string path, String username )
        {
            string theRealname = GetItemId(@"/" + username + @"/", path + @"/" +filename, SharefileCCP.ITEM_TYPE.FILE);
            string id = GetItemId(theRealname, ITEM_TYPE.FILE);
            return !string.IsNullOrEmpty(id);
        }

        public bool DoesFolderExist(string path, string name)
        {
            string id = GetItemId(path, name, ITEM_TYPE.FOLDER);
            return !string.IsNullOrEmpty(id);
        }

        /// <summary>
        /// Searches for a file or folder and returns its id
        /// The folder if found will be selected thereby allowing files to be uploaded to it
        /// </summary>
        /// <param name="query">search query: partial name for file or folder </param>
        /// <param name="type">file or folder</param>
        /// <returns></returns>
        public string GetItemId(string query, ITEM_TYPE type)
        {
            string itemId = string.Empty;

            Dictionary<string, object> requiredParameters = new Dictionary<string, object>();
            requiredParameters.Add("query", query);
            requiredParameters.Add("showpartial", false); // no, dont show me partial matches, i want exactly what i asked for or nothing.

            String url = BuildUrl("search", "search", requiredParameters);
            
            JObject jsonObj = InvokeShareFileOperation(url);
            if (!(bool)jsonObj["error"])
            {
                JArray items = (JArray)jsonObj["value"];
                foreach (JObject item in items)
                {
                    if (IsValidType(item, type))
                    {
                        itemId = GetItemId(item);
                        break;
                    }
                    else
                    {
                        throw new Exception("IsValidType returned false. fix it.");
                    }
                }
            }
            return itemId;
        }

        public string GetItemId(string path, string name, ITEM_TYPE type)
        {
            string itemId = string.Empty;

            Dictionary<string,string> items = FolderListData(path,type);
            if (items.ContainsKey(name))
            {
                itemId = items[name];
            }
            return itemId;
        }

        /// <summary>
        /// Checks if item type is valid 
        /// </summary>
        /// <param name="item">JObject search results</param>
        /// <param name="type">folder or file</param>
        /// <returns></returns>
        private bool IsValidType(JObject item, ITEM_TYPE type)
        {
            bool isValidType = false;
            foreach (JProperty prop in item.Properties())
            {
                if (prop.Name == "type")
                {
                    if ((string)item["type"] == GetTypeAsString(type))
                    {
                        isValidType = true;
                        break;
                    }
                }
            }

            return isValidType;
        }

        /// <summary>
        /// Returns the JObject property value corresponding to the type enum
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetTypeAsString(ITEM_TYPE type)
        {
            string stringType = string.Empty;

            switch (type)
            {
                case ITEM_TYPE.FOLDER:
                    stringType = "folder";
                    break;
                case ITEM_TYPE.FILE:
                    stringType = "file";
                    break;
                default:
                    break;
            }

            return stringType;
        }

        /// <summary>
        /// Returns the value of the JObject's id property if it exists
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private string GetItemId(JObject item)
        {
            string itemId = string.Empty;

            foreach (JProperty prop in item.Properties())
            {
                if (prop.Name == "id")
                {
                    itemId = (string)item["id"];
                }
            }

            return itemId;
        }

        /// <summary>
        /// Prints out a folder list for the specified path or root if none is provided. Currently prints out id, filename, creationdate, type.
        /// </summary>
        /// <param name="path">folder to list</param>
        public Dictionary<string, string> FolderListData(string path, ITEM_TYPE type)
        {
            Dictionary<string, string> fileData = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(path))
            {
                path = "/";
            }

            Dictionary<string, object> requiredParameters = new Dictionary<string, object>();
            requiredParameters.Add("path", path);

            String url = BuildUrl("folder", "list", requiredParameters);

            JObject jsonObj = InvokeShareFileOperation(url);
            if (!(bool)jsonObj["error"])
            {
                JArray items = (JArray)jsonObj["value"];
                foreach (JObject item in items)
                {
                    Console.WriteLine(item["id"] + " " + item["filename"] + " " + item["creationdate"] + " " + item["type"]);
                    if(GetTypeAsString(type) == (string)item["type"])
                        fileData.Add((string)item["filename"], (string)item["id"]);
                }
            }
            else
            {
                Console.WriteLine(jsonObj["errorCode"] + " : " + jsonObj["errorMessage"]);
            }
            return fileData;
        }

        /// <summary>
        /// Prints out a folder list for the specified path or root if none is provided. Currently prints out id, filename, creationdate, type.
        /// </summary>
        /// <param name="path">folder to list</param>
        public List<string> FolderList(string path)
        {
            List<string> files = new List<string>();
            if (string.IsNullOrEmpty(path))
            {
                path = "/";
            }

            Dictionary<string, object> requiredParameters = new Dictionary<string, object>();
            requiredParameters.Add("path", path);

            String url = BuildUrl("folder", "list", requiredParameters);

            JObject jsonObj = InvokeShareFileOperation(url);
            if (!(bool)jsonObj["error"])
            {
                JArray items = (JArray)jsonObj["value"];
                foreach (JObject item in items)
                {
                    Console.WriteLine(item["id"] + " " + item["filename"] + " " + item["creationdate"] + " " + item["type"]);
                    files.Add((string)item["filename"]);
                }
            }
            else
            {
                Console.WriteLine(jsonObj["errorCode"] + " : " + jsonObj["errorMessage"]);
            }
            return files;
        }
         
        /// <summary>
        /// Uploads a file to ShareFile.
        /// </summary>
        /// <param name="localPath">full path to local file, i.e. c:\\path\\to\\local.file</param>
        /// <param name="optionalParameters">name/value optional parameters</param>
        public string FileUpload(string localPath, Dictionary<string, object> optionalParameters)
        {
            string retVal=string.Empty;

            //MessageBox.Show("Fileupload localpath: " + localPath);
            
            FileInfo file = new FileInfo(localPath);

            Dictionary<string, object> requiredParameters = new Dictionary<string, object>();
            requiredParameters.Add("filename", file.Name);

            string url = BuildUrl("file", "upload", requiredParameters, optionalParameters);

            JObject jsonObj = InvokeShareFileOperation(url);
            if (!(bool)jsonObj["error"])
            {
                string uploadUrl = (string)jsonObj["value"];
                Console.WriteLine(uploadUrl);
                retVal = UploadMultiPartFile(file, uploadUrl);
                Console.WriteLine(retVal);
            }
            else
            {                
                Console.WriteLine(jsonObj["errorCode"] + " : " + jsonObj["errorMessage"]);
                throw new Exception(jsonObj["errorCode"] + " : " + jsonObj["errorMessage"]);
            }
            return retVal;
        }

        public void DeleteFile(string fileId)
        {
           
            Dictionary<string, object> requiredParameters = new Dictionary<string, object>();
            requiredParameters.Add("id", fileId);

            String url = BuildUrl("file", "delete", requiredParameters);

            JObject jsonObj = InvokeShareFileOperation(url);
            if (!(bool)jsonObj["error"])
            {
                JToken result = (JToken)jsonObj["value"];

                // success
            }
            else
            {
                throw new Exception("Problem deleting file: " + jsonObj["errorCode"] + " : " + jsonObj["errorMessage"]);                
            }
            

        }

        /// <summary>
        /// Downloads a file from ShareFile.
        /// </summary>
        /// <param name="fileId">id of the file to download</param>
        /// <param name="localPath">complete path to download file to, i.e. c:\\path\\to\\local.file</param>
        public void FileDownload(string fileId, string localPath)
        {
            try
            {
                bool isSuccessful = true;
                string downloadUrl = GetDownLoadURL(fileId);
                Console.WriteLine("downloadUrl = " + downloadUrl);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                BufferedStream source = new BufferedStream(response.GetResponseStream());
                using (FileStream target = new FileStream(localPath, FileMode.Create))
                {
                    byte[] chunk = new byte[8192];
                    int len = 0;
                    while ((len = source.Read(chunk, 0, 8192)) > 0)
                    {
                        target.Write(chunk, 0, len);
                    }
                    Console.WriteLine("Download complete");
                }
                response.Close();
                
            }
            catch (Exception error)
            {
                
                throw new Exception("Problem downloading file: " + error.Message, error);
            }
        }

        public string GetDownLoadURL(string fileId)
        {
            
            Dictionary<string, object> requiredParameters = new Dictionary<string, object>();
            requiredParameters.Add("id", fileId);

            String url = BuildUrl("file", "download", requiredParameters);

            JObject jsonObj = InvokeShareFileOperation(url);
            if (!(bool)jsonObj["error"])            
                return (string)jsonObj["value"];            
            else
                throw new Exception("Unable to obtain download URL for file id:" + fileId + " Error code (" + jsonObj["errorCode"] + " : " + jsonObj["errorMessage"]+")");
            
        }

        public void UploadClipboard()
        {
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                //TextWriter tw = new 
            }
            if(Clipboard.ContainsImage())
            {
                using (BinaryWriter b = new BinaryWriter(File.Open(CLIPBOARD_BIN_FILE, FileMode.Create)))
                {
                    //b.Write(Clipboard.GetImage());
                }
            }
        }

        /// <summary>
        /// Sends a Send a File email.
        /// </summary>
        /// <param name="path">path to file in ShareFile to send</param>
        /// <param name="to">email address to send to</param>
        /// <param name="subject">subject of the email</param>
        /// <param name="optionalParameters">name/value optional parameters</param>
        public void FileSend(string path, string to, string subject, Dictionary<string, object>optionalParameters)
        {
            Dictionary<string, object> requiredParameters = new Dictionary<string, object>();
            requiredParameters.Add("path", path);
            requiredParameters.Add("to", to);
            requiredParameters.Add("subject", subject);

            String url = BuildUrl("file", "send", requiredParameters, optionalParameters);

            JObject jsonObj = InvokeShareFileOperation(url);
            if (!(bool)jsonObj["error"])
            {
                string value = (string)jsonObj["value"];
                Console.WriteLine(value);
            }
            else
            {
                Console.WriteLine(jsonObj["errorCode"] + " : " + jsonObj["errorMessage"]);
            }
        }

        /// <summary>
        /// Creates a client or employee user in ShareFile.
        /// </summary>
        /// <param name="firstName">first name</param>
        /// <param name="lastName">last name</param>
        /// <param name="email">email address</param>
        /// <param name="isEmployee">true to create an employee, false to create a client</param>
        /// <param name="optionalParameters">name/value optional parameters</param>
        public void usersCreate(string firstName, string lastName, string email, bool isEmployee, Dictionary<string, object> optionalParameters)
        {
            Dictionary<string, object> requiredParameters = new Dictionary<string, object>();
            requiredParameters.Add("firstname", firstName);
            requiredParameters.Add("lastname", lastName);
            requiredParameters.Add("email", email);
            requiredParameters.Add("isemployee", isEmployee);

            String url = BuildUrl("users", "create", requiredParameters, optionalParameters);

            JObject jsonObj = InvokeShareFileOperation(url);
            if (!(bool)jsonObj["error"])
            {
                JObject user = (JObject)jsonObj["value"];
                Console.WriteLine(user["id"] + " " + user["primaryemail"]);
            }
            else
            {
                Console.WriteLine(jsonObj["errorCode"] + " : " + jsonObj["errorMessage"]);
            }
        }

        /// <summary>
        /// Creates a distribution group in ShareFile.
        /// </summary>
        /// <param name="name">name of group</param>
        /// <param name="optionalParameters">name/value optional parameters</param>
        public void GroupCreate(string name, Dictionary<string, object> optionalParameters)
        {
            Dictionary<string, object> requiredParameters = new Dictionary<string, object>();
            requiredParameters.Add("name", name);

            String url = BuildUrl("group", "create", requiredParameters, optionalParameters);

            JObject jsonObj = InvokeShareFileOperation(url);
            if (!(bool)jsonObj["error"])
            {
                JObject group = (JObject)jsonObj["value"];
                Console.WriteLine(group["id"] + " " + group["name"]);
            }
            else
            {
                Console.WriteLine(jsonObj["errorCode"] + " : " + jsonObj["errorMessage"]);
            }
        }

        /// <summary>
        /// Searches for items in ShareFile.
        /// </summary>
        /// <param name="query">search term</param>
        /// <param name="optionalParameters">name/value optional parameters</param>
        public void Search(string query, Dictionary<string, object> optionalParameters)
        {
            Dictionary<string, object> requiredParameters = new Dictionary<string, object>();
            requiredParameters.Add("query", query);

            String url = BuildUrl("search", "search", requiredParameters, optionalParameters);

            JObject jsonObj = InvokeShareFileOperation(url);
            if (!(bool)jsonObj["error"])
            {
                JArray items = (JArray)jsonObj["value"];
                if (items.Count == 0)
                {
                    Console.WriteLine("No Results");
                    return;
                }

                string path = "";
                foreach (JObject item in items)
                {
                    path = "/";
                    if (((string)item["parentid"]).Equals("box"))
                    {
                        path = "/File Box";
                    }
                    else
                    {
                        path = (string)item["parentsemanticpath"];
                    }
                    Console.WriteLine(path + "/" + item["filename"] + " " + item["creationdate"] + " " + item["type"]);
                }
            }
            else
            {
                Console.WriteLine(jsonObj["errorCode"] + " : " + jsonObj["errorMessage"]);
            }           
        }


        // ------------------------------------------- Utility Functions ------------------------------------------- 
        private string BuildUrl(string endpoint, string op, Dictionary<string, object> requiredParameters)
        {
            return (BuildUrl(endpoint, op, requiredParameters, new Dictionary<string,object>()));
        }

        private string BuildUrl(string endpoint, string op, Dictionary<string, object> requiredParameters, Dictionary<string, object>optionalParameters)
        {
            
            requiredParameters.Add("authid", this.authId);
            requiredParameters.Add("op", op);
            requiredParameters.Add("fmt", "json");

            ArrayList parameters = new ArrayList();
            foreach (KeyValuePair<string, object> kv in requiredParameters)
            {
                parameters.Add(string.Format("{0}={1}", HttpUtility.UrlEncode(kv.Key), HttpUtility.UrlEncode(kv.Value.ToString())));
            }
            foreach (KeyValuePair<string, object> kv in optionalParameters)
            {
                parameters.Add(string.Format("{0}={1}", HttpUtility.UrlEncode(kv.Key), HttpUtility.UrlEncode(kv.Value.ToString())));
            }

            String url = string.Format("https://{0}.{1}/rest/{2}.aspx?{3}", this.subdomain, this.tld, endpoint, String.Join("&", parameters.ToArray()));
            Console.WriteLine(url);

            return (url);
        }

        private JObject InvokeShareFileOperation(string requestUrl)
        {            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            String json = reader.ReadToEnd();
            response.Close();
            return JObject.Parse(json);
        }

        private string UploadMultiPartFile(FileInfo fileInfo, string fileUploadUrl)
        {
            string contentType;

            byte[] uploadData = CreateMultipartFormUpload(fileInfo, fileInfo.Name, out contentType);

            string rv = Send(fileUploadUrl, contentType, uploadData);
            return (rv);
        }

        private Byte[] CreateMultipartFormUpload(FileInfo fileInfo, string remoteFilename, out string contentType)
        {
            string boundaryGuid = "upload-" + Guid.NewGuid().ToString("n");
            contentType = "multipart/form-data; boundary=" + boundaryGuid;

            MemoryStream ms = new MemoryStream();
            byte[] boundaryBytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundaryGuid + "\r\n");

            // Write MIME header
            ms.Write(boundaryBytes, 2, boundaryBytes.Length - 2);
            string header = String.Format(@"Content-Disposition: form-data; name=""{0}""; filename=""{1}""" +
                "\r\nContent-Type: application/octet-stream\r\n\r\n", "File1", remoteFilename);
            byte[] headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
            ms.Write(headerBytes, 0, headerBytes.Length);

            // Load the file into the byte array
            using (FileStream file = fileInfo.OpenRead())
            {
                byte[] buffer = new byte[1024 * 1024];
                int bytesRead;

                while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }
            }

            // Write MIME footer
            boundaryBytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundaryGuid + "--\r\n");
            ms.Write(boundaryBytes, 0, boundaryBytes.Length);

            byte[] retVal = ms.ToArray();
            ms.Close();

            return retVal;
        }

        private string Send(string url, string contenttype, byte[] postBytes)
        {
            String retVal;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 1000 * 60; // 60 seconds
                request.Method = "POST";
                request.ContentType = contenttype;
                request.ContentLength = postBytes.Length;
                request.Credentials = CredentialCache.DefaultCredentials;

                using (Stream postStream = request.GetRequestStream())
                {
                    int chunkSize = 48 * 1024;
                    int remaining = postBytes.Length;
                    int offset = 0;

                    do
                    {
                        if (chunkSize > remaining) { chunkSize = remaining; }
                        postStream.Write(postBytes, offset, chunkSize);

                        remaining -= chunkSize;
                        offset += chunkSize;

                    } while (remaining > 0);

                    postStream.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    StreamReader rdr = new StreamReader(response.GetResponseStream());
                    retVal = rdr.ReadToEnd();
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                retVal = "Upload was not successfull. Please try again later";
            }
            return retVal;
        }

        public static void Main(string[] args)
        {
            SharefileCCP sample = new SharefileCCP();
            Dictionary<string, object> optionalParameters = new Dictionary<string, object>();

            SharefileCCP.HRESULT loginStatus = sample.Authenticate("mysubdomain", "sharefile.com", "my@email.address", "mypassword");
            if (loginStatus == HRESULT.S_OK)
            {
                sample.FolderList("/MyFolder");
            }
        }

    }


}