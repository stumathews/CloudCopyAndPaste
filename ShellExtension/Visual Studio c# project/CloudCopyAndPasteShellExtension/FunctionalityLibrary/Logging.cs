using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalityLibrary
{
    /// <summary>
    /// Logging functionality 
    /// </summary>
    public class Logger
    {        
        /// <summary>
        /// Rudimentary log for shell extensions everyday logging needs.
        /// Creates the log file in default log file location if it doesnt exist
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogIt(String message)
        {
            message = "INFO: " + message;
            try
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(CommonFunctionality.Instance.common_path, CommonFunctionality.LOG_FILE_NAME), true))
                {
                    sw.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " " + message);
                    sw.Flush();                   
                }
            }
            catch (Exception/*Cant do anything about logging failing to log!*/){}

        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="message">The message.</param>
        public void LogItAsError(String message)
        {
            message = "Error: " + message;
            LogIt(message);
        }
    }
}
