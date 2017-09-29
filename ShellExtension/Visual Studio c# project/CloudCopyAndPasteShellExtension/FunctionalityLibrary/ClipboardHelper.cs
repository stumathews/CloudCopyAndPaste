using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FunctionalityLibrary
{
    class ClipboardHelper
    {
        // Demonstrates SetData, ContainsData, and GetData. 
        public static Object SwapClipboardFormattedData(String format, Object data)
        {
            Object returnObject = null;
            if (Clipboard.ContainsData(format))
            {
                returnObject = Clipboard.GetData(format);                
            }

            Clipboard.SetData(format, data);
            return returnObject;
        }

        public static bool ClipboardContainsType(string format)
        {
            return Clipboard.GetDataObject().GetDataPresent(format);
        }


        internal static string GetClipboardText()
        {

            if (ClipboardHelper.ClipboardContainsType(DataFormats.Text))
                return (String)Clipboard.GetDataObject().GetData(DataFormats.Text);
            return String.Empty;

            //if (ClipboardHelper.ClipboardContainsType(DataFormats.StringFormat))
            //{
            //    String text = (String)Clipboard.GetDataObject().GetData(DataFormats.StringFormat);
            //}

            //if (ClipboardHelper.ClipboardContainsType(DataFormats.UnicodeText))
            //{
            //    String text = (String)Clipboard.GetDataObject().GetData(DataFormats.Text);
            //}

            //if (ClipboardHelper.ClipboardContainsType(DataFormats.OemText))
            //{
            //    String text = (String)Clipboard.GetDataObject().GetData(DataFormats.Text);
            //}
        }
    }
}
