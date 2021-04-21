/*
Alt Controller
--------------
Copyright 2013 Tim Brogden
http://altcontroller.net

Description
-----------
A free program for mapping computer inputs, such as pointer movements and button presses, 
to actions, such as key presses. The aim of this program is to help make third-party programs,
such as computer games, more accessible to users with physical difficulties.

License
-------
This file is part of Alt Controller. 
Alt Controller is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Alt Controller is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Alt Controller.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AltController.Core;
using AltController.Config;

namespace AltController.UserControls
{
    /// <summary>
    /// Interaction logic for ErrorMessageControl.xaml
    /// </summary>
    public partial class ErrorMessageControl : UserControl
    {
        private string _lastError = "";

        /// <summary>
        /// Constructor
        /// </summary>
        public ErrorMessageControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Show error message
        /// </summary>
        /// <param name="message"></param>
        public void Show(string caption, Exception ex)
        {
            // Store the error message so the user can click for more details
            StringBuilder sb = new StringBuilder();
            sb.Append(ex.Message);
            if (ex.StackTrace != null)
            {
                sb.AppendLine();
                sb.AppendLine("Stack trace: ");
                sb.Append(ex.StackTrace);
            }
            int i = 0;
            while (ex.InnerException != null && ++i<100)
            {
                ex = ex.InnerException;
                sb.AppendLine();
                sb.Append("Extra info: ");
                sb.Append(ex.Message);
            }
            _lastError = sb.ToString();

            // Log to file
            string messageLogPath = Path.Combine(AppConfig.UserDataDir, Constants.MessageLogFileName);
            string logMessage = string.Format("{0}{1}{2}{1}",
                DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture), Environment.NewLine, _lastError);
            File.AppendAllText(messageLogPath, logMessage);

            this.ErrorButton.Content = caption + " - click for details";
            this.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Clear current error message
        /// </summary>
        public void Clear()
        {
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        /// <summary>
        /// Show error details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ErrorButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(_lastError, "Error details", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
