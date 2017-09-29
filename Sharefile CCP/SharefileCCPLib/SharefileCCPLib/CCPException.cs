using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharefileCCPLib
{
    public class CCPException : Exception
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CCPException()
        { }

        /// <summary>
        /// Constructor with message
        /// </summary>
        /// <param name="message"></param>
        public CCPException(string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor with message and inner exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner_exception"></param>
        public CCPException(string message, Exception inner_exception)
            : base(message, inner_exception)
        { }

        /// <summary>
        /// Gets inner exception of last chained ASMException
        /// </summary>
        public Exception MostInner
        {
            get
            {
                Exception asmException = this;

                while (asmException.InnerException is CCPException)
                    asmException = asmException.InnerException;

                return asmException == this ? InnerException : asmException.InnerException ?? asmException;
            }
        }

        public string GetLastError()
        {
            string inner = string.IsNullOrEmpty(this.InnerException.Message.ToString()) ? string.Empty : this.InnerException.Message.ToString();
            string error = string.Format("Exception: {0}\r\n{1}", this.Message, inner);
            Logger.AppendToClientLogFile(error);
            return error;
        }

    }
}
