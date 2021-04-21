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
using System.Windows.Controls;
using AltController.Core;
using AltController.Actions;

namespace AltController.UserControls
{
    public partial class ToggleMouseButtonActionControl : UserControl
    {
        // Members
        private NamedItemList _mouseButtonListItems = new NamedItemList();
        private ToggleMouseButtonAction _currentAction = new ToggleMouseButtonAction();
        private EActionType _actionType = EActionType.ToggleMouseButton;

        // Properties
        public EActionType ActionType { get { return _actionType; } set { _actionType = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ToggleMouseButtonActionControl()
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
            Utils utils = new Utils();
            foreach (EMouseButton mouseButton in Enum.GetValues(typeof(EMouseButton)))
            {
                if (mouseButton != EMouseButton.None)
                {
                    _mouseButtonListItems.Add(new NamedItem((long)mouseButton, utils.GetMouseButtonName(mouseButton)));
                }
            }
            this.MouseButtonCombo.ItemsSource = _mouseButtonListItems;

            if (_currentAction != null)
            {
                this.MouseButtonCombo.SelectedValue = (long)_currentAction.MouseButton;
            }
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is ToggleMouseButtonAction)
            {
                _currentAction = (ToggleMouseButtonAction)action;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem selectedItem = (NamedItem)this.MouseButtonCombo.SelectedItem;
            if (selectedItem != null)
            {
                _currentAction = new ToggleMouseButtonAction();
                _currentAction.MouseButton = (EMouseButton)selectedItem.ID;                
            }

            return _currentAction;
        }

    }
}
