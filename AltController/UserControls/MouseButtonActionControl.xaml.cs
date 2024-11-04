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
    public partial class MouseButtonActionControl : UserControl
    {
        // Members
        private bool _isLoaded;
        private NamedItemList _mouseButtonListItems = new NamedItemList();
        private MouseButtonAction _currentAction;
        private EActionType _actionType;
        private EMouseButton _selectedMouseButton = EMouseButton.Left;
       
        // Properties
        public EMouseButton SelectedMouseButton
        {
            get 
            {
                return _selectedMouseButton;
            }
            set
            {
                _selectedMouseButton = value;                
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MouseButtonActionControl()
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

            _isLoaded = true;

            if (_currentAction != null)
            {
                DisplayAction(_currentAction);
            }
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is MouseButtonAction)
            {
                _currentAction = (MouseButtonAction)action;
                _actionType = _currentAction.ActionType;
                if (_isLoaded)
                {
                    DisplayAction(_currentAction);
                }
            }
        }

        /// <summary>
        /// Display the settings for an action
        /// </summary>
        /// <param name="action"></param>
        private void DisplayAction(MouseButtonAction action)
        {
            this.MouseButtonCombo.SelectedValue = (long)action.MouseButton;
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = new MouseButtonAction(_actionType);
            _currentAction.MouseButton = SelectedMouseButton;
            return _currentAction;
        }

        private void MouseButtonCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SelectedMouseButton = (EMouseButton)((NamedItem)e.AddedItems[0]).ID;
            }
        }
    }
}
