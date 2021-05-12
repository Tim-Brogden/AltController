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
using System.Windows;
using System.Windows.Input;
using AltController.Core;

namespace AltController
{
    /// <summary>
    /// About window
    /// </summary>
    public partial class HelpAboutWindow : Window
    {
        public HelpAboutWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Window loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.VersionText.Text = string.Format("{0} {1}", Properties.Resources.String_Version, Constants.AppVersion);
            this.CopyrightText.Text = string.Format("{0} 2013-{1} {2}", Properties.Resources.String_Copyright, DateTime.Now.Year, Constants.AuthorName);
            this.TranslatorNamesText.Text = Constants.TranslatorNames;
        }

        /// <summary>
        /// Can Close command execute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        /// <summary>
        /// Close window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
            e.Handled = true;
        }

        /// <summary>
        /// Hyperlink clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            }
            catch (Exception)
            {
            }
            e.Handled = true;
        }

    }
}
