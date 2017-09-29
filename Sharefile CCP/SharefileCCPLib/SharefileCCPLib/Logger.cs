using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharefileCCPLib
{
    /// <summary>
    /// A central place for logging functionality
    /// </summary>
    public class Logger
    {
        private static readonly string     _logName      = "CCP.log";
        private static readonly string     _verboseLog = "CCPVerboseLog.log";
        private static readonly string     _verboseRequired      = "ActivateCCPVerboseLog.txt";
        private static readonly string     _eventlog_name      = "CCP" ;
		private static bool _outputToConsole = false;
        private static System.Diagnostics.EventLog          _event_log              = null ;
        private static System.Diagnostics.EventLogEntryType _max_event_log_type     = System.Diagnostics.EventLogEntryType.Error ;
        private static bool _logging_autodisabled  = false;
		private static object lockObject = new Object();
		private static object logLock = new Object();

		/// <summary>
		/// When true, output the logging to the console
		/// </summary>
		public static bool OutputToConsole 
		{ 
			set
			{
				lock (lockObject)
				{
					_outputToConsole = value;
				}
			}
		}

        /// <summary>
        /// Initialises the Logger prior to use
        /// </summary>
        public static void Initialise( string source_name )
        {
            try
            {
                if (_event_log == null)
                {
                    lock (typeof(Logger))
                    {
                        if (_event_log == null)
                        {
                            CreateLog(source_name);

                            _event_log = new System.Diagnostics.EventLog(_eventlog_name, ".", source_name);

                            try
                            {
                                string max_event_log_type = System.Web.Configuration.WebConfigurationManager.AppSettings["MaxEventLogType"];

                                if (!string.IsNullOrEmpty(max_event_log_type))
                                    _max_event_log_type = (System.Diagnostics.EventLogEntryType)System.Enum.Parse(typeof(System.Diagnostics.EventLogEntryType), max_event_log_type);
                            }
                            catch
                            { }
                        }
                    }
                }

                System.Diagnostics.StackFrame lFrame = new System.Diagnostics.StackTrace(1).GetFrame(0);

                MandatoryLog(System.Diagnostics.EventLogEntryType.Information, string.Format("Build version [{0}]", lFrame.GetMethod().DeclaringType.AssemblyQualifiedName));
#if DEBUG
            MandatoryLog( System.Diagnostics.EventLogEntryType.Information, "Debug Build" );
#else
                MandatoryLog(System.Diagnostics.EventLogEntryType.Information, "Release Build");
#endif
                MandatoryLog(System.Diagnostics.EventLogEntryType.Information, string.Format("Running on OS Version [{0}]", System.Environment.OSVersion));
                MandatoryLog(System.Diagnostics.EventLogEntryType.Information, string.Format("Running on .Net Version [{0}]", System.Environment.Version));

                System.Net.IPHostEntry my_ips = System.Net.Dns.GetHostEntry(System.Environment.MachineName);

                MandatoryLog(System.Diagnostics.EventLogEntryType.Information, string.Format("Running on machine [{0}] [{1}]", System.Environment.MachineName, (my_ips != null && my_ips.AddressList.Length > 0) ? my_ips.AddressList[0].ToString() : "no-ip?"));
                MandatoryLog(System.Diagnostics.EventLogEntryType.Information, string.Format("Running as User [{0}]", System.Security.Principal.WindowsIdentity.GetCurrent().Name));
            }
            catch (Exception)
            {
                //Can't do anything with logging failures.
            }
        }

        /// <summary>
        /// CreateLog
        /// </summary>
        /// <param name="source_name"></param>
        public static void  CreateLog( string source_name )
        {
            if ( !System.Diagnostics.EventLog.SourceExists( source_name ) )
            {
                int i = 10 ;

                while( --i > 0 && !System.Diagnostics.EventLog.SourceExists( source_name ) )
                {
                    System.Threading.Thread.Sleep( 100 ) ;
                }
            }
        }

        /// <summary>
        /// Appends text to a client log file.
        /// </summary>
        /// <param name="strText">Text to log to the file</param>
        public static void AppendToClientLogFile(string strText)
        {
            try
            {
				if (_outputToConsole)
				{
					Console.WriteLine(strText);
					return;
				}

                string strlogfile = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"), _logName);
                using (System.IO.StreamWriter sw = System.IO.File.AppendText(strlogfile))
                {
                    sw.WriteLine(System.DateTime.Now.ToString() + ": " + strText);
                    sw.Close();
                }
            }
            catch (System.Exception)
            {
                /* If we couldn't write to the log file then don't fret - it's not the end of the world...*/
            }
        }

        /// <summary>
        /// Appends the specified string and relevant new line characters to the log file
        /// </summary>
        /// <param name="strText">The text to append</param>
		/// <param name="traceLevel">if its info, prevents it coming out on the console for automation</param>
        public static void AppendToLogFile(string strText,System.Diagnostics.TraceLevel traceLevel = System.Diagnostics.TraceLevel.Info)
        {
            try
            {
				if (_outputToConsole)
				{
					if (traceLevel <= System.Diagnostics.TraceLevel.Info)
					{
						lock (logLock)
						{
							Console.WriteLine(strText);
						}
					}
				}

                if (System.IO.File.Exists(System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"), _verboseRequired)) == true)
                {
                    string strlogfile = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("TEMP"), _verboseLog);
					using (System.IO.StreamWriter sw = System.IO.File.AppendText(strlogfile))
					{
						lock (logLock)
						{
							sw.WriteLine(System.DateTime.Now.ToString() + ": " + strText);
						}
						sw.Close();
					}
                }
            }
            catch (System.Exception)
            {
                /* If we couldn't write to the log file then don't fret - it's not the end of the world...*/
                
                /* But we should be able to log it to the system event log so we dont lose reference of it*/
                Logger.Warning("{0}", new object[] { strText });
            }
        }

        /// <summary>
        /// logs an info msg that a method body has started
        /// </summary>
        public static void In()
        {
            //Debug( "Entering method {0}", GetMethodOnStack(2) ) ;

            AppendToLogFile ("Entering method " + GetMethodOnStack(2));
        }

        /// <summary>
        /// logs an info msg that a method body has ended
        /// </summary>
        public static void Out()
        {
            //Debug( "Leaving method {0}", GetMethodOnStack(2) ) ;
            AppendToLogFile("Leaving method " +  GetMethodOnStack(2));
        }

        /// <summary>
        /// Writes the specified text and relevant new line characters to the log file.
        /// Prepends the calling function's name
        /// </summary>
        /// <param name="logline">The text to write</param>
        public static void WriteLine(string logline)
        {
            AppendToLogFile("Method: " + GetMethodOnStack (2) + " - " + logline);
        }

        /// <summary>
        /// Log a message with a timestamp to millisecond accuracy associated
        /// </summary>
        /// <param name="what"></param>
        public static void Timestamp( string what )
        {
            Debug( "Timestamp [{0}] for [{1}] ", System.DateTime.Now.ToUniversalTime().ToString("u"), what ) ;
        }

        /// <summary>
        /// logs a debug msg
        /// </summary>
        /// <remarks>Things that we can't recover from, or are unanticipated.</remarks>
        public static void Debug( string format, params object[] args )
        {
            OptionalLog( System.Diagnostics.EventLogEntryType.Information, format, args );
        }

        /// <summary>
        /// logs an info msg
        /// </summary>
        public static void Info( string format, params object[] args )
        {
            OptionalLog( System.Diagnostics.EventLogEntryType.Information, format, args );
        }

        /// <summary>
        /// logs an info msg regardless of the max log level selected.
        /// </summary>
        public static void MandatoryInfo(string format, params object[] args)
        {
            MandatoryLog(System.Diagnostics.EventLogEntryType.Information, format, args);
        }

        /// <summary>
        /// logs a warning msg
        /// </summary>
        public static void Warning( string format, params object[] args )
        {
            OptionalLog( System.Diagnostics.EventLogEntryType.Warning, format, args );
        }

        /// <summary>
        /// logs an error msg
        /// </summary>
        /// <remarks>
        /// Things that we can anticipate, but stop proper operation and which the user needs to know about.
        /// E.g. unlicensed app, error on configuration.
        /// </remarks>
        public static void Error( string format, params object[] args )
        {
            OptionalLog( System.Diagnostics.EventLogEntryType.Error, format, args );
        }

        /// <summary>
        /// logs a fatal msg
        /// </summary>
        /// <remarks>Things that we can't recover from, or are unanticipated.</remarks>
        public static void Fatal( string format, params object[] args )
        {
            OptionalLog( System.Diagnostics.EventLogEntryType.Error, format, args );
        }

        /// <summary>
        /// Helper method to output an exception to the Microsoft logger
        /// </summary>
        /// <param name="aMessage">A brief description of what was being done</param>
        /// <param name="aException">The exception that occurred</param>
        public static void Exception( string aMessage, System.Exception aException )
        {
            Exception(aMessage, aException, 1);
        }

        /// <summary>
        /// Helper method to output an exception to the Microsoft logger
        /// </summary>
        /// <param name="aMessage">A brief description of what was being done</param>
        /// <param name="aException">The exception that occurred</param>
        /// <param name="depth">Number of stack frames to discard</param>
        public static void Exception(string aMessage, System.Exception aException, int depth)
        {
            OptionalLog(System.Diagnostics.EventLogEntryType.Error, "While [{0}], exception [{1}] occurred. {2}", new object[] { aMessage, aException.Message, GetMethodOnStack(depth + 2) });
            OptionalLog(System.Diagnostics.EventLogEntryType.Error, "Stack [{0}]", new object[] { aException.StackTrace });
        }

        /// <summary>
        /// This causes a message to be submitted to the Log. The message may or may not appear somewhere depending
        /// on the logging settings
        /// </summary>
        /// <param name="level">The severity of the message</param>
        /// <param name="format">A format string to format the message</param>
        /// <param name="args">parameters that might be used in the format string (optional) </param>
        private static void OptionalLog( System.Diagnostics.EventLogEntryType level, string format, params object[] args )
        {
            if ( level <= _max_event_log_type )
            {
                MandatoryLog( level, format, args ) ;
            }
        }

        /// <summary>
        /// This causes a message to be submitted to the Log. The message may or may not appear somewhere depending
        /// on the logging settings
        /// </summary>
        /// <param name="level">The severity of the message</param>
        /// <param name="format">A format string to format the message</param>
        /// <param name="args">parameters that might be used in the format string (optional) </param>
        private static void MandatoryLog( System.Diagnostics.EventLogEntryType level, string format, params object[] args )
        {
            if (_logging_autodisabled == false)
            {
                try
                {
                    string msg = string.Format(format, args);
                    if (_event_log != null)
                    {
                        _event_log.WriteEntry(msg, level);
                    }
                }
                catch
                {
                    // If we got an exception when trying to write an entry to the event log then ensure we don't keep trying again.
                    _logging_autodisabled = true;
                } //Don't throw an exception when logging.
            }
        }

        /// <summary>
        /// Gets details of the call in which the logging is being done.
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        private static string GetMethodOnStack(int depth)
        {
            string result = string.Empty;
            try
            {
                System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame(depth, true);
                if (sf == null)
                {
                    return string.Empty;
                }
                string declaringType    = sf.GetMethod().DeclaringType.Name == null ? string.Empty : sf.GetMethod().DeclaringType.Name;
                string methodName       = sf.GetMethod().Name == null ? string.Empty : sf.GetMethod().Name;
                string fileName         = sf.GetFileName() == null ? string.Empty : sf.GetFileName();
                string lineNumber       = sf.GetFileLineNumber().ToString();

                result = string.Format("{0}::{1} in [{2}] line:{3}", declaringType, methodName, fileName, lineNumber);
            }
            catch (System.Exception /*se*/)
            {
            }
            return result;
        }

    }
}


