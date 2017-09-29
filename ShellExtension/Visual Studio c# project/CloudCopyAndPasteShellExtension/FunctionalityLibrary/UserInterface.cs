using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FunctionalityLibrary
{
    /// <summary>
    /// Seperates user interface into its own component.
    /// </summary>
    public class UserInterface
    {
                               
        /// <summary>
        /// Shows the user message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="technical">The technical.</param>
        public void ShowUserMessage(string message, CommonFunctionality.SEVERITY severity, string technical = "")
        {
            String theMessage = String.Format("{0}\n{1}", message, technical);

            switch (severity)
            {
                case CommonFunctionality.SEVERITY.Low:
                    MessageBox.Show(theMessage, "Cloud copy message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case CommonFunctionality.SEVERITY.High:
                case CommonFunctionality.SEVERITY.Medium:
                   FunctionalityLibrary.UserInterfaces.ErrorForm.Show(theMessage, technical);
                    break;
                default:
                    throw new Exception("Unknown severity specified.");
            }

            CommonFunctionality.Instance.logger.LogIt(theMessage);

        }
    }
}
