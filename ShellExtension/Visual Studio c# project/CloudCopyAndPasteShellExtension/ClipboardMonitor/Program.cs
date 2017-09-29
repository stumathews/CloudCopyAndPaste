using FunctionalityLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardMonitor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                FunctionalityLibrary.CommonFunctionality.Instance.Initialise(); // Initialize our global singleton with all out settings and configurations
                new Cloud().FetchAwaitingClipboardFiles();

                MessageOnlyWindow mof = new MessageOnlyWindow();
                Application.Run();
            }
            catch (Exception error)
            {
                CommonFunctionality.Instance.logger.LogItAsError("Unexpected error starting up clipboard monitor: " + error.Message);
            }

            /*
             * If you're wondering what the hell is going on here, let me explain:
             * We're creating a Message-only window but to do this we need to first have crafted a MessageOnlyWindow form, instantiate it...
             * and it automatically starts handling WM_ messages via its message pump. Application.Run() basically just keeps the app alive and servicing 
             * our custom message pump in the MessageOnlyWindow
             */

        }
    }
}
