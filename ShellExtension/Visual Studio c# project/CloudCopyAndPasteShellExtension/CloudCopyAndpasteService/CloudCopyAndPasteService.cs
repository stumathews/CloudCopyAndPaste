using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;
using FunctionalityLibrary;

namespace CloudCopyAndpasteService
{
    public partial class CloudCopyAndPasteService : ServiceBase
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);
        public delegate int ServiceControlHandlerEx(int control, int eventType, IntPtr eventData, IntPtr context);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr RegisterServiceCtrlHandlerEx(string lpServiceName, ServiceControlHandlerEx cbex, IntPtr context);
        const int WM_DRAWCLIPBOARD = 0x0308;
        const int WM_ASKCBFORMATNAME = 0x030C;
        const int WM_CHANGECBCHAIN = 0x030D;
        const int WM_CLIPBOARDUPDATE = 0x031D;
        const int WM_DESTROYCLIPBOARD = 0x0307;
        const int WM_HSCROLLCLIPBOARD = 0x030E;
        const int WM_PAINTCLIPBOARD = 0x0309;
        const int WM_RENDERALLFORMATS = 0x0306;
        const int WM_RENDERFORMAT = 0x0305;
        const int WM_SIZECLIPBOARD = 0x030B;
        const int WM_VSCROLLCLIPBOARD = 0x030A;

        public const int SERVICE_CONTROL_STOP = 1;        
        public const int SERVICE_CONTROL_SHUTDOWN = 5;
        private ServiceControlHandlerEx myCallback;
        private IntPtr _ClipboardViewerNext;


        /// <summary>
        /// Our custom Service control handler.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="eventData">The event data.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private int ServiceControlHandler(int control, int eventType, IntPtr eventData, IntPtr context)
        {            
            if (control == SERVICE_CONTROL_STOP || control == SERVICE_CONTROL_SHUTDOWN)
            {
                CommonFunctionality.Instance.logger.LogIt("Shutting down or stopping cloud copy and paste service...");
                base.Stop();
            }
            else
            {
                switch( control )
                {
                    case WM_ASKCBFORMATNAME:
                    case WM_CHANGECBCHAIN:
                    case WM_CLIPBOARDUPDATE:
                    case WM_DESTROYCLIPBOARD:
                    case WM_DRAWCLIPBOARD:
                    case WM_HSCROLLCLIPBOARD:
                    case WM_PAINTCLIPBOARD:
                    case WM_RENDERALLFORMATS:
                    case WM_SIZECLIPBOARD:
                    case WM_VSCROLLCLIPBOARD:
                        CommonFunctionality.Instance.logger.LogIt("Got clipboard change notification!");
                        break;
                    default:
                        CommonFunctionality.Instance.logger.LogIt("Got other wnd message: " + control.ToString());
                        break;
                }
                
            }
           

            return 0;
        }

        public CloudCopyAndPasteService()
        {
            InitializeComponent();
            CommonFunctionality.Instance.Initialise();
        }

        protected override void OnStart(string[] args)
        {
            CommonFunctionality.Instance.logger.LogIt("Service starting...");
            IntPtr handle = RegisterCustomServiceControlHandler();
            if (!handle.Equals(ServiceHandle) || handle == IntPtr.Zero)
                CommonFunctionality.Instance.logger.LogIt("There was a problem registering custom service controller.");

             _ClipboardViewerNext = SetClipboardViewer(this.ServiceHandle);
             if (_ClipboardViewerNext == IntPtr.Zero)
                 CommonFunctionality.Instance.logger.LogIt("There was a problem setting clipboard viewer.");
            

        }


        /// <summary>
        /// Registers the custom service control handler.
        /// </summary>
        /// <returns></returns>
        private IntPtr RegisterCustomServiceControlHandler()
        {
            CommonFunctionality.Instance.logger.LogIt("Registering custom service control handler...");
            myCallback = new ServiceControlHandlerEx(ServiceControlHandler);
            return RegisterServiceCtrlHandlerEx(this.ServiceName, myCallback, IntPtr.Zero);
        }
    }

   
}
