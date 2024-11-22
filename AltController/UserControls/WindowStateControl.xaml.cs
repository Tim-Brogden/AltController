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
using System.Windows;
using System.Windows.Controls;
using AltController.Actions;
using AltController.Core;

namespace AltController.UserControls
{
    /// <summary>
    /// Control for editing a Window state action to maximise / restore / minimise the active window
    /// </summary>
    public partial class WindowStateControl : UserControl
    {
        // Fields
        private WindowStateAction _currentAction = new WindowStateAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public WindowStateControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshDisplay();
        }
        
        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is WindowStateAction)
            {
                _currentAction = (WindowStateAction)action;
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Redisplay the action
        /// </summary>
        private void RefreshDisplay()
        {
            if (IsLoaded && _currentAction != null)
            {
                switch (_currentAction.WindowState)
                {
                    case EWindowState.Minimise:
                        MinimiseButton.IsChecked = true; break;
                    case EWindowState.Maximise:
                        MaximiseButton.IsChecked = true; break;
                    case EWindowState.Maximise | EWindowState.Normal:
                        MaximiseOrRestoreButton.IsChecked = true; break;
                }
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            EWindowState windowState = EWindowState.None;
            if (MinimiseButton.IsChecked == true)
            {
                windowState = EWindowState.Minimise;
            }
            else if (MaximiseButton.IsChecked == true)
            {
                windowState = EWindowState.Maximise;
            }
            else if (MaximiseOrRestoreButton.IsChecked == true)
            {
                windowState = EWindowState.Maximise | EWindowState.Normal;
            }

            _currentAction = new WindowStateAction();
            _currentAction.WindowState = windowState;

            return _currentAction;
        }

    }
}
