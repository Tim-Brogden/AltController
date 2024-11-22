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
using System.IO;
using AltController.Actions;
using AltController.Config;
using AltController.Core;
using AltController.Input;

namespace AltController.UserControls
{
    /// <summary>
    /// Control for editing Show / hide custom window actions
    /// </summary>
    public partial class CustomWindowStateControl : UserControl
    {
        // Fields
        private NamedItemList _windowListItems = new NamedItemList();
        private CustomWindowStateAction _currentAction = new CustomWindowStateAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomWindowStateControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Control loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowNameCombo.ItemsSource = _windowListItems;

            RefreshDisplay();
        }

        /// <summary>
        /// Populate the list of custom windows
        /// </summary>
        /// <param name="sources"></param>
        public void SetInputSources(NamedItemList sources)
        {
            _windowListItems.Clear();
            _windowListItems.Add(new NamedItem(Constants.NoneID, Properties.Resources.String_None_Option));
            _windowListItems.Add(new NamedItem(Constants.DefaultID, Properties.Resources.String_All_Option));

            foreach (BaseSource source in sources)
            {
                if (source is CustomWindowSource)
                {
                    string title = (source as CustomWindowSource).WindowTitle;
                    _windowListItems.Add(new NamedItem(source.ID, title));
                }
            }
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is CustomWindowStateAction)
            {
                _currentAction = (CustomWindowStateAction)action;
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
                if (_windowListItems.GetItemByID(_currentAction.WindowID) != null)
                {
                    this.WindowNameCombo.SelectedValue = _currentAction.WindowID;
                }
                else
                {
                    this.WindowNameCombo.SelectedValue = 0;
                }

                switch (_currentAction.WindowState)
                {
                    case EWindowState.Minimise:
                        HideButton.IsChecked = true;
                        break;
                    case EWindowState.Normal:
                        ShowButton.IsChecked = true;
                        break;
                    default:
                        ShowOrHideButton.IsChecked = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem profileNameItem = (NamedItem)this.WindowNameCombo.SelectedItem;
            if (profileNameItem != null)
            {
                _currentAction = new CustomWindowStateAction();
                _currentAction.WindowID = profileNameItem.ID;
                _currentAction.WindowTitle = profileNameItem.Name;

                if (ShowButton.IsChecked == true)
                {
                    _currentAction.WindowState = EWindowState.Normal;
                }
                else if (HideButton.IsChecked == true)
                {
                    _currentAction.WindowState = EWindowState.Minimise;
                }
                else
                {
                    _currentAction.WindowState = EWindowState.Minimise | EWindowState.Normal;
                }
            }

            return _currentAction;
        }
    }
}
