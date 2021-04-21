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
    public partial class TypeKeyControl : UserControl
    {
        // Members
        private NamedItemList _keyListItems = new NamedItemList();
        private TypeKeyAction _currentAction = new TypeKeyAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public TypeKeyControl()
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
            GUIUtils.PopulateDisplayableListWithKeys(_keyListItems);
            this.KeyboardKeyCombo.ItemsSource = _keyListItems;

            if (_currentAction != null)
            {
                this.KeyboardKeyCombo.SelectedValue = _currentAction.UniqueKeyID;
                this.AltCheck.IsChecked = _currentAction.IsAltModifierSet;
                this.ControlCheck.IsChecked = _currentAction.IsControlModifierSet;
                this.ShiftCheck.IsChecked = _currentAction.IsShiftModifierSet;
                this.WinCheck.IsChecked = _currentAction.IsWinModifierSet;
            }
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is TypeKeyAction)
            {
                _currentAction = (TypeKeyAction)action;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem keyboardKey = (NamedItem)this.KeyboardKeyCombo.SelectedItem;
            if (keyboardKey != null)
            {
                _currentAction = new TypeKeyAction();
                _currentAction.UniqueKeyID = keyboardKey.ID;
                _currentAction.IsAltModifierSet = this.AltCheck.IsChecked == true;
                _currentAction.IsControlModifierSet = this.ControlCheck.IsChecked == true;
                _currentAction.IsShiftModifierSet = this.ShiftCheck.IsChecked == true;
                _currentAction.IsWinModifierSet = this.WinCheck.IsChecked == true;
            }

            return _currentAction;
        }

    }
}
