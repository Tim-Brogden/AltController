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
using AltController.Core;
using AltController.Actions;

namespace AltController.UserControls
{
    public partial class ChangeModeControl : UserControl
    {
        // Members
        private NamedItemList _changeModeListItems = new NamedItemList();
        private ChangeModeAction _currentAction = new ChangeModeAction();

        /// <summary>
        ///  Constructor
        /// </summary>
        public ChangeModeControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Control loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentAction != null)
            {
                this.ChangeModeCombo.SelectedValue = _currentAction.ModeID;
            }
        }

        /// <summary>
        /// Set the list of modes for the combo box
        /// </summary>
        /// <param name="modesList"></param>
        public void SetModesList(NamedItemList modesList)
        {
            GUIUtils.PopulateDisplayableListWithNamedItems(_changeModeListItems, modesList, true);

            this.ChangeModeCombo.ItemsSource = _changeModeListItems;
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is ChangeModeAction)
            {
                _currentAction = (ChangeModeAction)action;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;
            
            NamedItem item = (NamedItem)this.ChangeModeCombo.SelectedItem;
            if (item != null)
            {
                _currentAction = new ChangeModeAction();
                _currentAction.ModeID = item.ID;
                _currentAction.ModeName = item.Name;
            }

            return _currentAction;
        }

    }
}
