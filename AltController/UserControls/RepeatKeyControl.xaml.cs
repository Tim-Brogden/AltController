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
    public partial class RepeatKeyControl : UserControl
    {
        // Members
        private NamedItemList _keyListItems = new NamedItemList();
        private RepeatKeyAction _currentAction = new RepeatKeyAction();

        /// <summary>
        /// Constructor
        /// </summary>
        public RepeatKeyControl()
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
            this.KeyToPressCombo.ItemsSource = _keyListItems;

            if (_currentAction != null)
            {
                this.KeyToPressCombo.SelectedValue = _currentAction.UniqueKeyID;
                this.HoldForSlider.Value = _currentAction.PressDurationMS * 0.001;
                this.RepeatEverySlider.Value = _currentAction.RepeatEveryMS * 0.001;
                this.StopAfterSlider.Value = _currentAction.StopAfterMS * 0.001;
            }
        }

        /// <summary>
        /// Set the action to display if any
        /// </summary>
        /// <param name="action"></param>
        public void SetCurrentAction(BaseAction action)
        {
            if (action != null && action is RepeatKeyAction)
            {
                _currentAction = (RepeatKeyAction)action;
            }
        }

        /// <summary>
        /// Get the current action
        /// </summary>
        /// <returns></returns>
        public BaseAction GetCurrentAction()
        {
            _currentAction = null;

            NamedItem keyToPress = (NamedItem)this.KeyToPressCombo.SelectedItem;
            if (keyToPress != null)
            {
                _currentAction = new RepeatKeyAction();
                _currentAction.UniqueKeyID = keyToPress.ID;
                _currentAction.PressDurationMS = (int)(HoldForSlider.Value * 1000.0);
                _currentAction.RepeatEveryMS = (int)(RepeatEverySlider.Value * 1000.0);
                _currentAction.StopAfterMS = (int)(StopAfterSlider.Value * 1000.0);
            }

            return _currentAction;
        }

    }
}
