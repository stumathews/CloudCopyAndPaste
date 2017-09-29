using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FunctionalityLibrary;
using Nini.Config;

namespace ClipboardMonitor
{

    /// <summary>
    /// The purpose of this Form, other than it being a form that only processes messages via its message pump, is
    /// to listen to changes to the clipboard and send them to sharefile.
    /// </summary>
    class MessageOnlyWindow : Form
    {

        private IntPtr _ClipboardViewerNext;
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll")]
        static extern bool ChangeClipboardChain (IntPtr hWndRemove, IntPtr hWndNewNext);



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
        const int WM_CLOSE = 0x0010; // if we had a window, shown and visible, which we dont - this is sent when user presses X on window
        const int WM_DESTROY = 0x0002; // when the app is teriminated.
        const string POLL_TIMER = "poll_timer";
        FunctionalityLibrary.Cloud cloud = new Cloud();
        private bool ChangedClipboardChain = false;
        private Timer poll_timer = new Timer();
        
        public MessageOnlyWindow()
        {
            var accessHandle = this.Handle;
                        
                // Start a timer that polls for new clipboard files and downloads them to improve clipboard paste times
                
                poll_timer.Tick += poll_timer_Tick;
                int interval = 1; //minute
                poll_timer.Interval = ((interval * 1000) * 60); // internal is in minutes, so convert to milliseconds ie.2 minutes = 120 000ms
                poll_timer.Start();
            
        }

        void poll_timer_Tick(object sender, EventArgs e)
        {

            try
            {
                CommonFunctionality.Instance.logger.LogIt("Automatic performing poll.");
                cloud.FetchAwaitingClipboardFiles();
            }
            catch (System.Net.WebException web)
            {
                CommonFunctionality.Instance.logger.LogIt("Unable to fetch awaiting clipboard files: " + web.Message);
            }  
            catch (Exception)
            {
                // last attempt is to reauthenticate
                CommonFunctionality.Instance.Authenticate(true);
                cloud.FetchAwaitingClipboardFiles();
            }

        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ChangeToMessageOnlyWindow();
            Visible = false;
            _ClipboardViewerNext = SetClipboardViewer(this.Handle);
        }

        /// <summary>
        /// Changes the automatic message only window.
        /// </summary>
        private void ChangeToMessageOnlyWindow()
        {
            IntPtr HWND_MESSAGE = new IntPtr(-3);
            SetParent(this.Handle, HWND_MESSAGE);
        }

        /// <summary>
        /// </summary>
        /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message" /> to process.</param>
        protected override void WndProc(ref Message m)
        {
            try
            {
                switch (m.Msg)
                {
                    case WM_ASKCBFORMATNAME:
                    case WM_CHANGECBCHAIN:
                        m = HandleChangeClipboardChainEvent(m); // Be a standup citizen of the clipboard
                        break;
                    case WM_DRAWCLIPBOARD:
                        cloud.CloudClipboardCopy(); // Clipboard has changed, serialise it and send it up to the cloud
                        break;
                    case WM_DESTROY:
                    case WM_CLOSE:
                        cleanup();
                        break;
                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
            catch (Exception)
            {
                // last ditch attempt it to re-authenticate
                CommonFunctionality.Instance.Authenticate(true);
                WndProc(ref m);
            }

        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        private void cleanup()
        {
            if (!ChangedClipboardChain)
            {
                ChangeClipboardChain(this.Handle, _ClipboardViewerNext);
                ChangedClipboardChain = true;
            }
            poll_timer.Stop();
            
        }

        /// <summary>
        /// We need to be a first class citizen and re-send notification that a window is deregistering itself from the clipboard monitoring chain
        /// </summary>
        /// <param name="m">The command.</param>
        /// <returns></returns>
        private Message HandleChangeClipboardChainEvent(Message m)
        {
            /* We need to be a first class citizen and re-send notification
             * that a window is deregistering itself from the clipboard monitoring chain.*/
            if (m.WParam == _ClipboardViewerNext)
            {
                _ClipboardViewerNext = m.LParam;
            }
            else
            {
                SendMessage(_ClipboardViewerNext, m.Msg, m.WParam, m.LParam);
            }
            return m;
        }
    }
}


//case WM_CLIPBOARDUPDATE:
//case WM_DESTROYCLIPBOARD:
//case WM_HSCROLLCLIPBOARD:
//case WM_PAINTCLIPBOARD:
//case WM_RENDERALLFORMATS:
//case WM_SIZECLIPBOARD:
//case WM_VSCROLLCLIPBOARD:
